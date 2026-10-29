using System;
using System.Collections.Generic;
using System.Threading;
using Code.Hero.Data;
using Code.UI.Elements;
using Code.UI.Windows;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.Infrastructure.State;
using CodeBase.Logic;
using CodeBase.UI.Services.Factory;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace CodeBase.CharacterIntegration
{
    public sealed class CharacterMenuService : IHeroDataProvider, ITickable, IDisposable, CodeBase.Infrastructure.IService
    {
        private readonly CharacterAssets _assets;
        private readonly IHeroDataService _hero;
        private readonly IPersistentProgressService _progress;
        private readonly ISaveLoadService _save;
        private readonly ISaveSlotService _slots;
        private readonly IGameStateMachine _states;
        private readonly IUIFactory _ui;
        private readonly LoadingCurtain _curtain;
        private readonly Dictionary<int, HeroClass> _classes = new();
        private readonly CompositeDisposable _statsBinding = new();
        private readonly CancellationTokenSource _lifetime = new();
        private CharacterMenuConfig _config;
        private GameObject _view;
        private CharacterMenuToolbar _toolbar;
        private IDisposable _viewScope, _loadingScope;
        private bool _initialized, _opening, _worldReady;
        public bool HasPendingNewGame { get; private set; }
        public event Action StatsChanged;
        public IReadOnlyDictionary<int, HeroClass> HeroClassesReadonly => _classes;

        public CharacterMenuService(CharacterAssets assets, IHeroDataService hero,
            IPersistentProgressService progress, ISaveLoadService save, IGameStateMachine states,
            IUIFactory ui, LoadingCurtain curtain, ISaveSlotService slots)
        { _assets=assets; _hero=hero; _progress=progress; _save=save; _states=states; _ui=ui; _curtain=curtain; _slots=slots; }

        public UniTask InitializeAsync() => LoadAsync(_lifetime.Token);
        public async UniTask LoadAsync(CancellationToken token)
        {
            if (_initialized) return;
            _config = Resources.Load<CharacterMenuConfig>("CharacterMenu/Config");
            if (_config == null) throw new InvalidOperationException("Не настроен CharacterMenu/Config");
            var json = await _assets.LoadAsync<TextAsset>(CharacterAssets.Classes, token);
            var data = JsonUtility.FromJson<HeroClassesRoot>(json.text);
            if (data?.heroClasses == null || data.heroClasses.Count == 0) throw new InvalidOperationException("Пустой каталог классов");
            foreach (var dto in data.heroClasses)
                if (!_classes.TryAdd(dto.id, new HeroClass(dto))) throw new InvalidOperationException("Повторный Id класса");
            await _assets.LoadAsync<GameObject>(CharacterAssets.Menu, token);
            await _assets.LoadAsync<GameObject>(CharacterAssets.Creation, token);
            await _assets.LoadAsync<GameObject>(CharacterAssets.Attributes, token);
            _slots.InitializeStorage(IsValid, _config.InitialLevel);
            _initialized = true;
        }

        public async UniTask ShowMainMenuAsync()
        {
            if (_opening) return;
            _opening=true;
            try
            {
                var prefab=await _assets.LoadAsync<GameObject>(CharacterAssets.Menu,_lifetime.Token);
                ReplaceView(UnityEngine.Object.Instantiate(prefab));
                foreach(var button in _view.GetComponentsInChildren<OpenWindowButton>(true))button.Construct(this);
                var menu=_view.GetComponent<CharacterMainMenuView>();
                var saved=_save.LoadProgress();
                menu.Bind(this,_worldReady,IsValid(saved),_hero.CurrentHero.Value!=null);
                if(!string.IsNullOrEmpty(_slots.Notice))menu.Status.text=_slots.Notice;
                _curtain.Hide();
            }
            finally{_opening=false;}
        }

        public void StartNewGame()=>OpenSlots(true);
        public void OpenSlots(bool newGame=false)
        {
            if(_opening)return;
            var go=new GameObject("SaveSlots",typeof(RectTransform));
            ReplaceView(go);
            go.AddComponent<SaveSlotsView>().Construct(_slots,_worldReady,newGame,BeginProfile,LoadSlot,
                ()=>ShowMainMenuAsync().Forget(HandleError),_classes);
        }
        private void BeginProfile(string id){_slots.PrepareNewProfile(id);OpenCreation();}
        private void LoadSlot(string id)
        {
            try
            {
                if(_worldReady && _slots.ActiveSlotId!=id)_save.SaveProgress();
                _slots.SelectSlot(id);
                HasPendingNewGame=false;
                StartLoading();_states.Enter<ReloadGameState>();
            }
            catch(Exception error){HandleError(error);}
        }
        public void OpenCreation()
        {
            if(!_slots.HasPendingProfile){OpenSlots(true);return;}
            OpenCreationAsync().Forget(HandleError);
        }
        private async UniTask OpenCreationAsync()
        {
            if(_opening)return; _opening=true;
            try
            {
                var prefab=await _assets.LoadAsync<GameObject>(CharacterAssets.Creation,_lifetime.Token);
                ReplaceView(UnityEngine.Object.Instantiate(prefab));
                var window=_view.GetComponent<NewGameWindow>();
                window.Closed+=()=>{_slots.CancelPendingProfile();ShowMainMenuAsync().Forget(HandleError);};
                window.Construct(this,_assets,_config.Balance,Commit);
            }
            finally{_opening=false;}
        }

        private void Commit(HeroData hero)
        {
            var progress=new PlayerProgress(_config.InitialLevel)
            {
                CharacterSchemaVersion=1,
                Character=CharacterProfileData.Capture(hero.Hero)
            };
            progress.HeroState.MaxHP=hero.Stats.MaxHealth.Value;
            progress.HeroState.ResetHP();
            progress.HeroStats.Damage=hero.Stats.Damage.Value;
            progress.HeroStats.DamageRadius=1;
            _progress.Progress=progress;
            _hero.SetHero(hero);
            HasPendingNewGame=true;
            StartLoading();
            _states.Enter<ReloadGameState>();
        }

        public void Continue()
        {
            if(_worldReady){Resume();return;}
            if(_slots.ActiveSlotId==null)return;
            LoadSlot(_slots.ActiveSlotId);
        }

        public bool IsValid(PlayerProgress data)
        {
            if(data?.HeroState==null || data.HeroStats==null || data.WorldData?.PositionOnLevel==null ||
               data.WorldData.LootData==null || data.KillData==null || data.PurchaseData==null ||
               !Application.CanStreamedLevelBeLoaded(data.WorldData.PositionOnLevel.Level) ||
               !Finite(data.HeroState.CurrentHP) || !Finite(data.HeroState.MaxHP) || data.HeroState.MaxHP<=0 ||
               data.HeroState.CurrentHP<0 || data.HeroState.CurrentHP>data.HeroState.MaxHP ||
               !Finite(data.HeroStats.Damage) || data.HeroStats.Damage<0 ||
               data.WorldData.LootData.Collected<0 || data.KillData.ClaeredSpawners==null ||
               data.WorldData.LootData.LootsOnGround?.Dict==null || data.PurchaseData.BoughtIAPs?.Dict==null)
                return false;
            if(data.Character==null)return true;
            if(data.CharacterSchemaVersion!=1 || !_classes.TryGetValue(data.Character.ClassId,out var definition))return false;
            try{using var restored=data.Character.Restore(definition);return true;}catch(ArgumentException){return false;}
        }
        private static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);

        public void RestoreProfile(PlayerProgress progress)
        {
            _statsBinding.Clear();
            if(progress.Character==null){_hero.SetHero(null);return;}
            if(!IsValid(progress))throw new InvalidOperationException("Сохранение персонажа несовместимо");
            var hero=new HeroData(progress.Character.Restore(_classes[progress.Character.ClassId]),_config.Balance);
            _hero.SetHero(hero);
            hero.Stats.Damage.Subscribe(v=>{progress.HeroStats.Damage=v;StatsChanged?.Invoke();}).AddTo(_statsBinding);
            hero.Stats.MaxHealth.Subscribe(v=>{progress.HeroState.MaxHP=v;progress.HeroState.CurrentHP=Mathf.Clamp(progress.HeroState.CurrentHP,0,v);StatsChanged?.Invoke();}).AddTo(_statsBinding);
        }

        private void StartLoading(){ClearView();_worldReady=false;if(_toolbar!=null)_toolbar.gameObject.SetActive(false);_loadingScope??=MenuInputScope.Acquire(false);_curtain.Show();}
        public void BeforeWorldLoad()=>StartLoading();
        public void WorldLoaded()
        {
            if(HasPendingNewGame){_save.SaveProgress();HasPendingNewGame=false;}
            _loadingScope?.Dispose();_loadingScope=null;_worldReady=true;
            if(_toolbar==null){var go=new GameObject("CharacterMenuToolbar");UnityEngine.Object.DontDestroyOnLoad(go);_toolbar=go.AddComponent<CharacterMenuToolbar>();_toolbar.Construct(this);}
            _toolbar.gameObject.SetActive(true);
        }

        public void OpenAttributes()=>OpenAttributesAsync().Forget(HandleError);
        private async UniTask OpenAttributesAsync()
        {
            if(_opening || _hero.CurrentHero.Value==null)return;_opening=true;
            try
            {
                var prefab=await _assets.LoadAsync<GameObject>(CharacterAssets.Attributes,_lifetime.Token);
                ReplaceView(UnityEngine.Object.Instantiate(prefab));
                _view.GetComponent<DemoWindow>().Construct(_hero);
                _view.GetComponent<DemoWindow>().Closed+=ClearView;
            }
            finally{_opening=false;}
        }
        public void Resume(){ClearView();}
        private void ReplaceView(GameObject next){ClearView();_view=next;UnityEngine.Object.DontDestroyOnLoad(next);_viewScope=MenuInputScope.Acquire();}
        private void ClearView(){if(_view!=null){_view.SetActive(false);UnityEngine.Object.Destroy(_view);}_view=null;_viewScope?.Dispose();_viewScope=null;}
        private void HandleError(Exception error)
        {
            if(error is OperationCanceledException)return;
            HasPendingNewGame=false;
            _slots.CancelPendingProfile();
            Debug.LogException(error);_loadingScope?.Dispose();_loadingScope=null;
            _curtain.ShowError("Не удалось загрузить игру. "+error.Message,()=>ShowMainMenuAsync().Forget(Debug.LogException));
        }
        public void LoadingFailed(Exception error)=>HandleError(error);
        public void Tick()
        {
            var keys=Keyboard.current;if(!_initialized || _opening || keys==null)return;
            if(keys.escapeKey.wasPressedThisFrame && _worldReady){if(_view!=null)Resume();else ShowMainMenuAsync().Forget(HandleError);}
            else if(keys.cKey.wasPressedThisFrame && _worldReady && _view==null)OpenAttributes();
        }
        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }
        public void Dispose()
        {
            _lifetime.Cancel();ClearView();_loadingScope?.Dispose();_statsBinding.Dispose();
            if(_toolbar!=null)UnityEngine.Object.Destroy(_toolbar.gameObject);
            foreach(var hero in _classes.Values)hero.Dispose();_classes.Clear();_lifetime.Dispose();
        }
    }
}

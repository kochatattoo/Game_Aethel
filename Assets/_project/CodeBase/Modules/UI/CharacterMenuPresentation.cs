using System;
using System.Collections.Generic;
using System.Threading;
using Code.Hero.Data;
using Code.UI.Elements;
using Code.UI.Windows;
using CodeBase.Infrastructure.Services.SaveLoad;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeBase.CharacterIntegration
{
    /// <summary>Owns views, their subscriptions and pause leases. It never changes the game state.</summary>
    public sealed class CharacterMenuPresentation : ICharacterPresentation
    {
        private readonly CharacterAssets _assets;
        private GameObject _view;
        private CharacterMenuToolbar _toolbar;
        private IDisposable _pause;
        public bool HasView => _view != null;

        public CharacterMenuPresentation(CharacterAssets assets) => _assets = assets;

        public async UniTask<string> LoadCatalogAsync(CancellationToken token) =>
            (await _assets.LoadAsync<TextAsset>(CharacterAssets.Classes, token)).text;

        public async UniTask WarmUpAsync(CancellationToken token)
        {
            await _assets.LoadAsync<GameObject>(CharacterAssets.Menu, token);
            await _assets.LoadAsync<GameObject>(CharacterAssets.Creation, token);
            await _assets.LoadAsync<GameObject>(CharacterAssets.Attributes, token);
        }

        public async UniTask ShowMenuAsync(ICharacterMenuActions actions, bool worldReady, bool canContinue,
            bool hasHero, string notice, CancellationToken token)
        {
            var prefab = await _assets.LoadAsync<GameObject>(CharacterAssets.Menu, token);
            Replace(Object.Instantiate(prefab));
            foreach (var button in _view.GetComponentsInChildren<OpenWindowButton>(true)) button.Construct(actions);
            var menu = _view.GetComponent<CharacterMainMenuView>();
            menu.Bind(actions, worldReady, canContinue, hasHero);
            if (!string.IsNullOrEmpty(notice)) menu.Status.text = notice;
        }

        public void ShowSlots(ICharacterMenuActions actions, ISaveSlotService slots, bool playing, bool newGame,
            Action<string> create, Action<string> load, IReadOnlyDictionary<int, HeroClass> classes)
        {
            Replace(new GameObject("SaveSlots", typeof(RectTransform)));
            _view.AddComponent<SaveSlotsView>().Construct(slots, playing, newGame, create, load,
                () => actions.ShowMainMenuAsync().Forget(Debug.LogException), classes);
        }

        public async UniTask ShowCreationAsync(IHeroDataProvider provider, CharacterBalance balance,
            Action<HeroData> commit, Action cancel, CancellationToken token)
        {
            var prefab = await _assets.LoadAsync<GameObject>(CharacterAssets.Creation, token);
            Replace(Object.Instantiate(prefab));
            var window = _view.GetComponent<NewGameWindow>();
            window.Closed += cancel;
            window.Construct(provider, _assets, balance, commit);
        }

        public async UniTask ShowAttributesAsync(IHeroDataService hero, CancellationToken token)
        {
            var prefab = await _assets.LoadAsync<GameObject>(CharacterAssets.Attributes, token);
            Replace(Object.Instantiate(prefab));
            var window = _view.GetComponent<DemoWindow>();
            window.Construct(hero);
            window.Closed += Clear;
        }

        public void SetToolbar(ICharacterMenuActions actions, bool visible)
        {
            if (_toolbar == null && visible)
            {
                var root = new GameObject("CharacterMenuToolbar");
                Object.DontDestroyOnLoad(root);
                _toolbar = root.AddComponent<CharacterMenuToolbar>();
                _toolbar.Construct(actions);
            }
            if (_toolbar != null) _toolbar.gameObject.SetActive(visible);
        }

        private void Replace(GameObject view)
        {
            Clear();
            _view = view;
            Object.DontDestroyOnLoad(view);
            _pause = MenuInputScope.Acquire();
        }

        public void Clear()
        {
            if (_view != null) { _view.SetActive(false); Object.Destroy(_view); }
            _view = null;
            _pause?.Dispose();
            _pause = null;
        }

        public void Dispose()
        {
            Clear();
            if (_toolbar != null) Object.Destroy(_toolbar.gameObject);
        }
    }
}

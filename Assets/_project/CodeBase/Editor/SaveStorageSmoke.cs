using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Code.Hero.Data;
using CodeBase.CharacterIntegration;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.SaveLoad;
using PetProject.SaveStorage;
using UnityEditor;
using UnityEngine;
using Zenject;
using Code.UI.Windows;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

namespace CodeBase.EditorTools
{
    public static class SaveStorageSmoke
    {
        private const string Flag="PetProject.CharacterMenu.Smoke", Key="PetProject.CharacterMigration.Smoke";
        private static ISaveSlotService Slots=>ProjectContext.Instance.Container.Resolve<ISaveSlotService>();
        private static ISaveLoadService Saves=>ProjectContext.Instance.Container.Resolve<ISaveLoadService>();

        [InitializeOnLoadMethod]
        private static void RegisterIsolation()
        {
            EditorApplication.playModeStateChanged-=OnPlayMode;
            EditorApplication.playModeStateChanged+=OnPlayMode;
        }
        private static void OnPlayMode(PlayModeStateChange state)
        {
            if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Flag,false))
            {
                AssetDatabase.DisallowAutoRefresh();EditorApplication.LockReloadAssemblies();
                SessionState.SetBool("PetProject.SaveStorage.ReloadLocked",true);
            }
            if(state==PlayModeStateChange.ExitingPlayMode && SessionState.GetBool("PetProject.SaveStorage.ReloadLocked",false))
            {
                EditorApplication.UnlockReloadAssemblies();AssetDatabase.AllowAutoRefresh();
                SessionState.SetBool("PetProject.SaveStorage.ReloadLocked",false);
            }
        }

        [MenuItem("Tools/PetProject/Save Storage/Validate Slot UI Flow")]
        public static void UIFlow()=>UIFlowAsync().Forget(Debug.LogException);
        private static async UniTask UIFlowAsync()
        {
            RequirePlay();
            var menu=ProjectContext.Instance.Container.Resolve<CharacterMenuService>();
            var before=Hash(File.ReadAllText(Path.Combine(SaveLoadService.StorageRoot,"slot-2.json")));
            menu.StartNewGame();await UniTask.Yield();
            var view=UnityEngine.Object.FindFirstObjectByType<SaveSlotsView>();
            Press(view.transform.Find("Panel/slot-2/Заменить").GetComponent<Button>());
            Press(view.transform.Find("Confirm/Dialog/Отмена").GetComponent<Button>());
            Press(view.transform.Find("Panel/slot-3/Создать").GetComponent<Button>());
            await UniTask.Yield();
            UnityEngine.Object.FindFirstObjectByType<NewGameWindow>().Close();await UniTask.Yield();
            if(Slots.HasPendingProfile || Slots.GetSlots().Single(s=>s.Id=="slot-3").State!=SaveSlotStatus.Empty ||
                before!=Hash(File.ReadAllText(Path.Combine(SaveLoadService.StorageRoot,"slot-2.json"))))throw new InvalidOperationException("Отмена изменила сохранение");
            Report("ui-cancel");
            menu.StartNewGame();await UniTask.Yield();
            view=UnityEngine.Object.FindFirstObjectByType<SaveSlotsView>();
            Press(view.transform.Find("Panel/slot-3/Создать").GetComponent<Button>());await UniTask.Yield();
            var window=UnityEngine.Object.FindFirstObjectByType<NewGameWindow>();
            window.Draft.SelectClass(1);for(var i=0;i<10;i++)if(!window.Draft.TryAllocate("str",1))throw new InvalidOperationException("Не распределены очки");
            Press((Button)new SerializedObject(window).FindProperty("_acceptButton").objectReferenceValue);
            await UniTask.WaitUntil(()=>!menu.HasPendingNewGame).Timeout(TimeSpan.FromSeconds(60), DelayType.Realtime);
            if(Slots.ActiveSlotId!="slot-3" || !Slots.GetSlots().Single(s=>s.Id=="slot-3").CanLoad)throw new InvalidOperationException("Новый герой не записан в слот 3");
            var hero=UnityEngine.Object.FindFirstObjectByType<CodeBase.Hero.HeroFacade>();
            if(hero==null || !hero.GetComponent<UnityEngine.AI.NavMeshAgent>().isOnNavMesh || Math.Abs(hero.Health.Max-50)>.001f || Math.Abs(hero.GetComponent<CodeBase.Hero.HeroAttack>().Damage-7)>.001f)throw new InvalidOperationException("Мир/позиция/характеристики не восстановлены");
            menu.OpenAttributes();Report("ui-new-profile-world");
        }
        private static void Press(Button button)
        {
            if(button==null||!button.IsInteractable())throw new InvalidOperationException("Кнопка недоступна");
            button.onClick.Invoke();
        }

        [MenuItem("Tools/PetProject/Save Storage/Run Storage Checks")]
        public static void Checks()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Остановите Play mode");
            // Execute these synchronous pure-file fixtures without Test Runner's scene save/reload.
            var type=AppDomain.CurrentDomain.GetAssemblies().First(a=>a.GetName().Name=="PetProject.SaveStorage.Tests").GetType("PetProject.SaveStorage.Tests.JsonSlotStoreTests",true);
            var tests=type.GetMethods().Where(m=>m.GetCustomAttributes(false).Any(a=>a.GetType().FullName=="NUnit.Framework.TestAttribute")).ToArray();
            var failures=new System.Collections.Generic.List<string>();
            foreach(var test in tests)
            {
                var fixture=Activator.CreateInstance(type);
                try{type.GetMethod("SetUp").Invoke(fixture,null);test.Invoke(fixture,null);}
                catch(Exception error){failures.Add(test.Name+": "+(error.InnerException??error).Message);}
                finally{type.GetMethod("TearDown").Invoke(fixture,null);}
            }
            Directory.CreateDirectory("Library/SaveStorageMigration");
            File.WriteAllText("Library/SaveStorageMigration/storage-checks.json",JsonUtility.ToJson(new CheckResult{total=tests.Length,passed=tests.Length-failures.Count,failures=failures.ToArray()},true));
            if(failures.Count>0)throw new InvalidOperationException(string.Join("\n",failures));
            AdapterChecks();
            Debug.Log("SaveStorage: "+tests.Length+"/"+tests.Length+" проверок пройдено");
        }
        private static void AdapterChecks()
        {
            var progress=new PlayerProgress("Level_2");
            progress.HeroState.MaxHP=50;progress.HeroState.CurrentHP=17;progress.HeroStats.Damage=2;
            var data=GameSaveAdapter.Capture(progress,"Level_2");
            var restored=GameSaveAdapter.Restore(JsonUtility.FromJson<GameSaveData>(JsonUtility.ToJson(data)));
            if(restored.WorldData.PositionOnLevel.Position!=null || restored.HeroState.CurrentHP!=17)throw new InvalidOperationException("Потерян признак отсутствующей позиции/текущее HP");
            progress.WorldData.PositionOnLevel.Position=new Vector3Data(1,2,3);progress.WorldData.LootData.Collected=99;
            progress.WorldData.LootData.LootsOnGround.Dict.Add("Level_2_loot",new LootObject("Level_2_loot",new Vector3(4,5,6),new Loot{Value=12}));
            progress.PurchaseData.BoughtIAPs.Dict.Add("purchase",new BoughtIAP("purchase",2));
            data=GameSaveAdapter.Capture(progress,"Level_2");restored=GameSaveAdapter.Restore(JsonUtility.FromJson<GameSaveData>(JsonUtility.ToJson(data)));
            if(restored.WorldData.PositionOnLevel.Position.X!=1 || restored.WorldData.LootData.Collected!=99 || restored.WorldData.LootData.LootsOnGround.Dict["Level_2_loot"].loot.Value!=12 || restored.PurchaseData.BoughtIAPs.Dict["purchase"].Count!=2)throw new InvalidOperationException("Потеряны данные уровня/покупок");
            data.Run.Status=RunStatus.Active;data.Run.RunId=Guid.NewGuid().ToString("N");data.Level.LevelId="Arena";
            restored=GameSaveAdapter.Restore(data);
            if(restored.WorldData.PositionOnLevel.Level!="Level_2" || restored.WorldData.PositionOnLevel.Position!=null || restored.WorldData.LootData.LootsOnGround.Dict.Count!=0)throw new InvalidOperationException("Прерванный забег не возвращает в безопасный уровень");
            data.Profile.Coins=-1;var rejected=false;
            try{GameSaveAdapter.Restore(data);}catch(ArgumentException){rejected=true;}
            if(!rejected)throw new InvalidOperationException("Отрицательная валюта принята");
            File.WriteAllText("Library/SaveStorageMigration/adapter-checks.json","{\"total\":4,\"passed\":4}");
        }
        [Serializable] private sealed class CheckResult { public int total,passed;public string[] failures; }

        [MenuItem("Tools/PetProject/Save Storage/Start Legacy Import Smoke")]
        public static void Start()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Игра уже запущена");
            SessionState.SetString("PetProject.SaveStorage.Original",Hash(PlayerPrefs.HasKey("Progress")+":"+PlayerPrefs.GetString("Progress","")));
            SessionState.SetString("PetProject.SaveStorage.PreviousTestKey",PlayerPrefs.GetString(Key,""));
            SessionState.SetBool("PetProject.SaveStorage.HadTestKey",PlayerPrefs.HasKey(Key));
            SessionState.SetString("PetProject.SaveStorage.Session",Guid.NewGuid().ToString("N"));
            var json=File.ReadAllText("Assets/_project/Source/CharacterMenu/Resources_moved/Hero/Classes/HeroClasses.json");
            var dto=JsonUtility.FromJson<HeroClassesRoot>(json).heroClasses.First(c=>c.id==1);
            using var hero=new HeroClass(dto);hero.Attributes.AddBaseValue("str",10);
            var progress=new PlayerProgress("Level_2"){CharacterSchemaVersion=1,Character=CharacterProfileData.Capture(hero)};
            progress.HeroState.MaxHP=50;progress.HeroState.ResetHP();progress.HeroStats.Damage=7;progress.HeroStats.DamageRadius=1;
            var legacy=JsonUtility.ToJson(progress);
            SessionState.SetString("PetProject.SaveStorage.Legacy",Hash(legacy));
            PlayerPrefs.SetString(Key,legacy);PlayerPrefs.Save();SessionState.SetBool(Flag,true);
            EditorApplication.isPlaying=true;
        }

        [MenuItem("Tools/PetProject/Save Storage/Validate Import")]
        public static void Import()
        {
            RequirePlay();
            var slot=Slots.GetSlots().Single(s=>s.Id=="slot-1");
            if(!slot.CanLoad||Slots.ActiveSlotId!="slot-1"||slot.Data.Profile.Character?.ClassId!=1)throw new InvalidOperationException("Legacy не импортирован");
            if(!File.Exists(Path.Combine(SaveLoadService.StorageRoot,"legacy-playerprefs.json"))||
                Hash(File.ReadAllText(Path.Combine(SaveLoadService.StorageRoot,"legacy-playerprefs.json")))!=SessionState.GetString("PetProject.SaveStorage.Legacy",""))throw new InvalidOperationException("Нет точной копии legacy");
            CheckLegacyKey();Report("legacy-import");
        }

        [MenuItem("Tools/PetProject/Save Storage/Validate Save Recovery Delete")]
        public static void Recovery()
        {
            RequirePlay();Slots.SaveToSlot("slot-2");Saves.SaveProgress();
            var path=Path.Combine(SaveLoadService.StorageRoot,"slot-2.json");
            if(!File.Exists(path+".bak"))throw new InvalidOperationException("Нет backup");
            File.WriteAllText(path,"{corrupt");
            if(Slots.GetSlots().Single(s=>s.Id=="slot-2").State!=SaveSlotStatus.Recovered)throw new InvalidOperationException("Backup не прочитан");
            Slots.SelectSlot("slot-2");
            if(Math.Abs(Saves.LoadProgress().HeroStats.Damage-7)>0.001f)throw new InvalidOperationException("Профиль изменился");
            Saves.SaveProgress();Slots.DeleteSlot("slot-1");
            if(Slots.GetSlots().Single(s=>s.Id=="slot-1").State!=SaveSlotStatus.Empty)throw new InvalidOperationException("Слот не удалён");
            if(!File.Exists(Path.Combine(SaveLoadService.StorageRoot,"legacy-playerprefs.json")))throw new InvalidOperationException("Потеряна копия legacy");
            CheckLegacyKey();Report("save-recovery-delete");
        }
        [MenuItem("Tools/PetProject/Save Storage/Open Slot Menu")]
        public static void Menu(){RequirePlay();ProjectContext.Instance.Container.Resolve<CharacterMenuService>().OpenSlots();}

        [MenuItem("Tools/PetProject/Save Storage/Restart Saved Smoke")]
        public static void Restart()
        {
            if(EditorApplication.isPlaying||!SessionState.GetBool(Flag,false))throw new InvalidOperationException("Остановите тестовый запуск");
            EditorApplication.isPlaying=true;
        }
        [MenuItem("Tools/PetProject/Save Storage/Validate Restart")]
        public static void RestartCheck()
        {
            RequirePlay();
            if(Slots.ActiveSlotId!="slot-2"||Slots.GetSlots().Single(s=>s.Id=="slot-1").State!=SaveSlotStatus.Empty)throw new InvalidOperationException("Выбор слота/маркер импорта не восстановлен");
            if(Math.Abs(Saves.LoadProgress().HeroStats.Damage-7)>0.001f)throw new InvalidOperationException("Профиль не восстановлен");
            CheckLegacyKey();Report("restart");
        }
        [MenuItem("Tools/PetProject/Save Storage/Finish Smoke")]
        public static void Finish()
        {
            if(EditorApplication.isPlaying){EditorApplication.isPlaying=false;return;}
            if(Hash(PlayerPrefs.HasKey("Progress")+":"+PlayerPrefs.GetString("Progress",""))!=SessionState.GetString("PetProject.SaveStorage.Original",""))throw new InvalidOperationException("Изменился основной Progress");
            if(SessionState.GetBool("PetProject.SaveStorage.HadTestKey",false))PlayerPrefs.SetString(Key,SessionState.GetString("PetProject.SaveStorage.PreviousTestKey",""));else PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();SessionState.SetBool(Flag,false);Report("save-isolation");
        }
        private static void CheckLegacyKey(){if(Hash(PlayerPrefs.GetString(Key,""))!=SessionState.GetString("PetProject.SaveStorage.Legacy",""))throw new InvalidOperationException("Изменён исходный legacy-ключ");}
        private static string Hash(string text){using var hash=SHA256.Create();return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(text)));}
        private static void RequirePlay(){if(!EditorApplication.isPlaying||!SessionState.GetBool(Flag,false))throw new InvalidOperationException("Нужен изолированный запуск");}
        private static void Report(string name){Directory.CreateDirectory("Library/SaveStorageMigration");File.WriteAllText("Library/SaveStorageMigration/"+name+".json","{\"success\":true}");Debug.Log("SaveStorage: "+name+" проверено");}
    }
}

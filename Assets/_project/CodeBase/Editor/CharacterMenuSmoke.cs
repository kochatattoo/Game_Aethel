using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Code.UI.Windows;
using CodeBase.CharacterIntegration;
using CodeBase.Hero;
using CodeBase.Infrastructure.Services.SaveLoad;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;
using Object=UnityEngine.Object;

namespace CodeBase.EditorTools
{
    public static class CharacterMenuSmoke
    {
        private const string Flag="PetProject.CharacterMenu.Smoke";
        private const string Key="PetProject.CharacterMigration.Smoke";
        private static CharacterMenuService Menu=>ProjectContext.Instance.Container.Resolve<CharacterMenuService>();

        [MenuItem("Tools/PetProject/Character Menu/Smoke/Diagnose")]
        public static void Diagnose()
        {
            RequirePlay();
            var container=ProjectContext.Instance.Container;
            var machine=container.Resolve<CodeBase.Infrastructure.State.IGameStateMachine>();
            var state=machine.GetType().GetField("_activeState",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)?.GetValue(machine);
            var objects=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            var lines=new System.Collections.Generic.List<string>{"State="+state?.GetType().Name,"TimeScale="+Time.timeScale,"UIRoot="+container.Resolve<CodeBase.UI.Services.Factory.IUIFactory>().Root};
            foreach(var t in objects)if(t.parent==null || t.name.Contains("Curtain") || t.name.Contains("Menu") || t.name.Contains("Coroutine"))lines.Add(t.name+" active="+t.gameObject.activeInHierarchy+" scene="+t.gameObject.scene.name);
            File.WriteAllLines("Library/CharacterMigration/runtime-diagnostic.txt",lines);
        }

        [MenuItem("Tools/PetProject/Character Menu/Smoke/Start Isolated Run")]
        public static void Start()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Игра уже запущена");
            SessionState.SetString(Flag+".Original",ProgressHash());
            SessionState.SetBool(Flag,true);PlayerPrefs.DeleteKey(Key);PlayerPrefs.Save();EditorApplication.isPlaying=true;
        }
        [MenuItem("Tools/PetProject/Character Menu/Smoke/Open Creation")]
        public static void Creation(){RequirePlay();Menu.OpenCreation();}

        [MenuItem("Tools/PetProject/Character Menu/Smoke/Commit Sample Hero")]
        public static void Commit()
        {
            RequirePlay();var window=Object.FindFirstObjectByType<NewGameWindow>();if(window==null)throw new InvalidOperationException("Нет окна создания");
            window.Draft.SelectClass(1);for(var i=0;i<10;i++)if(!window.Draft.TryAllocate("str",1))throw new InvalidOperationException("Не распределены очки");
            var serialized=new SerializedObject(window);((Button)serialized.FindProperty("_acceptButton").objectReferenceValue).onClick.Invoke();
        }
        [MenuItem("Tools/PetProject/Character Menu/Smoke/Open Attributes")]
        public static void Attributes(){RequirePlay();Menu.OpenAttributes();}

        [MenuItem("Tools/PetProject/Character Menu/Smoke/Restart Saved Run")]
        public static void Restart()
        {
            if(EditorApplication.isPlaying||!SessionState.GetBool(Flag,false)||!PlayerPrefs.HasKey(Key))throw new InvalidOperationException("Нужно завершить Play mode с тестовым сохранением");
            EditorApplication.isPlaying=true;
        }
        [MenuItem("Tools/PetProject/Character Menu/Smoke/Continue Saved Run")]
        public static void ContinueSaved(){RequirePlay();Menu.Continue();}
        [MenuItem("Tools/PetProject/Character Menu/Smoke/Resume")]
        public static void Resume(){RequirePlay();Menu.Resume();}

        [MenuItem("Tools/PetProject/Character Menu/Smoke/Validate Runtime")]
        public static void Validate()
        {
            RequirePlay();var hero=Object.FindFirstObjectByType<HeroFacade>();if(hero==null)throw new InvalidOperationException("Игровой герой не создан");
            var attack=hero.GetComponent<HeroAttack>();var data=ProjectContext.Instance.Container.Resolve<ISaveLoadService>().LoadProgress();
            if(data?.Character==null||data.Character.ClassId!=1||Math.Abs(attack.Damage-7)>0.001f||Math.Abs(hero.Health.Max-50)>0.001f)throw new InvalidOperationException("HP/урон/сохранение не совпадают с профилем");
            if(Object.FindObjectsByType<DemoWindow>(FindObjectsSortMode.None).Length!=1)throw new InvalidOperationException("Нет единственного окна атрибутов");
            Directory.CreateDirectory("Library/CharacterMigration");File.WriteAllText("Library/CharacterMigration/runtime-validation.json",JsonUtility.ToJson(new Result{success=true,scene=SceneManager.GetActiveScene().name,classId=data.Character.ClassId,damage=attack.Damage,maxHealth=hero.Health.Max},true));
            Debug.Log("Проверка CharacterMenu пройдена: герой, атрибуты, сохранение и игровой урон согласованы");
        }
        [Serializable] private class Result{public bool success;public string scene;public int classId;public float damage,maxHealth;}
        [MenuItem("Tools/PetProject/Character Menu/Smoke/Finish Isolated Run")]
        public static void Finish()
        {
            if(EditorApplication.isPlaying){EditorApplication.isPlaying=false;return;}
            SessionState.SetBool(Flag,false);PlayerPrefs.DeleteKey(Key);PlayerPrefs.Save();
            if(SessionState.GetString(Flag+".Original","")!=ProgressHash())throw new InvalidOperationException("Основное сохранение изменилось во время smoke run");
            File.WriteAllText("Library/CharacterMigration/save-isolation-validation.json","{\"success\":true,\"originalProgressUnchanged\":true,\"smokeSaveRemoved\":true}");
        }
        private static void RequirePlay(){if(!EditorApplication.isPlaying||!SessionState.GetBool(Flag,false))throw new InvalidOperationException("Нужен изолированный smoke run");}
        private static string ProgressHash(){using var sha=SHA256.Create();return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(PlayerPrefs.HasKey("Progress")+":"+PlayerPrefs.GetString("Progress", ""))));}
    }
}

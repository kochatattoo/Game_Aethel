using System;
using System.IO;
using System.Linq;
using Code.UI.Windows;
using CodeBase.CharacterIntegration;
using CodeBase.DI;
using TMPro;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CodeBase.EditorTools
{
    public static class CharacterMenuMigration
    {
        private const string Root="Assets/_project/Source/CharacterMenu";
        private const string Menu=Root+"/Resources_moved/SceneAssets/Menu.prefab";
        private const string Creation=Root+"/Resources/UI/Windows/NewGameWindow.prefab";
        private const string Attributes=Root+"/Resources/UI/Windows/DemoSceneWindow.prefab";
        private const string Curtain=Root+"/Resources/SceneAssets/LoadingCurtain.prefab";

        [MenuItem("Tools/PetProject/Character Menu/Configure Imported Assets")]
        public static void Configure()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Остановите игру перед настройкой");
            var settings=AddressableAssetSettingsDefaultObject.Settings;
            if(settings==null)throw new InvalidOperationException("Не настроены Addressables");
            var group=settings.FindGroup("CharacterMenu")??settings.CreateGroup("CharacterMenu",false,false,false,null,typeof(BundledAssetGroupSchema),typeof(ContentUpdateGroupSchema));
            var schema=group.GetSchema<BundledAssetGroupSchema>();
            schema.BuildPath.SetVariableByName(settings,"Local.BuildPath");schema.LoadPath.SetVariableByName(settings,"Local.LoadPath");
            Register(settings,group,Menu,CharacterAssets.Menu);Register(settings,group,Creation,CharacterAssets.Creation);Register(settings,group,Attributes,CharacterAssets.Attributes);
            Register(settings,group,Root+"/Resources_moved/Hero/Classes/HeroClasses.json",CharacterAssets.Classes);
            foreach(var guid in AssetDatabase.FindAssets("t:Sprite",new[]{Root+"/Icons"}))
            {
                var path=AssetDatabase.GUIDToAssetPath(guid);var name=Path.GetFileNameWithoutExtension(path);
                Register(settings,group,path,"PetProject/HeroIcons/"+(name=="Fighter"?"Warrior":name));
            }
            ConfigureMenu();ConfigureWindow(Creation);ConfigureWindow(Attributes);
            if(!AssetDatabase.IsValidFolder("Assets/_project/Resources/CharacterMenu"))AssetDatabase.CreateFolder("Assets/_project/Resources","CharacterMenu");
            const string configPath="Assets/_project/Resources/CharacterMenu/Config.asset";
            if(AssetDatabase.LoadAssetAtPath<CharacterMenuConfig>(configPath)==null)AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<CharacterMenuConfig>(),configPath);
            const string contextPath="Assets/_project/Resources/ProjectContext.prefab";
            Directory.CreateDirectory("Library/CharacterMigration/backup");
            const string contextBackup="Library/CharacterMigration/backup/ProjectContext.prefab";
            if(!File.Exists(contextBackup))File.Copy(contextPath,contextBackup);
            var context=PrefabUtility.LoadPrefabContents(contextPath);
            try
            {
                var installer=context.GetComponentInChildren<BootstrapInstaller>(true);
                if(installer==null)throw new InvalidOperationException("Не найден BootstrapInstaller");
                installer.LoadingCurtain=AssetDatabase.LoadAssetAtPath<GameObject>(Curtain);
                InstallGlobalEffects(context);
                PrefabUtility.SaveAsPrefabAsset(context,contextPath);
            }
            finally{PrefabUtility.UnloadPrefabContents(context);}
            RemoveSceneEffects();
            EditorUtility.SetDirty(settings);AssetDatabase.SaveAssets();
            Validate();
            Debug.Log("Перенос CharacterMenu: ассеты и Addressables настроены");
        }

        private static void InstallGlobalEffects(GameObject context)
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_project/Resources/SceneContext.prefab");
            var sourceSound=source.GetComponent<global::Infrastructure.DI.SoundInstaller>();
            var sourceVfx=source.GetComponent<global::Infrastructure.DI.VFXInstaller>();
            var sound=context.GetComponent<global::Infrastructure.DI.SoundInstaller>();
            var vfx=context.GetComponent<global::Infrastructure.DI.VFXInstaller>();
            if(sound==null)
            {
                if(sourceSound==null)throw new InvalidOperationException("Нет исходного SoundInstaller");
                sound=context.AddComponent<global::Infrastructure.DI.SoundInstaller>();
                EditorUtility.CopySerialized(sourceSound,sound);
            }
            if(vfx==null)
            {
                if(sourceVfx==null)throw new InvalidOperationException("Нет исходного VFXInstaller");
                vfx=context.AddComponent<global::Infrastructure.DI.VFXInstaller>();
                EditorUtility.CopySerialized(sourceVfx,vfx);
            }
            var project=new SerializedObject(context.GetComponent<Zenject.ProjectContext>());
            project.FindProperty("_parentNewObjectsUnderContext").boolValue=true;
            var installers=project.FindProperty("_monoInstallers");
            foreach(var component in new Component[]{sound,vfx})
            {
                var found=false;for(var i=0;i<installers.arraySize;i++)if(installers.GetArrayElementAtIndex(i).objectReferenceValue==component)found=true;
                if(!found){var index=installers.arraySize++;installers.GetArrayElementAtIndex(index).objectReferenceValue=component;}
            }
            project.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void RemoveSceneEffects()
        {
            const string scenePath="Assets/_project/Resources/SceneContext.prefab";
            const string backup="Library/CharacterMigration/backup/SceneContext.prefab";
            if(!File.Exists(backup))File.Copy(scenePath,backup);
            var scene=PrefabUtility.LoadPrefabContents(scenePath);
            try
            {
                var serial=new SerializedObject(scene.GetComponent<Zenject.SceneContext>());var list=serial.FindProperty("_monoInstallers");
                for(var i=list.arraySize-1;i>=0;i--)
                {
                    var component=list.GetArrayElementAtIndex(i).objectReferenceValue;
                    if(component is global::Infrastructure.DI.SoundInstaller || component is global::Infrastructure.DI.VFXInstaller)
                    {list.GetArrayElementAtIndex(i).objectReferenceValue=null;list.DeleteArrayElementAtIndex(i);}
                }
                serial.ApplyModifiedPropertiesWithoutUndo();
                var sound=scene.GetComponent<global::Infrastructure.DI.SoundInstaller>();if(sound!=null)Object.DestroyImmediate(sound);
                var vfx=scene.GetComponent<global::Infrastructure.DI.VFXInstaller>();if(vfx!=null)Object.DestroyImmediate(vfx);
                PrefabUtility.SaveAsPrefabAsset(scene,scenePath);
            }
            finally{PrefabUtility.UnloadPrefabContents(scene);}
        }

        private static void Register(AddressableAssetSettings settings,AddressableAssetGroup group,string path,string address)
        {
            var guid=AssetDatabase.AssetPathToGUID(path);if(string.IsNullOrEmpty(guid))throw new InvalidOperationException("Нет ассета "+path);
            var collision=settings.FindAssetEntry(guid);if(collision!=null && collision.address!=address)throw new InvalidOperationException("Ассет уже имеет другой адрес: "+path);
            var entry=settings.CreateOrMoveEntry(guid,group);entry.address=address;
        }
        private static void ConfigureMenu()
        {
            var root=PrefabUtility.LoadPrefabContents(Menu);
            try
            {
                SetCanvas(root);
                foreach(var nested in root.GetComponentsInChildren<Canvas>(true))
                    if(nested.gameObject!=root){nested.overrideSorting=false;}
                var view=root.GetComponent<CharacterMainMenuView>();if(view==null)view=root.AddComponent<CharacterMainMenuView>();
                var all=root.GetComponentsInChildren<Transform>(true);
                var newGame=all.First(t=>t.name=="NewGame_Button").GetComponent<Button>();
                var exit=all.First(t=>t.name=="Exit_Button").GetComponent<Button>();
                newGame.GetComponentInChildren<TMP_Text>().text="Новая игра";exit.GetComponentInChildren<TMP_Text>().text="Выход";
                var parent=newGame.transform.parent;
                parent.SetParent(root.transform,false);
                var continueButton=FindOrClone(parent,newGame,"Continue_Button","Продолжить");
                var attributesButton=FindOrClone(parent,newGame,"Attributes_Button","Атрибуты");
                newGame.transform.SetSiblingIndex(0);continueButton.transform.SetSiblingIndex(1);attributesButton.transform.SetSiblingIndex(2);exit.transform.SetSiblingIndex(3);
                var layout=parent.GetComponent<VerticalLayoutGroup>();if(layout==null)layout=parent.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.spacing=16;layout.childAlignment=TextAnchor.MiddleCenter;layout.childControlHeight=true;layout.childControlWidth=true;layout.childForceExpandHeight=false;layout.childForceExpandWidth=true;
                var rect=(RectTransform)parent;rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=Vector2.zero;rect.sizeDelta=new Vector2(540,430);
                foreach(Transform child in parent)if(child.GetComponent<Button>()==null)child.gameObject.SetActive(false);
                foreach(var button in new[]{newGame,continueButton,attributesButton,exit}){var element=button.GetComponent<LayoutElement>();if(element==null)element=button.gameObject.AddComponent<LayoutElement>();element.preferredHeight=82;element.minHeight=82;var caption=button.GetComponentInChildren<TMP_Text>();caption.color=Color.white;caption.margin=Vector4.zero;}
                view.ContinueButton=continueButton;view.AttributesButton=attributesButton;
                var status=all.FirstOrDefault(t=>t.name=="MenuStatus");
                if(status==null)
                {
                    var go=new GameObject("MenuStatus",typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(root.transform,false);status=go.transform;
                    var sr=(RectTransform)status;sr.anchorMin=sr.anchorMax=new Vector2(.5f,.5f);sr.sizeDelta=new Vector2(900,80);sr.anchoredPosition=new Vector2(0,-310);
                }
                var label=status.GetComponent<TextMeshProUGUI>();label.font=newGame.GetComponentInChildren<TMP_Text>().font;label.fontSize=28;label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;view.Status=label;
                PrefabUtility.SaveAsPrefabAsset(root,Menu);
            }
            finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        private static Button FindOrClone(Transform parent,Button source,string name,string caption)
        {
            var found=parent.Find(name);var button=found!=null?found.GetComponent<Button>():Object.Instantiate(source,parent);
            button.name=name;
            foreach(var component in button.GetComponents<Code.UI.Elements.OpenWindowButton>())Object.DestroyImmediate(component);
            button.onClick=new Button.ButtonClickedEvent();button.GetComponentInChildren<TMP_Text>().text=caption;return button;
        }
        private static void ConfigureWindow(string path)
        {
            var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                SetCanvas(root);
                foreach(var t in root.GetComponentsInChildren<Transform>(true))if(t.name=="DemoElements")t.gameObject.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        private static void SetCanvas(GameObject root)
        {
            var canvas=root.GetComponent<Canvas>();if(canvas==null)canvas=root.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.overrideSorting=true;canvas.sortingOrder=200;
            if(root.GetComponent<GraphicRaycaster>()==null)root.AddComponent<GraphicRaycaster>();
            var scaler=root.GetComponent<CanvasScaler>();if(scaler==null)scaler=root.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
        }

        [MenuItem("Tools/PetProject/Character Menu/Validate Imported Assets")]
        public static void Validate()
        {
            foreach(var path in new[]{Menu,Creation,Attributes,Curtain})
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(prefab==null)throw new InvalidOperationException("Нет префаба "+path);
                foreach(var t in prefab.GetComponentsInChildren<Transform>(true))if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0)throw new InvalidOperationException("Missing Script: "+path+"/"+t.name);
            }
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Menu).GetComponent<CharacterMainMenuView>();
            if(root==null||root.ContinueButton==null||root.AttributesButton==null||root.Status==null)throw new InvalidOperationException("Не настроено главное меню");
            Directory.CreateDirectory("Library/CharacterMigration");File.WriteAllText("Library/CharacterMigration/assets-validation.json","{\"success\":true,\"prefabs\":4}");
        }
    }
}

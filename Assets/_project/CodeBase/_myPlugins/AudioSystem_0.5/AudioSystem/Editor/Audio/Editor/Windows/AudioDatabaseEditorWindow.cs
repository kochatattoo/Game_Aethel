using UnityEditor;
using UnityEngine;
using Infrastructure.AudioSystem.Parameters;
using Infrastructure.AudioSystem.Components.MaterialConfigs;
using Infrastructure.AudioSystem.Events;
using System.Collections.Generic;
using System.Linq;

namespace Audio.Editor.Windows
{
    public class WwiseAudioToolsWindow : EditorWindow
    {
        private const string RelativePropertyPath = "_material";
        private const string PropertyPath = "_materials";

        private AudioDatabase _database;
        private WwiseAudioConfig _config;
        private WwiseToMaterialConfig _materialConfig;

        private UnityEditor.Editor _databaseEditor;
        private UnityEditor.Editor _configEditor;
        private UnityEditor.Editor _materialEditor;

        private int _tabIndex = 0;
        private Vector2 _scrollPos;
        private Vector2 _explorerScrollPos;

        private List<AudioEventAsset> _eventAssets = new();
        private List<AudioParameterAsset> _paramAssets = new();
        private List<AudioSwitchAsset> _switchAssets = new();
        private List<AudioStateAsset> _stateAssets = new();
        private List<WwiseBankAsset> _bankAssets = new();
        private List<SurfaceAudioKeyResolverConfig> _resolverConfigs = new();
        private List<SurfaceWwiseSwitchResolverConfig> _resolverMaterialConfigs = new();

        [MenuItem("Tools/Audio/Wwise Audio Tools %&W")] // Alt+Shift+W
        public static void ShowWindow() => GetWindow<WwiseAudioToolsWindow>("Wwise Audio Tools");

        private void OnEnable() => RefreshAssets();

        private void OnGUI()
        {
            _tabIndex = GUILayout.Toolbar(_tabIndex, new string[] { "Audio Database", "Wwise Global Config", "Material Global Config", "Asset Explorer" });

            EditorGUILayout.Space(5);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            switch (_tabIndex)
            {
                case 0:
                    DrawEditor(ref _database, ref _databaseEditor);
                break;

                case 1:
                    DrawEditor(ref _config, ref _configEditor);
                break;

                case 2:
                    DrawEditor(ref _materialConfig, ref _materialEditor);
                break;

                default:
                    DrawAssetExplorer();
                break;
            }
            EditorGUILayout.EndScrollView();
        }

        private void RefreshAssets()
        {
            _database = FindAsset<AudioDatabase>();
            _config = FindAsset<WwiseAudioConfig>();
            _materialConfig = FindAsset<WwiseToMaterialConfig>();

            _eventAssets = FindAllAssets<AudioEventAsset>();
            _paramAssets = FindAllAssets<AudioParameterAsset>();
            _switchAssets = FindAllAssets<AudioSwitchAsset>();
            _stateAssets = FindAllAssets<AudioStateAsset>();
            _bankAssets = FindAllAssets<WwiseBankAsset>();
            _resolverConfigs = FindAllAssets<SurfaceAudioKeyResolverConfig>();
            _resolverMaterialConfigs = FindAllAssets<SurfaceWwiseSwitchResolverConfig>();

        }

        private T FindAsset<T>() where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<T>(path);
            }
            return null;
        }

        private List<T> FindAllAssets<T>() where T : ScriptableObject
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(a => a != null)
                .ToList();
        }

        private void DrawEditor<T>(ref T asset, ref UnityEditor.Editor cachedEditor) where T : ScriptableObject
        {
            if (asset == null)
            {
                EditorGUILayout.HelpBox($"{typeof(T).Name} не найден!", MessageType.Warning);
                if (GUILayout.Button($"Создать {typeof(T).Name}")) 
                    CreateAsset<T>(out asset);
                return;
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Editing: {asset.name}", EditorStyles.boldLabel);
            if (GUILayout.Button("Ping", GUILayout.Width(50))) 
                EditorGUIUtility.PingObject(asset);
            if (GUILayout.Button("Refresh", GUILayout.Width(60))) 
                RefreshAssets();
            EditorGUILayout.EndHorizontal();

            if (cachedEditor == null || cachedEditor.target != asset)
            {
                if (cachedEditor != null) DestroyImmediate(cachedEditor);
                cachedEditor = UnityEditor.Editor.CreateEditor(asset);
            }

            SerializedObject so = cachedEditor.serializedObject;
            SerializedProperty materialsProp = so.FindProperty(PropertyPath);

            if (materialsProp != null && materialsProp.isArray)
            {
                DrawMaterialsList(so, materialsProp);
            }
            else
            {
                cachedEditor.OnInspectorGUI();
            }
        }

        private void DrawMaterialsList(SerializedObject so, SerializedProperty listProp)
        {
            so.Update(); 

            for (int i = 0; i < listProp.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();


                EditorGUILayout.PropertyField(listProp.GetArrayElementAtIndex(i), true);

                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    if (IsItemFilled(listProp.GetArrayElementAtIndex(i)))
                    {
                        if (EditorUtility.DisplayDialog(
                            "Удаление элемента",
                            "Этот слот содержит настройки. Вы уверены, что хотите его удалить?",
                            "Да, удалить",
                            "Отмена"))
                        {
                            RemoveItem(listProp, i);
                        }
                    }
                    else
                    {
                        RemoveItem(listProp, i);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            so.ApplyModifiedProperties();
        }

        private bool IsItemFilled(SerializedProperty itemProp)
        {
            var materialProp = itemProp.FindPropertyRelative(RelativePropertyPath);
            return materialProp != null && materialProp.objectReferenceValue != null;
        }

        private void RemoveItem(SerializedProperty list, int index)
        {
            list.DeleteArrayElementAtIndex(index);

            if (list.GetArrayElementAtIndex(index) == null)
                list.DeleteArrayElementAtIndex(index);
        }

        private void DrawAssetExplorer()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Refresh All Assets", EditorStyles.toolbarButton)) RefreshAssets();
            EditorGUILayout.EndHorizontal();

            _explorerScrollPos = EditorGUILayout.BeginScrollView(_explorerScrollPos);

            DrawAssetGroup("Audio Events", _eventAssets);
            DrawAssetGroup("Audio Parameters", _paramAssets);
            DrawAssetGroup("Audio Switches", _switchAssets);
            DrawAssetGroup("Audio States", _stateAssets);
            DrawAssetGroup("Audio Banks", _bankAssets);
            DrawAssetGroup("Surface Resolvers", _resolverConfigs);
            DrawAssetGroup("Surface Material Resolvers", _resolverMaterialConfigs);

            EditorGUILayout.EndScrollView();
        }

        private void DrawAssetGroup<T>(string title, List<T> assets) where T : Object
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            if (assets.Count == 0) EditorGUILayout.HelpBox("Ассеты не найдены", MessageType.None);

            foreach (var asset in assets)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                if (GUILayout.Button("P", GUILayout.Width(20))) EditorGUIUtility.PingObject(asset);

                EditorGUILayout.ObjectField(asset, typeof(T), false);

                GUIContent settingsIcon = EditorGUIUtility.IconContent("SettingsIcon");

                if (GUILayout.Button(settingsIcon, EditorStyles.iconButton, GUILayout.Width(25)))
                {
                    OpenAssetInNewInspector(asset);
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space(10);
        }

        private void CreateAsset<T>(out T asset) where T : ScriptableObject
        {
            asset = ScriptableObject.CreateInstance<T>();
            string path = $"Assets/Settings/{typeof(T).Name}.asset";

            if (!System.IO.Directory.Exists("Assets/Settings")) 
                System.IO.Directory.CreateDirectory("Assets/Settings");

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
        }

        private void OpenAssetInNewInspector(Object asset)
        {
            if (asset == null) 
                return;

            Selection.activeObject = asset;

            EditorApplication.ExecuteMenuItem("Assets/Properties...");
        }

        private void OnDestroy()
        {
            if (_databaseEditor != null) 
                DestroyImmediate(_databaseEditor);
            if (_configEditor != null) 
                DestroyImmediate(_configEditor);
            if(_materialEditor != null) 
                DestroyImmediate(_materialEditor);
        }
    }
}
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VFXSystem.Parameters.Definition;
using VFXSystem.Parameters.MaterialMap;
using VFXSystem.Parameters.Settings;
using VFXSystem.Parameters;
using System.Linq;

namespace VFXSystem.Editor
{
    public class VFXEditorWindow : EditorWindow
    {
        private const string MaterialMapPath = "_materialConfigs";
        private const string DEFAULT_PATH = "Assets/Settings/VFX";
        private string _customPath = DEFAULT_PATH;

        private VFXDatabase _database;
        private VFXRestrictionSettings _restrictions;
        private VFXDefinitionsMapConfigs _definitionsMap;
        private VFXMapToMaterialConfigs _materialMap;
        private VFXQualitySettings _qualitySettings;

        private UnityEditor.Editor _databaseEditor;
        private UnityEditor.Editor _restrictionsEditor;
        private UnityEditor.Editor _definitionsMapEditor;
        private UnityEditor.Editor _materialMapEditor;
        private UnityEditor.Editor _qualityEditor;

        private int _tabIndex = 0;
        private Vector2 _scrollPos;
        private Vector2 _explorerScrollPos;

        private List<HitVFXDefinition> _hitDefinitions = new();

        [MenuItem("Tools/VFX System/VFX Editor Window %&V")] // Alt+Shift+V
        public static void ShowWindow() => GetWindow<VFXEditorWindow>("VFX Editor");

        private void OnEnable() => RefreshAssets();

        private void OnGUI()
        {
            _tabIndex = GUILayout.Toolbar(_tabIndex, new string[]
            {
            "Database",
            "Restrictions",
            "Maps & Materials",
            "Quality",
            "Hit Explorer"
            });

            EditorGUILayout.Space(5);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            switch (_tabIndex)
            {
                case 0: DrawEditor(ref _database, ref _databaseEditor); break;
                case 1: DrawEditor(ref _restrictions, ref _restrictionsEditor); break;
                case 2: DrawDoubleMapEditor(); break;
                case 3: DrawEditor(ref _qualitySettings, ref _qualityEditor); break;
                case 4: DrawHitExplorer(); break;
            }

            EditorGUILayout.EndScrollView();
        }

        private void RefreshAssets()
        {
            _database = FindAsset<VFXDatabase>();
            _restrictions = FindAsset<VFXRestrictionSettings>();
            _definitionsMap = FindAsset<VFXDefinitionsMapConfigs>();
            _materialMap = FindAsset<VFXMapToMaterialConfigs>();
            _qualitySettings = FindAsset<VFXQualitySettings>();

            _hitDefinitions = FindAllAssets<HitVFXDefinition>();
        }

        private void DrawDoubleMapEditor()
        {
            EditorGUILayout.HelpBox("Настройка связей: MaterialType -> VFXDefinition и Enum -> Material", MessageType.Info);

            DrawEditor(ref _definitionsMap, ref _definitionsMapEditor, "VFX Definitions Map");
            EditorGUILayout.Space(15);
            DrawEditor(ref _materialMap, ref _materialMapEditor, "Material to Enum Mapping");
        }

        private void DrawHitExplorer()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Refresh Hit Definitions", EditorStyles.toolbarButton)) RefreshAssets();
            EditorGUILayout.EndHorizontal();

            _explorerScrollPos = EditorGUILayout.BeginScrollView(_explorerScrollPos);
            DrawAssetGroup("All Hit Definitions", _hitDefinitions);
            EditorGUILayout.EndScrollView();
        }

        #region Core Engine

        private void DrawEditor<T>(ref T asset, ref UnityEditor.Editor cachedEditor, string overrideTitle = "") where T : ScriptableObject
        {
            if (asset == null)
            {
                EditorGUILayout.HelpBox($"{typeof(T).Name} не найден!", MessageType.Warning);
                if (GUILayout.Button($"Создать {typeof(T).Name}")) CreateAsset<T>(out asset);
                return;
            }

            string title = string.IsNullOrEmpty(overrideTitle) ? asset.name : overrideTitle;

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            if (GUILayout.Button("Ping", GUILayout.Width(50))) EditorGUIUtility.PingObject(asset);
            if (GUILayout.Button("Refresh", GUILayout.Width(60))) RefreshAssets();
            EditorGUILayout.EndHorizontal();

            if (cachedEditor == null || cachedEditor.target != asset)
            {
                if (cachedEditor != null) DestroyImmediate(cachedEditor);
                cachedEditor = UnityEditor.Editor.CreateEditor(asset);
            }

            cachedEditor.OnInspectorGUI();
        }

        private void DrawAssetGroup<T>(string title, List<T> assets) where T : Object
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            if (assets == null || assets.Count == 0) EditorGUILayout.HelpBox("Ассеты не найдены", MessageType.None);
            else
            {
                foreach (var asset in assets)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    if (GUILayout.Button("P", GUILayout.Width(20))) EditorGUIUtility.PingObject(asset);
                    EditorGUILayout.ObjectField(asset, typeof(T), false);

                    if (GUILayout.Button(EditorGUIUtility.IconContent("SettingsIcon"), EditorStyles.iconButton, GUILayout.Width(25)))
                        Selection.activeObject = asset; 

                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.Space(10);
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

        private void CreateAsset<T>(out T asset) where T : ScriptableObject
        {

            if (!AssetDatabase.IsValidFolder(_customPath))
            {
                System.IO.Directory.CreateDirectory(_customPath);
                AssetDatabase.Refresh();
            }

            asset = CreateInstance<T>();

            string fullPath = AssetDatabase.GenerateUniqueAssetPath($"{_customPath}/{typeof(T).Name}.asset");

            AssetDatabase.CreateAsset(asset, fullPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"<color=green>VFX System:</color> Created {typeof(T).Name} at {fullPath}");
        }

        private void DrawPathSelector()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Save Path:", GUILayout.Width(70));
            _customPath = EditorGUILayout.TextField(_customPath);

            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select VFX Settings Folder", "Assets", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    if (selectedPath.Contains(Application.dataPath))
                        _customPath = "Assets" + selectedPath.Replace(Application.dataPath, "");
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        #endregion
    }
}

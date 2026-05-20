using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GoogleSheetConfig
{
    public class GoogleSheetImporterWindow : OdinEditorWindow
    {
        [LabelText("Spread Sheet Key")] 
        public string spreadSheetKey = "1m8E1DYkc8yNX5G9Qfl2NTMeJWxvT4qqh-c9SJa0yBlE";

        [TableList(IsReadOnly = false, ShowPaging = false, AlwaysExpanded = true)]
        public List<ParserEntry> parserEntries;
        
        protected override void OnEnable()
        {
            base.OnEnable();

            if (parserEntries == null || parserEntries.Count == 0)
            {
                parserEntries = ParserConfig.SettingsParsers
                    .Select(settingsParser => new ParserEntry
                    {
                        NeedLoaded = true,
                        Parser = settingsParser,
                        Sheet = settingsParser.SheetName
                    })
                    .ToList();
            }
        }

        [HorizontalGroup("HeaderButtons", 0.5f)]
        [Button("Enable all parser", ButtonHeight = 30)]
        public void EnableAllParserHandler()
        {
            foreach (var parserEntry in parserEntries)
            {
                parserEntry.NeedLoaded = true;
            }
        }

        [HorizontalGroup("HeaderButtons", 0.5f)]
        [Button("Disable all parser", ButtonHeight = 30)]
        public void DisableAllParserHandler()
        {
            foreach (var parserEntry in parserEntries)
            {
                parserEntry.NeedLoaded = false;
            }
        }

        [Button("Parse Enabled Sheets", ButtonHeight = 30)]
        public void ParseEnabledSheetsHandler()
        {
            var enabledParsers = parserEntries
                .Where(parserEntry => parserEntry.NeedLoaded)
                .Select(parserEntry => parserEntry.Parser)
                .ToArray();

            if (enabledParsers.Length == 0)
            {
                Debug.LogError("No parser found");
                return;
            }

            try
            {
                AssetDatabase.StartAssetEditing();
            
                var googleSheetLoader = new GoogleSheetLoader();
                googleSheetLoader.Init();

                var sheetNames = enabledParsers.Select(parser => parser.SheetName).ToArray();
                var jsonBySheet = googleSheetLoader.DownloadSheets(spreadSheetKey, sheetNames);

                foreach (var pair in jsonBySheet)
                {
                    var sheetName = pair.Key;
                    var parser = enabledParsers.First(parser => parser.SheetName == sheetName);

                    var jArray = pair.Value;
                    parser.Parse(jArray);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
                throw;
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Close();
            }
        }

        [MenuItem("Utility/Google Sheet Importer Window")]
        private static void OpenWindow()
        {
            var window = GetWindow<GoogleSheetImporterWindow>();
            window.titleContent = new GUIContent("Google Sheet Importer Window");
            window.minSize = new Vector2(600, 450);
            window.position = new Rect(400, 200, 700, 500);
            window.Show();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace GoogleSheetConfig
{
    public class GoogleSheetLoader
    {
        static string CLIENT_ID = GoogleSecret.CLIENT_ID;

        static string CLIENT_SECRET = GoogleSecret.CLIENT_SECRET;

        static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };

        /// <summary>
        /// Name of application.
        /// </summary>
        private string appName = "Aethel";

        /// <summary>
        /// The root of spreadsheet's url.
        /// </summary>
        private string urlRoot = "https://spreadsheets.google.com/feeds/spreadsheets/";

        /// <summary>
        /// Progress of download and convert action. 100 is "completed".
        /// </summary>
        private float progress = 100;

        /// <summary>
        /// The message which be shown on progress bar when action is running.
        /// </summary>
        private string progressMessage = "";

        public void Init()
        {
            progress = 100;
            progressMessage = "";
            ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
        }
        
        public Dictionary<string, JArray> DownloadSheets(string spreadSheetKey, string[] wantedSheetNames)
        {
            //Validate input
            if (string.IsNullOrEmpty(spreadSheetKey))
            {
                Debug.LogError("spreadSheetKey can not be null!");
                return null;
            }

            Debug.Log("Start downloading google sheets from key: " + spreadSheetKey);

            //Authenticate
            progressMessage = "Authenticating...";
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = GetCredential(),
                ApplicationName = appName,
            });

            progress = 5;
            EditorUtility.DisplayCancelableProgressBar("Processing", progressMessage, progress / 100);
            progressMessage = "Get list of spreadsheets...";
            EditorUtility.DisplayCancelableProgressBar("Processing", progressMessage, progress / 100);

            Spreadsheet spreadSheetData = service.Spreadsheets.Get(spreadSheetKey).Execute();
            IList<Sheet> sheets = spreadSheetData.Sheets;
            
            //if((feed == null)||(feed.Entries.Count <= 0))
            if ((sheets == null) || (sheets.Count <= 0))
            {
                Debug.LogError("Not found any data!");
                progress = 100;
                EditorUtility.ClearProgressBar();
                return null;
            }

            progress = 15;

            //For each sheet in received data, check the sheet name. If that sheet is the wanted sheet, add it into the ranges.
            List<string> ranges = new List<string>();
            foreach (Sheet sheet in sheets)
            {
                if ((wantedSheetNames.Length <= 0) || (wantedSheetNames.Contains(sheet.Properties.Title)))
                {
                    ranges.Add(sheet.Properties.Title);
                }
            }

            SpreadsheetsResource.ValuesResource.BatchGetRequest request =
                service.Spreadsheets.Values.BatchGet(spreadSheetKey);
            request.Ranges = ranges;
            BatchGetValuesResponse response = request.Execute();

            var jsonDictionary = new Dictionary<string, JArray>();
            //For each wanted sheet, create a json file
            foreach (ValueRange valueRange in response.ValueRanges)
            {
                string Sheetname = valueRange.Range.Split('!')[0];
                progressMessage = string.Format("Processing {0}...", Sheetname);
                EditorUtility.DisplayCancelableProgressBar("Processing", progressMessage, progress / 100);
                //Create json file
                var jsonArray = CreateJson(valueRange);
                jsonDictionary.Add(Sheetname, jsonArray);
                if (wantedSheetNames.Length <= 0)
                    progress += 85 / (response.ValueRanges.Count);
                else
                    progress += 85 / wantedSheetNames.Length;
            }

            progress = 100;
            AssetDatabase.Refresh();

            Debug.Log("Download google sheets completed");

            EditorUtility.ClearProgressBar();
            return jsonDictionary;
        }

        private JArray CreateJson(ValueRange valueRange)
        {
            IDictionary<int, string> propertyNames = new Dictionary<int, string>();
            IDictionary<int, Dictionary<int, string>> values = new Dictionary<int, Dictionary<int, string>>();

            int rowIndex = 0;

            // Читаем таблицу
            foreach (IList<object> row in valueRange.Values)
            {
                int columnIndex = 0;

                foreach (string cellValue in row)
                {
                    if (rowIndex == 0)
                    {
                        propertyNames[columnIndex] = cellValue;
                    }
                    else
                    {
                        int dataRow = rowIndex - 1;

                        if (!values.ContainsKey(dataRow))
                            values[dataRow] = new Dictionary<int, string>();

                        values[dataRow][columnIndex] = cellValue;
                    }

                    columnIndex++;
                }

                rowIndex++;
            }

            // Создаём итоговый JArray
            JArray array = new JArray();

            foreach (var rowId in values.Keys)
            {
                JObject obj = new JObject();

                foreach (var columnId in propertyNames.Keys)
                {
                    string key = propertyNames[columnId];
                    string raw = values[rowId].ContainsKey(columnId) ? values[rowId][columnId] : "";

                    obj[key] = ParseValue(raw);
                }

                array.Add(obj);
            }

            return array;
        }

        private JToken ParseValue(string v)
        {
            if (string.IsNullOrWhiteSpace(v))
                return JValue.CreateNull();

            // ----- Массив -----
            if (v.Contains(","))
            {
                string[] parts = v.Split(',');

                // try int[]
                if (parts.All(p => int.TryParse(p, out _)))
                    return JArray.FromObject(parts.Select(int.Parse));

                // try float[]
                if (parts.All(p => float.TryParse(p, NumberStyles.Any, CultureInfo.InvariantCulture, out _)))
                    return JArray.FromObject(parts.Select(p => float.Parse(p, CultureInfo.InvariantCulture)));

                // try bool[]
                if (parts.All(p => bool.TryParse(p, out _)))
                    return JArray.FromObject(parts.Select(bool.Parse));

                // fallback: string[]
                return JArray.FromObject(parts);
            }

            // ----- Одинарное значение -----

            // bool
            if (bool.TryParse(v, out bool b))
                return new JValue(b);

            // int
            if (int.TryParse(v, out int i))
                return new JValue(i);

            // float
            if (float.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out float f))
                return new JValue(f);

            // string (по умолчанию)
            return new JValue(v);
        }

        private UserCredential GetCredential()
        {
            UserCredential credential = null;
            ClientSecrets clientSecrets = new ClientSecrets();
            clientSecrets.ClientId = CLIENT_ID;
            clientSecrets.ClientSecret = CLIENT_SECRET;
            try
            {
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    clientSecrets,
                    Scopes,
                    "user",
                    CancellationToken.None).Result;
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }

            return credential;
        }

        private bool MyRemoteCertificateValidationCallback(Object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            bool isOk = true;
            // If there are errors in the certificate chain, look at each error to determine the cause.
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                    {
                        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                        chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                        bool chainIsValid = chain.Build((X509Certificate2)certificate);
                        if (!chainIsValid)
                        {
                            Debug.LogError("certificate chain is not valid");
                            isOk = false;
                        }
                    }
                }
            }

            return isOk;
        }
    }
}
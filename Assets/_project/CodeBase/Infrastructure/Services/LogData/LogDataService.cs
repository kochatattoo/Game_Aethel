using CodeBase.Data;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace CodeBase.Infrastructure.Services.LogData
{
    public class LogDataService: ILogDataService, IInitializable
    {
        private const string LogPath = "LogData/UnityID";
        public LogData logData { get; private set; }

        public void Initialize()
        {
           Load();
            Debug.Log(logData.androidGameID);
            Debug.Log(logData.iOSGameID);
            Debug.Log(logData.rewardedID);
        }

        public async void InitializeAsync()
        {
           await LoadAsync();
        }

        private void Load() =>
            logData = Resources
             .Load<TextAsset>(LogPath)
             .text
             .ToDeserialized<LogData>();

        private Task<LogData> LoadAsync()
        {
            logData = Resources
              .Load<TextAsset>(LogPath)
              .text
              .ToDeserialized<LogData>();

            return Task.FromResult(logData);
        }
    }
}

using CodeBase.Data;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.SaveLoad
{
    public class SaveLoadService : ISaveLoadService
    {
        private const string ProgressKey = "Progress";

        private readonly IGameFactory _gameFactory;
        private readonly IPersistentProgressService _progressServices;


        public SaveLoadService(IPersistentProgressService progressServices, IGameFactory gamefactory)
        {
            _gameFactory = gamefactory;
            _progressServices = progressServices;
        }

        public void SaveProgress()
        {
            foreach (ISavedProgress progressWritet in _gameFactory.ProgressWriters)
                progressWritet.UpdateProgress(_progressServices.Progress);

            PlayerPrefs.SetString(ProgressKey, _progressServices.Progress.ToJson());

            Debug.Log("Progress Saved by service");
        }

        public PlayerProgress LoadProgress() =>
            PlayerPrefs.GetString(ProgressKey)?
            .ToDeserialized<PlayerProgress>();

    }
}

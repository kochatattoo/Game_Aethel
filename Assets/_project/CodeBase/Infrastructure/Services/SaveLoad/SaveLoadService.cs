using CodeBase.Data;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;
using Zenject;

namespace CodeBase.Infrastructure.Services.SaveLoad
{
    public class SaveLoadService : ISaveLoadService, IInitializable
    {
        private const string ProgressKey = "Progress";

        private readonly IServiceFactory _serviceFactory;
        private readonly IPersistentProgressService _progressServices;
        private IGameFactory _gameFactory;

        public SaveLoadService(IPersistentProgressService progressServices, IServiceFactory serviceFactory)
        {
            _progressServices = progressServices;
            _serviceFactory = serviceFactory;
        }

        public void Initialize()
        {
            _gameFactory = _serviceFactory.CreateService<IGameFactory>();
        }

        public void SaveProgress()
        {
            foreach (ISavedProgress progressWritet in _gameFactory.ProgressWriters)
                progressWritet.UpdateProgress(_progressServices.Progress);

            PlayerPrefs.SetString(ProgressKey, _progressServices.Progress.ToJson());

            Debug.Log("Progress Saved by service");
        }

        //TODO 
        public void SaveLevelPregress()
        {
            foreach (ISavedProgress progressWritet in _gameFactory.ProgressWriters)
                progressWritet.UpdateProgress(_progressServices.Progress);

            PlayerPrefs.SetString(ProgressKey, _progressServices.Progress.ToJson());
            Debug.Log("Level progress Saved by service");
        }

        public PlayerProgress LoadProgress() =>
            PlayerPrefs.GetString(ProgressKey)?
            .ToDeserialized<PlayerProgress>();
    }
}

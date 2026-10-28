using CodeBase.Data;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;
using Zenject;
using System;

namespace CodeBase.Infrastructure.Services.SaveLoad
{
    public class SaveLoadService : ISaveLoadService, IInitializable
    {
        private static string ProgressKey
        {
            get
            {
#if UNITY_EDITOR
                if (UnityEditor.SessionState.GetBool("PetProject.CharacterMenu.Smoke", false))
                    return "PetProject.CharacterMigration.Smoke";
#endif
                return "Progress";
            }
        }

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
            PlayerPrefs.Save();

            Debug.Log("Progress Saved by service");
        }

        public PlayerProgress LoadProgress()
        {
            var json = PlayerPrefs.GetString(ProgressKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json)) return null;
            try { return json.ToDeserialized<PlayerProgress>(); }
            catch (ArgumentException) { return null; }
        }
    }
}

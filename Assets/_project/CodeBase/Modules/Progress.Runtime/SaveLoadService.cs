using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeBase.Data;

using CodeBase.Infrastructure.Services.PersistentProgress;
using PetProject.SaveStorage;
using UnityEngine;


namespace CodeBase.Infrastructure.Services.SaveLoad
{
    public sealed class SaveLoadService : ISaveLoadService, ISaveSlotService
    {
        private readonly IProgressRegistry _registry;
        private readonly ISaveEnvironment _environment;
        private readonly IPersistentProgressService _progress;

        private JsonSlotStore<GameSaveData> _store;
        private Func<PlayerProgress, bool> _validator;
        private string _safeLevel, _profileId, _pendingSlot, _pendingProfile;
        private GameSaveData _loaded;
        public string ActiveSlotId { get; private set; }
        public string Notice { get; private set; }
        public bool HasPendingProfile => _pendingSlot != null;
        public static string StorageRoot => SaveEnvironment.Current.StorageRoot;

        public SaveLoadService(IPersistentProgressService progress, IProgressRegistry registry, ISaveEnvironment environment)
        {
            _progress = progress;
            _registry = registry;
            _environment = environment;
        }

        public void InitializeStorage(Func<PlayerProgress, bool> validator, string safeLevel)
        {
            if (_store != null) return;
            _validator = validator; _safeLevel = safeLevel;
            _store = new JsonSlotStore<GameSaveData>(_environment.StorageRoot, data => JsonUtility.ToJson(data), JsonUtility.FromJson<GameSaveData>, ValidateData);
            var slots = _store.List();
            if (slots.All(slot => slot.State == SaveSlotState.Empty) && !_store.HasLegacySnapshot)
            {
                var key = _environment.LegacyKey;
                var legacy = PlayerPrefs.GetString(key, "");
                if (!string.IsNullOrWhiteSpace(legacy)) ImportLegacy(legacy);
            }
            SelectMostRecent();
        }
        private bool ValidateData(GameSaveData data)
        {
            try { return _validator(GameSaveAdapter.Restore(data)); }
            catch (ArgumentException) { return false; }
            catch (NullReferenceException) { return false; }
        }
        private void ImportLegacy(string json)
        {
            try
            {
                var legacy = JsonUtility.FromJson<PlayerProgress>(json);
                if (legacy == null || !_validator(legacy)) { Notice = "Старое сохранение не прошло проверку"; return; }
                _store.Write("slot-1", Guid.NewGuid().ToString("N"), GameSaveAdapter.Capture(legacy, _safeLevel));
                _store.PreserveLegacy(json);
                Notice = "Старое сохранение импортировано в слот 1. Исходные данные сохранены";
            }
            catch (Exception error) when (error is ArgumentException || error is SaveStorageException || error is IOException || error is UnauthorizedAccessException)
            { Notice = "Не удалось импортировать старое сохранение. Исходные данные сохранены"; Debug.LogWarning(Notice + ": " + error.Message); }
        }
        public IReadOnlyList<SaveSlotInfo> GetSlots()
        {
            RequireReady();
            return _store.List().Select(slot => new SaveSlotInfo(slot.Id, (SaveSlotStatus)slot.State,
                slot.Message, slot.Envelope?.ProfileId, slot.Envelope?.SavedAtUtc, slot.Data)).ToArray();
        }
        public void SelectSlot(string slotId)
        {
            RequireReady();
            var slot = _store.Read(slotId);
            if (!slot.CanLoad) throw new SaveStorageException(slot.Message);
            var progress = GameSaveAdapter.Restore(slot.Data);
            if (!_validator(progress)) throw new SaveStorageException("Профиль несовместим с текущей игрой");
            _loaded = slot.Data; ActiveSlotId = slot.Id; _profileId = slot.Envelope.ProfileId;
            if (slot.State == SaveSlotState.Recovered) Notice = slot.Message;
            CancelPendingProfile();
        }
        public void PrepareNewProfile(string slotId)
        {
            RequireReady(); _store.FilePath(slotId);
            _pendingSlot = slotId; _pendingProfile = Guid.NewGuid().ToString("N");
        }
        public void CancelPendingProfile() { _pendingSlot = null; _pendingProfile = null; }
        public void SaveProgress()
        {
            var id = _pendingSlot ?? ActiveSlotId;
            if (id == null) throw new SaveStorageException("Сначала выберите слот сохранения");
            WriteProgress(id, _pendingProfile ?? _profileId, HasPendingProfile);
        }
        public void SaveToSlot(string slotId)
        {
            if (HasPendingProfile) throw new SaveStorageException("Дождитесь завершения создания героя");
            WriteProgress(slotId, _profileId ?? Guid.NewGuid().ToString("N"), true);
        }
        private void WriteProgress(string slotId, string profileId, bool allowReplaceUnusable)
        {
            RequireReady();
            _registry.Capture(_progress.Progress);
            if (!_validator(_progress.Progress)) throw new SaveStorageException("Текущий прогресс не прошёл проверку");
            var written = _store.Write(slotId, profileId, GameSaveAdapter.Capture(_progress.Progress, _safeLevel), allowReplaceUnusable);
            _loaded = written.Data;
            ActiveSlotId = slotId; _profileId = profileId;
            CancelPendingProfile(); Notice = null;
            Debug.Log("Прогресс записан в " + slotId);
        }
        public PlayerProgress LoadProgress() => _loaded == null ? null : GameSaveAdapter.Restore(JsonUtility.FromJson<GameSaveData>(JsonUtility.ToJson(_loaded)));
        public void DeleteSlot(string slotId)
        {
            RequireReady(); _store.Archive(slotId);
            if (ActiveSlotId == slotId) { ActiveSlotId = null; _loaded = null; _profileId = null; SelectMostRecent(); }
        }
        private void SelectMostRecent()
        {
            var slot = _store.List().Where(item => item.CanLoad).OrderByDescending(item => item.Envelope.SavedAtUtc, StringComparer.Ordinal).FirstOrDefault();
            if (slot != null) SelectSlot(slot.Id);
        }
        private void RequireReady() { if (_store == null) throw new SaveStorageException("Хранилище ещё не подготовлено"); }
    }
}

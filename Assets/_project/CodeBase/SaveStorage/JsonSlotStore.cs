using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace PetProject.SaveStorage
{
    /// <summary>Single-writer store. Payload codecs and validation belong to the game adapter.</summary>
    public sealed class JsonSlotStore<T> where T : class
    {
        public const int CurrentSchema = 1;
        public static readonly IReadOnlyList<string> SlotIds = Array.AsReadOnly(new[] { "slot-1", "slot-2", "slot-3" });
        private const long MaxFileBytes = 8 * 1024 * 1024;
        private readonly string _root;
        private readonly Func<T, string> _serialize;
        private readonly Func<string, T> _deserialize;
        private readonly Func<T, bool> _validate;

        public JsonSlotStore(string root, Func<T, string> serialize, Func<string, T> deserialize, Func<T, bool> validate)
        {
            _root = Path.GetFullPath(root);
            _serialize = serialize ?? throw new ArgumentNullException(nameof(serialize));
            _deserialize = deserialize ?? throw new ArgumentNullException(nameof(deserialize));
            _validate = validate ?? throw new ArgumentNullException(nameof(validate));
        }

        public string FilePath(string slotId) { CheckId(slotId); return Path.Combine(_root, slotId + ".json"); }

        public SaveSlot<T> Read(string slotId)
        {
            var path = FilePath(slotId);
            try
            {
                var primary = ReadFile(path, slotId);
                if (primary.CanLoad || primary.State == SaveSlotState.Unsupported || primary.State == SaveSlotState.Unavailable)
                    return primary;
                var backup = ReadFile(path + ".bak", slotId);
                if (backup.CanLoad)
                {
                    backup.State = SaveSlotState.Recovered;
                    backup.Message = "Восстановлено из резервной копии";
                    return backup;
                }
                if (primary.State == SaveSlotState.Empty && backup.State != SaveSlotState.Empty) return backup;
                return primary;
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
            {
                return Result(slotId, SaveSlotState.Unavailable, "Нет доступа к файлу сохранения");
            }
        }

        public List<SaveSlot<T>> List()
        {
            var result = new List<SaveSlot<T>>();
            foreach (var id in SlotIds) result.Add(Read(id));
            return result;
        }

        public SaveSlot<T> Write(string slotId, string profileId, T data, bool allowReplaceUnusable = false)
        {
            var path = FilePath(slotId);
            if (!Guid.TryParseExact(profileId, "N", out _)) throw new ArgumentException("Неверный profileId");
            if (data == null || !_validate(data)) throw new SaveStorageException("Профиль не прошёл проверку");
            var envelope = new SaveEnvelope
            {
                SchemaVersion = CurrentSchema, SlotId = slotId, ProfileId = profileId,
                SavedAtUtc = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture), PayloadJson = _serialize(data)
            };
            envelope.Checksum = Hash(envelope);
            var json = JsonUtility.ToJson(envelope, true);
            if(Encoding.UTF8.GetByteCount(json)>MaxFileBytes)throw new SaveStorageException("Сохранение превышает допустимый размер");
            // Round-trip before touching the previous save, including the game's payload validator.
            var roundTrip = Decode(json, slotId);
            if (!roundTrip.CanLoad) throw new SaveStorageException("Не удалось проверить сериализованный профиль");
            var temporary = path + ".tmp";
            try
            {
                Directory.CreateDirectory(_root);
                var current = ReadFile(path, slotId);
                var effective = Read(slotId);
                if (effective.State == SaveSlotState.Unavailable) throw new SaveStorageException("Слот недоступен для записи");
                if (!allowReplaceUnusable && (effective.State == SaveSlotState.Unsupported || effective.State == SaveSlotState.Corrupt))
                    throw new SaveStorageException("Для замены несовместимого или повреждённого слота требуется подтверждение в меню");
                using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var bytes = new UTF8Encoding(false).GetBytes(json);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }
                if (File.Exists(path))
                {
                    // Never replace a good backup with a corrupt/unsupported primary.
                    var previous = current.CanLoad ? path + ".bak" : path + ".replaced-" + Guid.NewGuid().ToString("N");
                    File.Replace(temporary, path, previous, true);
                }
                else File.Move(temporary, path);
                return roundTrip;
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is NotSupportedException)
            {
                throw new SaveStorageException("Сохранение не записано. Предыдущий файл сохранён", error);
            }
            finally
            {
                try { if (File.Exists(temporary)) File.Delete(temporary); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }

        public void Archive(string slotId)
        {
            var path = FilePath(slotId);
            try
            {
                var archive = Path.Combine(_root, "Deleted", slotId + "-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(archive);
                // Keep the whole slot (primary, backup, quarantined copies) for manual recovery.
                var moved = new List<KeyValuePair<string, string>>();
                try
                {
                    foreach (var file in Directory.GetFiles(_root, slotId + ".json*", SearchOption.TopDirectoryOnly))
                    {
                        var destination = Path.Combine(archive, Path.GetFileName(file));
                        File.Move(file, destination);
                        moved.Add(new KeyValuePair<string, string>(file, destination));
                    }
                }
                catch
                {
                    foreach (var pair in moved) File.Move(pair.Value, pair.Key);
                    throw;
                }
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
            { throw new SaveStorageException("Не удалось удалить слот", error); }
        }

        public void PreserveLegacy(string rawJson)
        {
            Directory.CreateDirectory(_root);
            var path = Path.Combine(_root, "legacy-playerprefs.json");
            // This immutable copy is also the migration marker. Never overwrite the original snapshot.
            if (File.Exists(path)) return;
            using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            var bytes = new UTF8Encoding(false).GetBytes(rawJson);
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush(true);
        }

        public bool HasLegacySnapshot => File.Exists(Path.Combine(_root, "legacy-playerprefs.json"));

        private SaveSlot<T> ReadFile(string path, string id)
        {
            if (!File.Exists(path)) return Result(id, SaveSlotState.Empty, "Пустой слот");
            if (new FileInfo(path).Length > MaxFileBytes) return Result(id, SaveSlotState.Corrupt, "Файл слишком большой");
            return Decode(File.ReadAllText(path, Encoding.UTF8), id);
        }

        private SaveSlot<T> Decode(string json, string id)
        {
            try
            {
                var envelope = JsonUtility.FromJson<SaveEnvelope>(json);
                if (envelope == null) return Result(id, SaveSlotState.Corrupt, "Файл повреждён");
                if (envelope.SchemaVersion > CurrentSchema)
                    return Result(id, SaveSlotState.Unsupported, "Сохранение из более новой версии игры");
                if (envelope.SchemaVersion != CurrentSchema || envelope.SlotId != id ||
                    !Guid.TryParseExact(envelope.ProfileId, "N", out _) ||
                    !DateTimeOffset.TryParseExact(envelope.SavedAtUtc, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out _) ||
                    string.IsNullOrEmpty(envelope.PayloadJson) || envelope.Checksum != Hash(envelope))
                    return Result(id, SaveSlotState.Corrupt, "Нарушена целостность сохранения");
                var data = _deserialize(envelope.PayloadJson);
                if (data == null || !_validate(data)) return Result(id, SaveSlotState.Corrupt, "Несовместимые данные профиля");
                return new SaveSlot<T> { Id = id, State = SaveSlotState.Ready, Envelope = envelope, Data = data, Message = "Готово" };
            }
            catch (Exception error) when (error is ArgumentException || error is InvalidOperationException || error is FormatException || error is NullReferenceException)
            { return Result(id, SaveSlotState.Corrupt, "Не удалось прочитать данные профиля"); }
        }

        private static string Hash(SaveEnvelope value)
        {
            using var hash = SHA256.Create();
            var text = value.SchemaVersion + "|" + value.SlotId + "|" + value.ProfileId + "|" + value.SavedAtUtc + "|" + value.PayloadJson;
            return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(text)));
        }

        private static SaveSlot<T> Result(string id, SaveSlotState state, string message) =>
            new() { Id = id, State = state, Message = message };

        private static void CheckId(string id)
        {
            foreach (var valid in SlotIds) if (id == valid) return;
            throw new ArgumentException("Неизвестный слот", nameof(id));
        }
    }
}

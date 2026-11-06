namespace CodeBase.Infrastructure.Services.SaveLoad
{
    public enum SaveSlotStatus { Empty, Ready, Recovered, Corrupt, Unsupported, Unavailable }

    public sealed class SaveSlotInfo
    {
        public string Id { get; }
        public SaveSlotStatus State { get; }
        public string Message { get; }
        public string ProfileId { get; }
        public string SavedAtUtc { get; }
        public GameSaveData Data { get; }
        public bool CanLoad => State == SaveSlotStatus.Ready || State == SaveSlotStatus.Recovered;

        public SaveSlotInfo(string id, SaveSlotStatus state, string message, string profileId, string savedAtUtc, GameSaveData data)
        {
            Id = id;
            State = state;
            Message = message;
            ProfileId = profileId;
            SavedAtUtc = savedAtUtc;
            Data = data;
        }
    }

    public interface ISaveEnvironment
    {
        string StorageRoot { get; }
        string LegacyKey { get; }
    }
}

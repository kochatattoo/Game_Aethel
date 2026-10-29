using System;

namespace PetProject.SaveStorage
{
    public enum SaveSlotState { Empty, Ready, Recovered, Corrupt, Unsupported, Unavailable }

    [Serializable]
    public sealed class SaveEnvelope
    {
        public int SchemaVersion;
        public string SlotId;
        public string ProfileId;
        public string SavedAtUtc;
        public string PayloadJson;
        public string Checksum;
    }

    public sealed class SaveSlot<T> where T : class
    {
        public string Id { get; internal set; }
        public SaveSlotState State { get; internal set; }
        public string Message { get; internal set; }
        public SaveEnvelope Envelope { get; internal set; }
        public T Data { get; internal set; }
        public bool CanLoad => State == SaveSlotState.Ready || State == SaveSlotState.Recovered;
    }

    public sealed class SaveStorageException : Exception
    {
        public SaveStorageException(string message, Exception inner = null) : base(message, inner) { }
    }
}

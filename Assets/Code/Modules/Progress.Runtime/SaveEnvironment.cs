using System.IO;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.SaveLoad
{
    public sealed class SaveEnvironment : ISaveEnvironment
    {
        public static ISaveEnvironment Current { get; set; } = new SaveEnvironment();
        public string StorageRoot => Path.Combine(Application.persistentDataPath, "Saves");
        public string LegacyKey => "Progress";
    }
}

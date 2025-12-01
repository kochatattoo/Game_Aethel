using CodeBase.StaticData;
using Unity.Services.Core;
using UnityEngine;

namespace CodeBase.Infrastructure.StaticData
{
    public interface IStaticDataService : IService
    {
        void Load();
        LevelStaticData ForLevel(string sceneKey);
    }
}
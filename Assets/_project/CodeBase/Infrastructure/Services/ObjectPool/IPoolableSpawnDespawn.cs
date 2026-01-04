using System;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.ObjectPool
{
    public interface IPoolableSpawnDespawn
    {
        object Spawn(Transform parent = null, Action<object> init = null);
        void Despawn(object instance);
    }
}

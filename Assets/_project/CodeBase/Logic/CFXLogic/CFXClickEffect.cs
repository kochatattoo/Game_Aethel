using CodeBase.Infrastructure.Services.ObjectPool;
using System;
using UnityEngine;

namespace Assets._project.CodeBase.Logic.CFXLogic
{
    public class CFXClickEffect : MonoBehaviour, IPoolable<CFXClickEffect>
    {
        public void OnDespawned()
        {
            throw new NotImplementedException();
        }

        public void OnSpawned()
        {
            throw new NotImplementedException();
        }

        public void SetPool(IPool<CFXClickEffect> pool)
        {
            throw new NotImplementedException();
        }
    }
}

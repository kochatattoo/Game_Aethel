using UnityEngine;

namespace CodeBase.Infrastructure.Services.ObjectPool
{
    public class PoolContainer: MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}

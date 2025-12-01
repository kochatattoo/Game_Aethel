using CodeBase.Infrastructure;
using UnityEngine;
namespace CodeBase
{
    public class GameBootstrapper : MonoBehaviour, ICoroutineRunner
    {
        void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}
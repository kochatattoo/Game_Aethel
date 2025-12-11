using CodeBase.Infrastructure;
using UnityEngine;
namespace CodeBase
{
    public class CoroutineRunner : MonoBehaviour, ICoroutineRunner
    {
        void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}
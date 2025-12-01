using UnityEngine;
namespace CodeBase
{
    public class GameRunner : MonoBehaviour
    {
        public GameBootstrapper bootstrapperPrefab;

        private void Awake()
        {
            var bootstrapper = FindFirstObjectByType<GameBootstrapper>();

            if (bootstrapper == null)
                Instantiate(bootstrapperPrefab);

        }
    }
}
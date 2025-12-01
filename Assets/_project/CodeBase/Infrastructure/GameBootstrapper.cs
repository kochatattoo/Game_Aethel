using UnityEngine;
namespace CodeBase
{
    public class GameBootstrapper : MonoBehaviour
    {
        void Awake()
        {
            Game game = new Game();

            DontDestroyOnLoad(this);
        }
    }
}
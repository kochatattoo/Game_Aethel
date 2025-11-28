using UnityEngine;
namespace CodeBase
{
    public class GameBootstrapper : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            Game game = new Game();

            DontDestroyOnLoad(this);
        }
    }
}
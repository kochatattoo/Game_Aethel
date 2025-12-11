using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine;

namespace CodeBase.Infrastructure.AssetManagement
{
    public interface IAsset
    {
        Task<GameObject> Instantiate(string path);
        Task<GameObject> Instantiate(string path, Vector3 at);
        Task<GameObject> Instantiate(string path, Transform parent);
        Task<T> Load<T>(AssetReference assetbReference) where T : class;
        Task<T> Load<T>(string address) where T : class;
        void CleanUp();
        void Initialize();
    }
}

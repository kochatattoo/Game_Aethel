using System.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public interface IGameFactory
    {
        Task<GameObject> CreateHero(Vector3 at);
    }
}
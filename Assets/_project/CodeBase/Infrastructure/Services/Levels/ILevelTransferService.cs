using UnityEngine;

namespace CodeBase.Infrastructure.Services.Levels
{
    public interface ILevelTransferService : IService 
    {
        void GoTo(string levelName);
    }
}

using System;
using UnityEngine;

namespace CodeBase.Infrastructure.Services
{
    public interface IInputService : IService
    {
        Vector2 Axis { get; }
        event Action Attack;
    }
}

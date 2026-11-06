using System;
using UnityEngine;

namespace CodeBase.Infrastructure.Services
{
    public interface IInputService
    {
        event Action<Vector3> Click;
    }
}

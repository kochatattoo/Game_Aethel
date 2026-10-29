using System;

namespace CodeBase.Infrastructure.Services
{
    public interface IClickListener
    {
        event Action OnProcessed;
    }
}
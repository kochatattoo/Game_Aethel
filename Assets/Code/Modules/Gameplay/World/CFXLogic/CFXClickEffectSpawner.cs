using CodeBase.Infrastructure.Services;
using System;
using Zenject;

namespace Assets._project.CodeBase.Logic.CFXLogic
{
    public class CFXClickEffectSpawner : IInitializable, IDisposable
    {
        private readonly IClickListener _clickListener;
        private readonly CFXClickEffect _effect;

        public CFXClickEffectSpawner(IClickListener clickListener, CFXClickEffect effect)
        {
            _clickListener = clickListener;
            _effect = effect;
        }

        public void Initialize()
        {
            _clickListener.OnProcessed += HandleTarget;
        }

        public void Dispose()
        {
            _clickListener.OnProcessed -= HandleTarget;
        }

        private void HandleTarget()
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using UnityEngine;

namespace CodeBase.CharacterIntegration
{
    public static class MenuInputScope
    {
        private static int _blocks, _pauses, _generation;
        private static float _previousTimeScale = 1;
        public static bool IsBlocked => _blocks > 0;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { _blocks = _pauses = 0; _generation++; }

        public static IDisposable Acquire(bool pause = true) => new Lease(pause);
        private sealed class Lease : IDisposable
        {
            private readonly bool _pause;
            private readonly int _version;
            private bool _disposed;
            public Lease(bool pause)
            {
                _version = _generation; _pause = pause; _blocks++;
                if (pause && _pauses++ == 0) { _previousTimeScale = Time.timeScale; Time.timeScale = 0; }
                Cursor.visible = true; Cursor.lockState = CursorLockMode.None;
            }
            public void Dispose()
            {
                if (_disposed || _version != _generation) return;
                _disposed = true; _blocks--;
                if (_pause && --_pauses == 0) Time.timeScale = _previousTimeScale;
            }
        }
    }
}

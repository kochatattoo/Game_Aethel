using UnityEngine;
using UnityEngine.Rendering;

namespace CodeBase.MotionTest
{
    public sealed class MotionTargetHighlighter : MonoBehaviour
    {
        private LineRenderer _renderer;

        public bool IsVisible => _renderer != null && _renderer.enabled;

        public void Initialize(MotionTestConfig config)
        {
            var node = new GameObject("Attack target highlight");
            node.transform.SetParent(transform, false);
            _renderer = node.AddComponent<LineRenderer>();
            _renderer.sharedMaterial = config.GridMaterial;
            _renderer.useWorldSpace = false;
            _renderer.loop = true;
            _renderer.positionCount = 32;
            _renderer.widthMultiplier = .06f;
            _renderer.startColor = config.TargetHighlightColor;
            _renderer.endColor = config.TargetHighlightColor;
            _renderer.shadowCastingMode = ShadowCastingMode.Off;
            _renderer.receiveShadows = false;
            var radius = Mathf.Max(config.AgentRadius + .2f, .65f);
            for (var i = 0; i < _renderer.positionCount; ++i)
            {
                var angle = i * Mathf.PI * 2 / _renderer.positionCount;
                _renderer.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, .08f, Mathf.Sin(angle) * radius));
            }
            _renderer.enabled = false;
        }

        public void SetVisible(bool value)
        {
            if (_renderer != null) _renderer.enabled = value;
        }
    }
}

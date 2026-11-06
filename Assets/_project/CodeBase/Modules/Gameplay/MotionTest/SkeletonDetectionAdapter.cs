using System;
using CodeBase.Components;
using UnityEngine;

namespace CodeBase.MotionTest
{
    public sealed class SkeletonDetectionAdapter : MonoBehaviour
    {
        [SerializeField] private VisionSensor _sensor = new VisionSensor();
        [SerializeField] private float _viewDistance = 10f;
        [SerializeField] private float _sleepDistance = 20f;
        private Transform _hero;
        private Transform _head;
        private Action _spotted;
        private float _cooldown;

        public void Construct(Transform hero, Action spotted)
        {
            _hero = hero;
            _spotted = spotted;
            _head = FindChild(transform, "Eyes_Direction");
            if (_head == null)
            {
                var marker = new GameObject("Detection head").transform;
                marker.SetParent(transform, false);
                marker.localPosition = new Vector3(0, 1.5f, 0);
                _head = marker;
            }
            _cooldown = .25f;
        }

        private void Update()
        {
            if (_hero == null || _spotted == null || _head == null) return;
            _cooldown -= Time.deltaTime;
            if (_cooldown > 0) return;
            _cooldown = .5f;
            if ((_hero.position - transform.position).sqrMagnitude > _sleepDistance * _sleepDistance) return;
            if (_sensor.Scan(_head, _viewDistance, 1 << 2, out _) > 0)
            {
                var callback = _spotted;
                _spotted = null;
                callback();
            }
        }

        private static Transform FindChild(Transform root, string name)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
                if (child.name == name) return child;
            return null;
        }
    }
}

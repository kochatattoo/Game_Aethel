using UnityEngine;

namespace CodeBase.Enemies
{
    public class RotateToHero : Follow
    {
        public float Speed;

        private Transform _heroTransform;

        public void Construct(Transform heroTransform) => _heroTransform = heroTransform;

        private void Update()
        {
            if (_heroTransform == null) return;
            var direction = _heroTransform.position - transform.position;
            if (direction.sqrMagnitude < .0001f) return;
            var target = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, target, Speed * Time.deltaTime);
        }
    }
}

using UnityEngine;

namespace CodeBase.Enemies
{
    public class RotateToHero : Follow
    {
        public float Speed;

        private Transform _heroTransform;
        private Vector3 _positionToLook;

        public void Consturct(Transform heroTransform)
        {
            _heroTransform = heroTransform;
        }

        private void Update()
        {
            if (Initialized())
                RotateTowardsHero();
        }

        private void RotateTowardsHero()
        {
            UpdatePositionLookAt();
            transform.rotation = SmoothRotation(transform.rotation, _positionToLook);
        }

        private void UpdatePositionLookAt()
        {
            Vector3 positionDiff = _heroTransform.position - transform.position;
            _positionToLook = new Vector3(positionDiff.x, positionDiff.y, positionDiff.z);
        }

        private Quaternion SmoothRotation(Quaternion rotation, Vector3 positionToLook) =>
            Quaternion.Lerp(rotation, TargetRotation(positionToLook), SpeedFactor());

        private Quaternion TargetRotation(Vector3 position) =>
            Quaternion.LookRotation(position);

        private float SpeedFactor() =>
            Speed * Time.deltaTime;

        private bool Initialized() =>
            _heroTransform != null;
    }
}

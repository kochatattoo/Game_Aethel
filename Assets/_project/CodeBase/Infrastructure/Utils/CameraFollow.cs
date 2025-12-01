using CodeBase.Hero;
using UnityEngine;
using Zenject;

namespace CodeBase.Infrastructure.Utils
{
    public class CameraFollow : MonoBehaviour
    {
        public float RotationAngelX = 45f;
        public float Distance = 20f;
        public float OffSetY = 1.5f;

        [SerializeField]
        private Transform _following;

        [Inject]
        private void Construct(HeroController controller)
        {
            _following = controller.transform;
        }

        private void LateUpdate()
        {
            if (_following == null) return;

            Quaternion rotation = Quaternion.Euler(RotationAngelX, 0f, 0f);

            var position = rotation * new Vector3(0, 0, -Distance) + FollowingPointPosition();

            transform.rotation = rotation;
            transform.position = position;

        }

        public void Follow(GameObject following) =>
            _following = following.transform;


        private Vector3 FollowingPointPosition()
        {
            Vector3 followingPosition = _following.position;
            followingPosition.y += OffSetY;

            return followingPosition;
        }
    }
}

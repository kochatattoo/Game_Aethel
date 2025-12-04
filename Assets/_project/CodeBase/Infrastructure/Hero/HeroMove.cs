using CodeBase.Infrastructure.Services;
using UnityEngine;
using Zenject;

namespace CodeBase.Hero
{
    public class HeroMove : MonoBehaviour
    {
        public float MoveSpeed = 5f;

        private CharacterController _characterController;
        private IInputService _input;

        [Inject]
        public void Construct(IInputService inputService)
        {
            _input = inputService;
        }

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        void Update()
        {
            Vector3 movementVector = Vector3.zero;

            if (_input?.Axis.sqrMagnitude > ProjectConstants.Epsilon)
            {
                movementVector = Camera.main.transform.TransformDirection(_input.Axis);
                movementVector.y = 0;
                movementVector.Normalize();
                transform.forward = movementVector;
            }

            movementVector += Physics.gravity;

            _characterController.Move(motion: MoveSpeed * Time.deltaTime * movementVector);
        }
    }
}
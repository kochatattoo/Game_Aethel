using CodeBase.InputActions;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeBase.Infrastructure.Services
{
    /// <summary>
    /// Класс для классического ввода управления WASD и для разработки стандартной TopDownControllSystem 
    /// В настоящий момент использую InputService - управление с помощью мышки и точки
    /// </summary>
    [Obsolete]
    public class InputServiceWASD : InputServiceAbstract
    {
        public InputServiceWASD()
        {
            _actions = new PlayerInputAction();
        }

        public override void Subscribe()
        {
            // Включаем экшен сет и подписываемся
            _actions.Player.Enable();

            _actions.Player.Move.performed += OnMove;
            _actions.Player.Move.canceled += OnMove;
            _actions.Player.Attack.started += OnAttack;
            _actions.Player.Attack.canceled += OnAttack;

            // Выбираем схему на старте
            ChoiseInpuDevice();
        }

        public override void Unsubscribe()
        {
            // Отписываемся и выключаем
            _actions.Player.Move.performed -= OnMove;
            _actions.Player.Move.canceled -= OnMove;
            _actions.Player.Attack.started -= OnAttack;
            _actions.Player.Attack.canceled -= OnAttack;

            _actions.Player.Disable();
            _actions.Dispose();
        }

        private void OnAttack(InputAction.CallbackContext ctx)
        {
            AttackAction(ctx);
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            _move = ctx.ReadValue<Vector2>();
        }
    }
}

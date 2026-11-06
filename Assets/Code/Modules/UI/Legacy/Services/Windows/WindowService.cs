using System;
using CodeBase.UI.Services.Factory;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.UI.Services.Windows
{
    public sealed class WindowService : IWindowService
    {
        private readonly IUIFactory _factory;
        public WindowService(IUIFactory factory) => _factory = factory;

        public void Open(WindowId id) => OpenAsync(id).Forget(Debug.LogException);

        public UniTask OpenAsync(WindowId id) => id switch
        {
            WindowId.Unknow => UniTask.CompletedTask,
            WindowId.Shop => _factory.CreateShopAsync(),
            WindowId.Option => _factory.CreateOptionAsync(),
            _ => throw new ArgumentOutOfRangeException(nameof(id), id, "Неизвестное окно")
        };
    }
}

using System;
using System.Collections.Generic;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.StaticData.Windows;
using CodeBase.UI.Services.Windows;
using Cysharp.Threading.Tasks;

namespace CodeBase.UI.Services.Factory
{
    public interface IWindowCatalog
    {
        UniTask InitializeAsync();
        WindowConfig ForWindow(WindowId id);
    }

    public sealed class WindowCatalog : IWindowCatalog
    {
        private readonly IAsset _assets;
        private readonly Dictionary<WindowId, WindowConfig> _windows = new();

        public WindowCatalog(IAsset assets) => _assets = assets;

        public async UniTask InitializeAsync()
        {
            if (_windows.Count > 0) return;
            var data = await _assets.Load<WindowStaticData>(AssetAddress.WindowStaticData);
            if (data == null) throw new InvalidOperationException("Не настроен каталог окон");
            foreach (var entry in data.Configs) _windows.Add(entry.WindowId, entry);
        }

        public WindowConfig ForWindow(WindowId id) => _windows.TryGetValue(id, out var config)
            ? config
            : throw new InvalidOperationException("Не настроено окно: " + id);
    }
}

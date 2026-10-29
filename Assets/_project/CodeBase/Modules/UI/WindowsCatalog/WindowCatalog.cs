using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase.StaticData.Windows;
using CodeBase.UI.Services.Windows;
using UnityEngine;

namespace CodeBase.UI.Services.Factory
{
    public interface IWindowCatalog { WindowConfig ForWindow(WindowId id); }

    public sealed class WindowCatalog : IWindowCatalog
    {
        private readonly Dictionary<WindowId, WindowConfig> _windows;

        public WindowCatalog()
        {
            var data = Resources.Load<WindowStaticData>("StaticData/ui/WindowStaticData");
            if (data == null) throw new InvalidOperationException("Не настроен каталог окон");
            _windows = data.Configs.ToDictionary(entry => entry.WindowId);
        }

        public WindowConfig ForWindow(WindowId id) => _windows.TryGetValue(id, out var config)
            ? config : throw new InvalidOperationException("Не настроено окно: " + id);
    }
}

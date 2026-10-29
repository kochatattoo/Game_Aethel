using CodeBase.UI.Services.Windows;
using CodeBase.UI.Windows;
using System;

namespace CodeBase.StaticData.Windows
{
    [Serializable]
    public class WindowConfig
    {
        public WindowId WindowId;
        public WindowBase prefab;
    }
}

using CodeBase.MotionTest;
using UnityEditor;

namespace CodeBase.EditorTools
{
    /// <summary>Render resolution is not an editor window's bounds. Prevent editor/tool clicks from becoming world clicks.</summary>
    [InitializeOnLoad]
    public static class MotionEditorInputGate
    {
        private static MotionTestController _host;
        internal static bool InputFixtureActive;
        static MotionEditorInputGate() => EditorApplication.update += Update;
        private static void Update()
        {
            if (!EditorApplication.isPlaying) { _host = null; return; }
            if (_host == null) _host = UnityEngine.Object.FindFirstObjectByType<MotionTestController>();
            if (_host == null) return;
            _host.PointerInputFilter ??= PointerAllowed;
            _host.KeyboardInputFilter ??= KeyboardAllowed;
        }
        private static bool KeyboardAllowed() => InputFixtureActive ||
            (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType().FullName == "UnityEditor.GameView" &&
             !EditorGUIUtility.editingTextField && !EditorApplication.isPaused);
        private static bool PointerAllowed() => InputFixtureActive ||
            (KeyboardAllowed() && EditorWindow.mouseOverWindow != null && EditorWindow.mouseOverWindow.GetType().FullName == "UnityEditor.GameView");
    }
}

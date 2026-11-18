using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.MotionTest
{
    public sealed class MotionWorldHealthBar : MonoBehaviour
    {
        private Camera _camera;
        private Image _fill;
        private Text _label;

        public float CurrentNormalized => _fill != null ? _fill.fillAmount : 0;
        public string Value => _label != null ? _label.text : string.Empty;

        public void Initialize(Camera camera, int maxHealth, Color color)
        {
            _camera = camera;
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = camera;
            canvas.sortingOrder = 20;
            var rect = GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(120, 28);
            rect.localScale = Vector3.one * .012f;

            var background = CreateImage("Background", rect, Vector2.zero, new Vector2(120, 12));
            background.color = new Color(.04f, .04f, .04f, .85f);
            _fill = CreateImage("Fill", background.rectTransform, Vector2.zero, new Vector2(118, 10));
            _fill.color = color;
            _fill.type = Image.Type.Filled;
            _fill.fillMethod = Image.FillMethod.Horizontal;
            _fill.fillOrigin = 0;
            Stretch(_fill.rectTransform, new Vector2(1, 1));

            var labelObject = new GameObject("HP", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            _label = labelObject.GetComponent<Text>();
            _label.rectTransform.SetParent(rect, false);
            _label.rectTransform.anchoredPosition = new Vector2(0, 9);
            _label.rectTransform.sizeDelta = new Vector2(120, 16);
            _label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _label.fontSize = 12;
            _label.alignment = TextAnchor.MiddleCenter;
            _label.color = Color.white;
            _label.raycastTarget = false;
            SetHealth(maxHealth, maxHealth);
        }

        public void SetHealth(int current, int maximum)
        {
            if (_fill == null) return;
            _fill.fillAmount = maximum > 0 ? Mathf.Clamp01((float)current / maximum) : 0;
            _label.text = $"{current}/{maximum}";
        }

        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        private void LateUpdate()
        {
            if (_camera != null) transform.rotation = _camera.transform.rotation;
        }

        private static Image CreateImage(string name, RectTransform parent, Vector2 position, Vector2 size)
        {
            var instance = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = instance.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = instance.GetComponent<Image>();
            image.raycastTarget = false;
            return image;
        }

        private static void Stretch(RectTransform rect, Vector2 inset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = inset;
            rect.offsetMax = -inset;
            rect.anchoredPosition = Vector2.zero;
        }
    }
}

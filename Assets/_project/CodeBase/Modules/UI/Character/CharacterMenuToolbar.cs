using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace CodeBase.CharacterIntegration
{
    public sealed class CharacterMenuToolbar : MonoBehaviour
    {
        public void Construct(ICharacterMenuActions service)
        {
            var canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=5;
            gameObject.AddComponent<GraphicRaycaster>();
            Create("Персонаж [C]",-210,service.OpenAttributes);
            Create("Меню [Esc]",-20,()=>service.ShowMainMenuAsync().Forget(UnityEngine.Debug.LogException));
        }
        private void Create(string title,float x,UnityEngine.Events.UnityAction action)
        {
            var go=new GameObject(title,typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(transform,false);
            var rect=(RectTransform)go.transform;rect.anchorMin=rect.anchorMax=new Vector2(1,0);rect.pivot=new Vector2(1,0);rect.anchoredPosition=new Vector2(x,15);rect.sizeDelta=new Vector2(175,40);
            go.GetComponent<Image>().color=new Color(0.1f,0.12f,0.15f,0.95f);go.GetComponent<Button>().onClick.AddListener(action);
            var text=new GameObject("Label",typeof(RectTransform),typeof(TMPro.TextMeshProUGUI));text.transform.SetParent(go.transform,false);
            var tr=(RectTransform)text.transform;tr.anchorMin=Vector2.zero;tr.anchorMax=Vector2.one;tr.offsetMin=tr.offsetMax=Vector2.zero;
            var label=text.GetComponent<TMPro.TextMeshProUGUI>();label.text=title;label.fontSize=18;label.alignment=TMPro.TextAlignmentOptions.Center;label.raycastTarget=false;
        }
    }
}

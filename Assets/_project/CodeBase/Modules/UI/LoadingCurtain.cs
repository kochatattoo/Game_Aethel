
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace CodeBase.Logic
{
    public class LoadingCurtain : MonoBehaviour, CodeBase.CharacterIntegration.ILoadingScreen
    {
        public CanvasGroup Curtain;
        private GameObject _error;
        private void Awake(){DontDestroyOnLoad(gameObject);}
        public void Show(){StopAllCoroutines();if(_error!=null)Destroy(_error);gameObject.SetActive(true);Curtain.alpha=1;Curtain.blocksRaycasts=true;}
        public void Hide(){StopAllCoroutines();if(_error!=null)Destroy(_error);if(gameObject.activeInHierarchy)StartCoroutine(Fade());}
        private IEnumerator Fade()
        {
            while(Curtain.alpha>0){Curtain.alpha=Mathf.MoveTowards(Curtain.alpha,0,Time.unscaledDeltaTime*4);yield return null;}
            Curtain.blocksRaycasts=false;gameObject.SetActive(false);
        }
        public void ShowError(string message,Action returnToMenu)
        {
            Show();
            _error=new GameObject("LoadingError",typeof(RectTransform));_error.transform.SetParent(transform,false);
            var rect=(RectTransform)_error.transform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.sizeDelta=new Vector2(800,300);
            var text=new GameObject("Message",typeof(RectTransform),typeof(TextMeshProUGUI));text.transform.SetParent(_error.transform,false);
            var tr=(RectTransform)text.transform;tr.sizeDelta=new Vector2(800,180);tr.anchoredPosition=new Vector2(0,40);
            var label=text.GetComponent<TextMeshProUGUI>();label.text=message;label.fontSize=24;label.alignment=TextAlignmentOptions.Center;
            var go=new GameObject("ReturnToMenu",typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(_error.transform,false);
            var br=(RectTransform)go.transform;br.sizeDelta=new Vector2(400,70);br.anchoredPosition=new Vector2(0,-110);go.GetComponent<Button>().onClick.AddListener(()=>returnToMenu());
            var title=new GameObject("Title",typeof(RectTransform),typeof(TextMeshProUGUI));title.transform.SetParent(go.transform,false);
            var lr=(RectTransform)title.transform;lr.anchorMin=Vector2.zero;lr.anchorMax=Vector2.one;lr.offsetMin=lr.offsetMax=Vector2.zero;
            var caption=title.GetComponent<TextMeshProUGUI>();caption.text="Вернуться в меню";caption.fontSize=24;caption.color=Color.black;caption.alignment=TextAlignmentOptions.Center;caption.raycastTarget=false;
        }
    }
}

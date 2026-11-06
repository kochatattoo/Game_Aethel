using System;
using Code.Hero.Data;
using CodeBase.CharacterIntegration;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Code.UI.MainMenuElements
{
    public class HeroClassUI : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private Image _icon;
        [SerializeField] private int _heroID;
        private UnityEngine.Events.UnityAction _click;
        public void SetData(HeroClass hero,int id,CharacterAssets assets,Action click)
        {
            _heroID=id;_name.text=hero.Name;_click=()=>click();_button.onClick.AddListener(_click);
            LoadIcon(hero.ImagePath,assets).Forget(ex=>{if(ex is not OperationCanceledException)Debug.LogException(ex);});
        }
        private async UniTask LoadIcon(string address,CharacterAssets assets)
        {
            var sprite=await assets.LoadAsync<Sprite>(address,this.GetCancellationTokenOnDestroy());
            if(this!=null)_icon.sprite=sprite;
        }
        public void SetSelected(bool selected){if(TryGetComponent<Image>(out var image))image.color=selected?Color.yellow:Color.white;}
        private void OnDestroy(){if(_click!=null)_button.onClick.RemoveListener(_click);}
    }
}


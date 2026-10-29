using System;
using Code.Hero.Data;
using Code.UI.MainMenuElements;
using CodeBase.CharacterIntegration;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
namespace Code.UI.Windows
{
    public class NewGameWindow : WindowBase
    {
        [SerializeField] private HeroClassContainerUI heroClassContainerUI;
        [SerializeField] private HeroClassDescriptionUI heroClassDescriptionUI;
        [SerializeField] private HeroAttributesView heroAttributesView;
        [SerializeField] private HeroParamentresUI heroParamentresUI;
        [SerializeField] private Button _acceptButton;
        [SerializeField] private TextMeshProUGUI _classNameField;
        private readonly CompositeDisposable _lifetime=new();
        private HeroDataFabric _draft;
        private bool _submitted;
        public HeroDataFabric Draft=>_draft;
        public void Construct(IHeroDataProvider provider,CharacterAssets assets,CharacterBalance balance,Action<HeroData> confirm)
        {
            _draft=new HeroDataFabric(provider);_draft.Initialize();
            _draft.SelectedClass.Subscribe(hero=>{_classNameField.text=hero?.Name??"Выберите класс";if(hero!=null&&ColorUtility.TryParseHtmlString(hero.Color,out var color))_classNameField.color=color;_acceptButton.interactable=_draft.CanConfirm;}).AddTo(_lifetime);
            _draft.BonusPoints.Subscribe(_=>_acceptButton.interactable=_draft.CanConfirm).AddTo(_lifetime);
            heroClassContainerUI.Construct(_draft,assets);
            heroClassDescriptionUI.Construct(_draft);
            heroParamentresUI.Balance=balance;
            heroAttributesView.Construct(heroParamentresUI,_draft,_lifetime);
            _acceptButton.OnClickAsObservable().Subscribe(_=>{
                if(_submitted||!_draft.CanConfirm)return;
                _submitted=true;_acceptButton.interactable=false;
                var hero=_draft.CreateHeroData(balance);
                try{confirm(hero);}catch{hero.Dispose();_submitted=false;_acceptButton.interactable=true;throw;}
            }).AddTo(_lifetime);
            foreach(var key in _draft.Classes.Keys){_draft.SelectClass(key);break;}
        }
        protected override void Cleanup(){_lifetime.Dispose();_draft?.Dispose();}
    }
}


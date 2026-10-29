using Code.Hero.Data;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
namespace Code.UI.MainMenuElements
{
    public class DistributePoints : MonoBehaviour
    {
        [SerializeField] private string _attrName;
        [SerializeField] private Button _increaseBtn,_decreaseBtn;
        [SerializeField] private TextMeshProUGUI _attributePointsText;
        private CompositeDisposable _binding;
        public void Bind(HeroClass hero,HeroDataFabric draft,CompositeDisposable lifetime)
        {
            _binding?.Dispose();_binding=new CompositeDisposable();_binding.AddTo(lifetime);
            hero.Attributes[_attrName].Subscribe(v=>{_attributePointsText.text=v.ToString();_attributePointsText.color=draft.Allocated(_attrName)>0?Color.yellow:Color.white;}).AddTo(_binding);
            draft.BonusPoints.Subscribe(points=>{_increaseBtn.interactable=points>0;_decreaseBtn.interactable=draft.Allocated(_attrName)>0;_attributePointsText.color=draft.Allocated(_attrName)>0?Color.yellow:Color.white;}).AddTo(_binding);
            _increaseBtn.OnClickAsObservable().Subscribe(_=>draft.TryAllocate(_attrName,1)).AddTo(_binding);
            _decreaseBtn.OnClickAsObservable().Subscribe(_=>draft.TryAllocate(_attrName,-1)).AddTo(_binding);
        }
        private void OnDestroy()=>_binding?.Dispose();
    }
}


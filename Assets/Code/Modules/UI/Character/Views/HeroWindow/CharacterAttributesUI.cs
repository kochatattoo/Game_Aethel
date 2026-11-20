using Code.Character.Attributes;using Code.Character.Data;using TMPro;using UniRx;using UnityEngine;
namespace Code.UI.HeroWindow
{
    public class CharacterAttributesUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _stranghtFieldPoint,_agileFieldPoint,_intelligenceFieldPoint,_wisdomFieldPoint,_staminaFieldPoint;
        public void Construct(CharacterData hero,CompositeDisposable lifetime){Bind(hero,AttributeConstants.STR,_stranghtFieldPoint,lifetime);Bind(hero,AttributeConstants.AGI,_agileFieldPoint,lifetime);Bind(hero,AttributeConstants.INT,_intelligenceFieldPoint,lifetime);Bind(hero,AttributeConstants.WIS,_wisdomFieldPoint,lifetime);Bind(hero,AttributeConstants.STM,_staminaFieldPoint,lifetime);}
        private void Bind(CharacterData hero,string key,TextMeshProUGUI field,CompositeDisposable lifetime)=>hero.Character.Attributes[key].Subscribe(v=>field.text=v.ToString()).AddTo(lifetime);
    }
}


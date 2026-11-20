
using Code.Character.Data;using Code.UI.MainMenuElements;using UniRx;using UnityEngine;
namespace Code.UI.HeroWindow
{
    public class CharacterWindowUI : MonoBehaviour
    {
        [SerializeField] private CharacterParametersUI _heroParamentresUI;
        [SerializeField] private CharacterAttributesUI _heroAttributesUI;
        [SerializeField] private CharacterEffectsUI _heroTimeEffectsUI;
        private CompositeDisposable _binding;
        public void Construct(CharacterData hero,CompositeDisposable lifetime){_binding?.Dispose();_binding=new CompositeDisposable();_binding.AddTo(lifetime);_heroParamentresUI.Construct(hero,_binding);_heroAttributesUI.Construct(hero,_binding);_heroTimeEffectsUI.Construct(hero,_binding);}
        private void OnDestroy()=>_binding?.Dispose();
    }
}

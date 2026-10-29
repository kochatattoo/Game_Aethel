
using Code.Hero.Data;using Code.UI.MainMenuElements;using UniRx;using UnityEngine;
namespace Code.UI.HeroWindow
{
    public class HeroWindowUI : MonoBehaviour
    {
        [SerializeField] private HeroParamentresUI _heroParamentresUI;
        [SerializeField] private HeroAttributesUI _heroAttributesUI;
        [SerializeField] private HeroTimeEffectsUI _heroTimeEffectsUI;
        private CompositeDisposable _binding;
        public void Construct(HeroData hero,CompositeDisposable lifetime){_binding?.Dispose();_binding=new CompositeDisposable();_binding.AddTo(lifetime);_heroParamentresUI.Construct(hero,_binding);_heroAttributesUI.Construct(hero,_binding);_heroTimeEffectsUI.Construct(hero,_binding);}
        private void OnDestroy()=>_binding?.Dispose();
    }
}

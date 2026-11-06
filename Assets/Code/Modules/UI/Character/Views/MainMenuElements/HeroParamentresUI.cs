using Code.Hero.Data;
using TMPro;
using UniRx;
using UnityEngine;
namespace Code.UI.MainMenuElements
{
    public class HeroParamentresUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _health,_mana,_attack,_defence,_evesion,_resistance;
        private CompositeDisposable _binding;
        public CharacterBalance Balance { get; set; }=new();
        public void Construct(HeroDataFabric draft,CompositeDisposable lifetime)
        {
            draft.SelectedClass.Subscribe(hero=>{if(hero==null)return;_binding?.Dispose();_binding=new CompositeDisposable();_binding.AddTo(lifetime);var stats=new HeroDerivedStats(hero,Balance);stats.AddTo(_binding);Bind(stats,_binding);}).AddTo(lifetime);
        }
        public void Construct(HeroData hero,CompositeDisposable lifetime)=>Bind(hero.Stats,lifetime);
        private void Bind(HeroDerivedStats stats,CompositeDisposable lifetime)
        {
            foreach(var unsupported in new[]{_mana,_defence,_evesion,_resistance})if(unsupported!=null)unsupported.gameObject.SetActive(false);
            stats.MaxHealth.Subscribe(v=>_health.text=$"Здоровье: {v:0.#}").AddTo(lifetime);
            stats.Damage.Subscribe(v=>_attack.text=$"Атака: {v:0.#}").AddTo(lifetime);
        }
        private void OnDestroy()=>_binding?.Dispose();
    }
}


using Code.Hero.Data;
using Code.UI.HeroWindow;
using UniRx;
using UnityEngine;
namespace Code.UI.Windows
{
    /// <summary>Представление атрибутов из донорского префаба; тестовые предметы скрыты.</summary>
    public class DemoWindow : WindowBase
    {
        [SerializeField] private HeroWindowUI _heroWindowUI;
        private readonly CompositeDisposable _lifetime=new();
        public void Construct(IHeroDataService data)
        {
            foreach(var t in GetComponentsInChildren<Transform>(true))if(t.name=="DemoElements")t.gameObject.SetActive(false);
            data.CurrentHero.Where(hero=>hero!=null).Subscribe(hero=>_heroWindowUI.Construct(hero,_lifetime)).AddTo(_lifetime);
        }
        protected override void Cleanup()=>_lifetime.Dispose();
    }
}


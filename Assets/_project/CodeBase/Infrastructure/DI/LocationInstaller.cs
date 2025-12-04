using CodeBase.Hero;
using CodeBase.Infrastructure.Hero;
using UnityEngine;
using Zenject;

namespace CodeBase.DI
{
    public class LocationInstaller : MonoInstaller
    {
        public Transform StartPoint;
        public GameObject HeroPrefab;
        public override void InstallBindings()
        {
            BindHeroController();

           // BindHeroFactory();
        }

        private void BindHeroFactory()
        {
            Container.BindFactory<Vector3, HeroMove, HeroFactory>()
                .FromComponentInNewPrefab(HeroPrefab)
                .UnderTransform(StartPoint)
                .AsSingle()
                .NonLazy();
        }

        private void BindHeroController()
        {
            HeroMove heroController = Container
                            .InstantiatePrefabForComponent<HeroMove>(HeroPrefab, StartPoint.position, Quaternion.identity, null);

            Container
                .Bind<HeroMove>()
                .FromInstance(heroController)
                .AsSingle()
                .NonLazy();
        }
    }

}

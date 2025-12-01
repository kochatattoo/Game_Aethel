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

            BindHeroFactory();
        }

        private void BindHeroFactory()
        {
            Container.BindFactory<Vector3, HeroController, HeroFactory>()
                .FromComponentInNewPrefab(HeroPrefab)
                .UnderTransform(StartPoint)
                .AsSingle()
                .NonLazy();
        }

        private void BindHeroController()
        {
            HeroController heroController = Container
                            .InstantiatePrefabForComponent<HeroController>(HeroPrefab, StartPoint.position, Quaternion.identity, null);

            Container
                .Bind<HeroController>()
                .FromInstance(heroController)
                .AsSingle()
                .NonLazy();
        }
    }

}

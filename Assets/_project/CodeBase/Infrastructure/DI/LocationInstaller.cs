using CodeBase.Hero;
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

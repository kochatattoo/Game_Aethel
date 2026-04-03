using UnityEngine;
using VFXSystem.Resolver;
using VFXSystem.Service;
namespace VFXSystem.Processors
{
    public class VFXProcessor : IVFXProcessor
    {
        private readonly IVFXFacade _facade;
        private readonly InteractResolver _resolver;

        public VFXProcessor(IVFXFacade facade)
        {
            _facade = facade;
            _resolver = new InteractResolver();
        }

        public void PlayVFX(VFXPointData data)
        {
            // Вот такие 2 API есть в фасаде
            //_facade.CreateVFX(MaterialType materialType, Vector3 point, Vector3 normal, float impactStrenght = 1);
            //_facade.CreateVFX(Material material, Vector3 point, Vector3 normal, float impactStrenght = 1);

            // Резолв 2мя методами, поиск сразу / если его нет, то по материалу обращение

            // Значит необходимо из полученного коллайдера - вытащить нужные данные 
            // Скорее всего нужна новая структура - которая будет отвечать за предоставление информации в VFX
            // Нам нужно определить следующие вещи
            // 1. Материал интеракта - возьмем его с самого Collider
            // 2. Точку - необходимо получить из точки хита
            // 3. Нормаль, вообще хз откуда достаем
            // 4. Сила удара / импакта, это параметр который передается от атаки

            Debug.Log($"{data.MaterialType}, {data.GameObject}, {data.Position}, {data.Normal}, {data.Interact}");

            Vector3 point = data.Position;
            Vector3 normal = data.Normal;
            float interact = data.Interact;

            if (data.MaterialType == BaseTypes.MaterialType.NoneDetected)
            {
                Material type = _resolver.MaterialResolve(data.GameObject);
                _facade.CreateVFX(type, point, normal, interact);
            }
            else
            {
                _facade.CreateVFX(data.MaterialType, point, normal, interact);
            }
        }
    }
}

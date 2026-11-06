using VFXSystem.Parameters.Definition;

namespace VFXSystem.Factory
{
    public interface IVFXFactory
    {
        /// <summary>
        /// Достаем VFXEntity(Сущность определяющая вызов VFX'сов) из пула 
        /// </summary>
        /// <param name="definition">SO описывающий VFX'ы и их параметры</param>
        /// <returns></returns>
        VFXEntity CreateVFXEntity(HitVFXDefinition definition);
    }
}

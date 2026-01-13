using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Factories
{
    /*
    /// <summary>
    /// Абстрактная фабрика, даёт:
    ///  - шаблонный метод BuildTree()
    ///  - базовые методы-сворки для всех стандартных узлов
    ///  - возможность в наследнике описать своё CreateRoot()
    /// </summary>
    public abstract class BehaviourFactory
    {
        /// <summary>
        /// Финальный метод, который вызывает ваш CreateRoot и возвращает
        /// готовое дерево.
        /// </summary>
        public IBehaviourNode BuildTree()
        {
            var root = CreateRoot();
            if (root == null)
                Debug.LogError("BehaviourFactory: CreateRoot вернул null!");
            return root;
        }
        /// <summary>
        /// В наследнике опишите постройку корня дерева и подпроектов.
        /// </summary>
        protected abstract BehaviourNode CreateRoot();

        #region Helpers for standard nodes

        /// <summary>
        /// Композитный нод вызывающий последовательно всех детей до первого фейла (логический элемент И).
        /// </summary>
        protected BehaviourSequence Sequence(string name, int priority = 0)
            => new(name, priority);

        /// <summary>
        /// Композитный нод последовательно вызывающий детей до первого успеха (логический элемент ИЛИ).
        /// </summary>
        protected BehaviourSelector Selector(string name, int priority = 0)
            => new(name, priority);

        /// <summary>
        /// Композитный нод приорететно вызывающий детей до первого успеха (логический элемент ИЛИ).
        /// </summary>
        protected BehaviourPrioritySelector PrioritySelector(string name, int priority = 0)
            => new(name, priority);

        /// <summary>
        /// Композитный нод случайно вызывающий детей до первого успеха (логический элемент ИЛИ).
        /// </summary>
        protected BehaviourRandomSelector RandomSelector(string name, int priority = 0)
            => new(name, priority);

        /// <summary>
        /// Композитный нод паралельно вызывающий детей пока не будет фейла.
        /// </summary>
        protected BehaviourParallel Parallel(string name, int priority = 0)
            => new(name, priority);

        /// <summary>
        /// Конечный Нод, выполняющий какую либо статегию, не имеет детей.
        /// </summary>
        protected BehaviourLeaf Leaf(string name, IStrategy strategy, int priority = 0)
            => new(name, strategy, priority);

        /// <summary>
        /// Декоратор: пропускает вызов дочернего узла только если с последнего
        /// завершённого запуска (Success или Failure) прошло >= cooldown секунд.
        /// Иначе сразу возвращает Failure.
        /// </summary>
        protected BehaviourCooldown Cooldown(string name, float seconds, int priority = 0)
            => new(name, seconds, priority);

        /// <summary>
        /// Декоратор: всегда возвращает Failure.
        /// Дочерний узел выполнится, но его результат будет выброшен.
        /// </summary>
        protected BehaviourFailer Failer(string name, int priority = 0)
            => new(name, priority);

        /// <summary>
        /// Декоратор: всегда возвращает обратный результат.
        /// </summary>
        protected BehaviourInverter Inverter(string name, int priority = 0)
            => new(name, priority);

        /// <summary>
        /// Декоратор-лимитер по времени: даёт дочернему узлу право отработать один раз 
        /// и затем «закрывает» его на заданный интервал, возвращая Running.
        /// </summary>
        protected BehaviourRateLimiter RateLimiter(string name, float intervalSeconds, int priority = 0)
            => new(name, intervalSeconds, priority);

        /// <summary>
        /// Декоратор: Зацикливает выполнение дерева
        /// </summary>
        protected BehaviourRepeater Repeater(string name, int priority = 0)
            => new(name, priority);

        /// <summary>
        /// Декоратор: всегда возвращает Success.
        /// Дочерний узел выполняется, но его результат игнорируется.
        /// </summary>
        protected BehaviourSucceeder Succeeder(string name, int priority = 0)
            => new(name, priority);

        /// <summary>
        /// Декоратор-таймаут:
        /// если дочерний узел не завершился в течение timeoutSeconds – отменяет его и возвращает Failure;
        /// если завершился – отдает его статус;
        /// пока ребенок бегает – возвращает Running.
        /// </summary>
        protected BehaviourTimeout Timeout(string name, float timeoutSeconds, int priority = 0)
            => new(name, timeoutSeconds, priority);

        /// <summary>
        /// Декоратор: выполняет ребёнка до первого Failure, пока он не провалится – возвращает Running.
        /// По провалу сбрасывает состояние и возвращает Failure.
        /// </summary>
        protected BehaviourUntilFail UntilFail(string name, int priority = 0)
            => new(name, priority);

        /// <summary>
        /// Декоратор: выполняет ребёнка до первого Success, пока он не успешен – возвращает Running.
        /// По провалу сбрасывает состояние и возвращает Success.
        /// </summary>
        protected BehaviourUntilSuccess UntilSuccess(string name, int priority = 0)
            => new(name, priority);

        /// <summary>
        /// Декоратор: ждёт первые waitDuration секунд, затем отдаёт управление ребёнку (если он есть),
        /// и после его завершения сбрасывается, чтобы на следующем входе снова ждать.
        /// </summary>
        protected BehaviourWait Wait(string name, float waitDuration, int priority = 0)
            => new(name, waitDuration, priority);

        #endregion
    } */
}

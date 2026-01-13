using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core;

namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.BehaviourTreeExtansions
{
    public static class BehaviourNodeExtensions
    {
        /// <summary>
        /// Добавляет ребёнка и возвращает сам узел, 
        /// чтобы можно было строить дерево в цепочке.
        /// </summary>
        public static T Add<T>(this T node, BehaviourNode child) where T : BehaviourNode
        {
            node.Children.Add(child);
            return node;
        }
    }
}

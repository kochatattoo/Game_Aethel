using System.Collections.Generic;

namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core
{
    public abstract class BehaviourNode: IBehaviourNode
    {
        public enum Status { Success, Failure, Running }

        public readonly string _name;
        public readonly int _priority;
        public readonly List<IBehaviourNode> Children = new();
        protected int _currentChild;
        public int Priority { get; }

        public BehaviourNode(string name = "Mode", int priority = 0)
        {
            _name = name;
            _priority = priority;
            Priority = priority;
        }

        // Вот тут стоит подумать, поскольку данный метод имеет смысл только для композитных нодов
        public void AddChild(IBehaviourNode child) => Children.Add(child);
        public virtual Status Process() => Children[_currentChild].Process();
        public virtual void Reset()
        {
            _currentChild = 0;
            foreach (var child in Children)
            {
                child.Reset();
            }
        }
    }
}


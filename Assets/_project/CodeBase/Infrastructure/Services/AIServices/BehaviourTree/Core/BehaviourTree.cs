namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Core
{
    public class BehaviourTree : BehaviourNode
    {
        public BehaviourTree(string name) : base(name) { }

        public override Status Process()
        {
            while (_currentChild < Children.Count)
            {
                var status = Children[_currentChild].Process();
                if (status != Status.Success)
                {
                    return status;
                }
                _currentChild++;
            }
            return Status.Success;
        }
    }
}


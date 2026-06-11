using UniRx;

namespace Shared.Utils.MultiBool
{
    public interface IReadOnlyMultiSourceBool
    {
        bool Current { get; }
        IReadOnlyReactiveProperty<bool> Value { get; }
    }
}
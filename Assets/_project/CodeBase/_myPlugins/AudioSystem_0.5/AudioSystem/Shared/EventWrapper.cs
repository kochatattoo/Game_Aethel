using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using System.Threading;
using UniRx;

namespace Shared.Utils
{
    public class EventWrapper<T> : IDisposable
    {
        public MultiSourceBool IsEnabled { get; } = new(true, MultiBoolDominance.FalseWins);

        private readonly Subject<T> _subject = new();

        public void Publish(T value)
        {
            if (!IsEnabled.Current)
                return;
            _subject.OnNext(value);
        }

        public UniTask<T> WaitAsync(CancellationToken token = default)
            => _subject.First().ToUniTask(cancellationToken: token);

        public IDisposable Subscribe(Action<T> onNext)
        {
            return _subject.Subscribe(onNext);
        }

        public void Dispose()
        {
            _subject?.Dispose();
            IsEnabled?.Dispose();
        }
    }

    public class EventWrapper : IDisposable
    {
        public MultiSourceBool IsEnabled { get; } = new(true, MultiBoolDominance.FalseWins);

        private readonly Subject<Unit> _subject = new();

        public void Publish()
        {
            if (!IsEnabled.Current)
                return;
            _subject.OnNext(Unit.Default);
        }

        public UniTask WaitAsync(CancellationToken token = default)
            => _subject.First().ToUniTask(cancellationToken: token);

        public IDisposable Subscribe(Action onNext)
        {
            return _subject.Subscribe(_ => onNext?.Invoke());
        }

        public void Dispose()
        {
            _subject?.Dispose();
            IsEnabled?.Dispose();
        }
    }
}

public interface IReadOnlyMultiSourceBool
{
    bool Current { get; }
    IReadOnlyReactiveProperty<bool> Value { get; }
}

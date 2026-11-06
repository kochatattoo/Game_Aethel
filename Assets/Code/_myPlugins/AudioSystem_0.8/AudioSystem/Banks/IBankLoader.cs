using Cysharp.Threading.Tasks;
using Infrastructure.AudioSystem.Events;
using Shared.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using UniRx;

namespace AudioSystem.Banks
{
    public interface IBankLoader: IDisposable
    {
        UniTask LoadBanksAsync(IEnumerable<WwiseBankAsset> banks, IProgress<float> progress = null, CancellationToken cancellationToken = default);
        UniTask UnloadBanksAsync(IEnumerable<WwiseBankAsset> banks);
        bool IsBankLoaded(WwiseBankAsset bank);

        /// <summary>Загружены ли сценовые банки.</summary>
        bool AreSceneBanksLoaded { get; }

        /// <summary>Поток, который срабатывает, когда сценовые банки загружены.</summary>
        EventWrapper OnSceneBanksLoaded { get;}
    }
}

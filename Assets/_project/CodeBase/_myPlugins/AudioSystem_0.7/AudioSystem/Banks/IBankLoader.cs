using Infrastructure.AudioSystem.Events;
using System.Collections.Generic;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace AudioSystem.Banks
{
    public interface IBankLoader
    {
        UniTask LoadBanksAsync(IEnumerable<WwiseBankAsset> banks, IProgress<float> progress = null, CancellationToken cancellationToken = default);
        UniTask UnloadBanksAsync(IEnumerable<WwiseBankAsset> banks);
        bool IsBankLoaded(WwiseBankAsset bank);
    }
}

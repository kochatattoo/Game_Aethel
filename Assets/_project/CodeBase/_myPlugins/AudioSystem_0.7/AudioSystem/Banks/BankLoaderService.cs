using Infrastructure.AudioSystem.Events;
using System.Collections.Generic;
using System;
using Zenject;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace AudioSystem.Banks
{
    public class BankLoaderService : IBankLoader, IInitializable
    {
        private readonly Dictionary<WwiseBankAsset, int> _bankRefCounts = new();
        private readonly WwiseBankAsset _initBank;

        public BankLoaderService(WwiseBankAsset initBank)
        {
            _initBank = initBank;
        }

        public void Initialize()
        {
            if (_initBank != null)
            {
                try
                {
                    _initBank.Load();
                    _bankRefCounts[_initBank] = 1;
                    Debug.Log("[Wwise] Init.bnk loaded.");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Wwise] Failed to load Init.bnk: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning("[Wwise] Init.bnk is not assigned.");
            }
        }

        public async UniTask LoadBanksAsync(IEnumerable<WwiseBankAsset> banks, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            if (banks == null)
                throw new ArgumentNullException(nameof(banks));

            var bankList = banks.Where(b => b != null).Distinct().ToList();
            if (bankList.Count == 0)
                return;

            // Определяем банки для загрузки
            var toLoad = new List<WwiseBankAsset>();
            foreach (var bank in bankList)
            {
                if (_bankRefCounts.TryGetValue(bank, out int count))
                {
                    _bankRefCounts[bank] = count + 1;
                }
                else
                {
                    _bankRefCounts[bank] = 1;
                    toLoad.Add(bank);
                }
            }

            int total = toLoad.Count;
            int loaded = 0;

            foreach (var bank in toLoad)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await LoadSingleBankAsync(bank, cancellationToken);
                loaded++;
                progress?.Report((float)loaded / total);
            }
        }

        private async UniTask LoadSingleBankAsync(WwiseBankAsset bank, CancellationToken cancellationToken)
        {
            await UniTask.RunOnThreadPool(() =>
            {
                try
                {
                    bank.Load();
                }
                catch (Exception e)
                {
                    throw new Exception($"Failed to load bank {bank.name}: {e.Message}", e);
                }
            }, cancellationToken: cancellationToken);
        }

        public async UniTask UnloadBanksAsync(IEnumerable<WwiseBankAsset> banks)
        {
            if (banks == null) return;

            var toUnload = new List<WwiseBankAsset>();
            foreach (var bank in banks.Distinct())
            {
                if (bank == null) continue;

                if (_bankRefCounts.TryGetValue(bank, out int count))
                {
                    count--;
                    if (count <= 0)
                    {
                        _bankRefCounts.Remove(bank);
                        toUnload.Add(bank);
                    }
                    else
                    {
                        _bankRefCounts[bank] = count;
                    }
                }
            }

            foreach (var bank in toUnload)
            {
                bank.Unload();
            }

            await UniTask.Yield();
        }

        public bool IsBankLoaded(WwiseBankAsset bank)
        {
            return _bankRefCounts.ContainsKey(bank) && _bankRefCounts[bank] > 0;
        }
    }
}

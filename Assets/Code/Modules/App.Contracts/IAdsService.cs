using System;

namespace CodeBase.Infrastructure.Services.Ads
{
    public interface IAdsService : IService
    {
        int Reward { get; }

        event Action RewardedVideoReady;

        void Initialize();
        void LoadAd();
        void ShowAd();
        void ShowRewardedVideo(Action onVideoFinished);
        bool IsRewardedVideoReady();
    }
}

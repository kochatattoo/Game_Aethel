using System;
using UnityEngine.Advertisements;
using UnityEngine;
using CodeBase.Infrastructure.Services.LogData;
using Zenject;

namespace CodeBase.Infrastructure.Services.Ads
{
    public class AdsService : IAdsService, IInitializable, 
        IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
    {
        private string _androidGameId;
        private string _iOSGameId;
        private string _rewardedVideoPlacementId;

        private readonly ILogDataService _logDataService;
        private string _gameID;
        public int Reward => 10;

        private event Action _onVideoFinished;
        public event Action RewardedVideoReady;

        public AdsService(ILogDataService logDataService)
        {
            _logDataService = logDataService;
        }

        public void Initialize()
        {
            SetIDs();

            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    _gameID = _androidGameId;
                    break;
                case RuntimePlatform.IPhonePlayer:
                    _gameID = _iOSGameId;
                    break;
                case RuntimePlatform.WindowsEditor:
                    _gameID = _androidGameId;
                    break;
                default:
                    Debug.Log("Unsupported platorm for adds");
                    break;
            }

            if (!Advertisement.isInitialized && Advertisement.isSupported)
            {
                Advertisement.Initialize(_gameID, true, this);
            }
        }

        // Call this public method when you want to get an ad ready to show.
        public void LoadAd()
        {
            // IMPORTANT! Only load content AFTER initialization (in this example, initialization is handled in a different script).
            Debug.Log("Loading Ad: " + _gameID);
            Advertisement.Load(_gameID, this);
        }

        // If the ad successfully loads, add a listener to the button and enable it:
        public void OnUnityAdsAdLoaded(string adUnitId)
        {
            Debug.Log("Ad Loaded: " + adUnitId);

            if (adUnitId.Equals(_gameID))
            {
                RewardedVideoReady?.Invoke();
            }
        }

        // Implement a method to execute when the user clicks the button:
        public void ShowAd()
        {
            Advertisement.Show(_rewardedVideoPlacementId, this);
        }

        public void ShowRewardedVideo(Action onVideoFinished)
        {
            _onVideoFinished = onVideoFinished;
            Advertisement.Show(_rewardedVideoPlacementId, this);
        }

        public bool IsRewardedVideoReady() => Advertisement.isInitialized;


        // Implement the Show Listener's OnUnityAdsShowComplete callback method to determine if the user gets a reward:
        public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
        {
            switch (showCompletionState)
            {
                case UnityAdsShowCompletionState.UNKNOWN:
                    Debug.Log($"OnUnityAdsShowComplete {showCompletionState}");
                    break;
                case UnityAdsShowCompletionState.SKIPPED:
                    Debug.Log($"OnUnityAdsShowComplete {showCompletionState}");
                    break;
                case UnityAdsShowCompletionState.COMPLETED:
                    _onVideoFinished?.Invoke();
                    break;
                default:
                    Debug.Log($"OnUnityAdsShowComplete {showCompletionState}");
                    break;
            }

            _onVideoFinished = null;

            if (adUnitId.Equals(_gameID) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
            {
                Debug.Log("Unity Ads Rewarded Ad Completed");
                // Grant a reward.
            }
        }

        // Implement Load and Show Listener error callbacks:
        public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
        {
            Debug.Log($"Error loading Ad Unit {adUnitId}: {error} - {message}");
            // Use the error details to determine whether to try to load another ad.
        }

        public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
        {
            Debug.Log($"Error showing Ad Unit {adUnitId}: {error} - {message}");
            // Use the error details to determine whether to try to load another ad.
        }

        public void OnUnityAdsShowStart(string adUnitId) { }
        public void OnUnityAdsShowClick(string adUnitId) { }


        public void OnInitializationComplete()
        {
            Debug.Log("Unity Ads initialization complete.");
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.Log($"Unity Ads Initialization Failed: {error} - {message}");
        }

        private void SetIDs()
        {
            _androidGameId = _logDataService.logData.androidGameID;
            Debug.Log(_androidGameId);
            _iOSGameId = _logDataService.logData.iOSGameID;
            Debug.Log(_iOSGameId);
            _rewardedVideoPlacementId = _logDataService.logData.rewardedID;
            Debug.Log(_rewardedVideoPlacementId);
        }
    }
}

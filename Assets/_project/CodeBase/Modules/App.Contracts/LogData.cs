using System;

namespace CodeBase.Infrastructure.Services.LogData
{
    [Serializable]
    public class LogData
    {
        public string androidGameID;
        public string iOSGameID;
        public string rewardedID;
    }

    [Serializable]
    public class UnityDataID
    {
        public string androidGameID;
        public string iOSGameID;
    }

    [Serializable]
    public class AdsDataID
    {
        public string rewardedID;
    }
}

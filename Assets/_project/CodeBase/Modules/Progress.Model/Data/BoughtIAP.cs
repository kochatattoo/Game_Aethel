using System;


namespace CodeBase.Data
{
    [Serializable]
    public class BoughtIAP
    {
        public string IAPid;
        public int Count;

        public BoughtIAP(string iAPid, int count = 1)
        {
            IAPid = iAPid;
            Count = count;
        }
    }
}

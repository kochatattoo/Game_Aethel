using CodeBase.Utility;
using System;


namespace CodeBase.Data
{
    [Serializable]
    public class PurchaseData
    {
        public SerializableDictionary<string, BoughtIAP> BoughtIAPs = new SerializableDictionary<string, BoughtIAP>();

        public event Action Changed;

        public void AddPurchase(string id)
        {
            if (BoughtIAPs.Dict.TryGetValue(id, out BoughtIAP boughtIAP))
                boughtIAP.Count++;
            else
                BoughtIAPs.Dict.Add(id, new BoughtIAP(id));

            Changed?.Invoke();
        }
    }
}

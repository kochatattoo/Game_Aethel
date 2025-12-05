using System;


namespace CodeBase.Data
{
    [Serializable]
    public class PlayerProgress
    {
        public State HeroState;
        public KillData KillData;
        public WorldData WorldData;
        public Stats HeroStats;
        public PurchaseData PurchaseData;

        public PlayerProgress(string initialLevel)
        {
            WorldData = new WorldData(initialLevel);
            HeroState = new State();
            HeroStats = new Stats();
            KillData = new KillData();
            PurchaseData = new PurchaseData();
        }

        public PlayerProgress()
        {
            WorldData = new WorldData();
            HeroState = new State();
            HeroStats = new Stats();
            KillData = new KillData();
            PurchaseData = new PurchaseData();
        }
    }
}

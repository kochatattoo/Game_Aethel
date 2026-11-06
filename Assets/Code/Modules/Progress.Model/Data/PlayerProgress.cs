using System;

namespace CodeBase.Data
{
    [Serializable]
    public class PlayerProgress
    {
        public int CharacterSchemaVersion;
        public Code.Hero.Data.CharacterProfileData Character;
        public State HeroState;
        public KillData KillData;
        public WorldData WorldData;
        public Stats HeroStats;
        public PurchaseData PurchaseData;

        public PlayerProgress(string initialLevel) : this(new WorldData(initialLevel))
        {
        }

        public PlayerProgress() : this(new WorldData())
        {
        }

        private PlayerProgress(WorldData worldData)
        {
            WorldData = worldData;
            HeroState = new State();
            HeroStats = new Stats();
            KillData = new KillData();
            PurchaseData = new PurchaseData();
        }
    }
}

using System;


namespace CodeBase.Data
{

        [Serializable]

        public class WorldData
        {
            public PositionOnLevel PositionOnLevel;
            public LootData LootData;

            public WorldData(string initialLevel)
            {
                PositionOnLevel = new PositionOnLevel(initialLevel);
                LootData = new LootData();
            }

            public WorldData()
            {
                PositionOnLevel = new PositionOnLevel("Level_1");
                LootData = new LootData();
            }
        } 
}

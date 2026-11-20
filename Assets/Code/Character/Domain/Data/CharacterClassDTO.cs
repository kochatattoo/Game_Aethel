using Code.Character.Attributes;

using System;
using System.Collections.Generic;

namespace Code.Character.Data
{
    [Serializable]
    public class CharacterClassDTO
    {
        public int id;
        public string name;
        public List <AttributeEntry> attributes;
        public string color;
        public string description;
        public string imagePath;   // путь к Sprite
        public List<string> abilities;
    }

    [Serializable]
    public class AttributeEntry
    {
        public string key;
        public int value;
    }
}


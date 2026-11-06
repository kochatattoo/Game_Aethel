using System;

namespace Code.Hero.Attributes
{
    public static class AttributeConstants
    {
        public const string STR = "str";
        public const string AGI = "agi";
        public const string INT = "inte";
        public const string WIS = "wis";
        public const string STM = "stm";
    }

    public enum AttributeName
    {
        STR,
        AGI,
        INT,
        WIS,
        STM
    }

    public static class AttributeExtensions
    {
        public static string ToStringValue(this AttributeName name)
        {
            return name switch
            {
                AttributeName.STR => AttributeConstants.STR,
                AttributeName.AGI => AttributeConstants.AGI,
                AttributeName.INT => AttributeConstants.INT,
                AttributeName.WIS => AttributeConstants.WIS,
                AttributeName.STM => AttributeConstants.STM,
                _ => throw new ArgumentOutOfRangeException(nameof(name))
            };
        }

        public static string ToFriendlyName(this AttributeName name) // Красивое имя для UI
        {
            return name switch
            {
                AttributeName.STR => "Сила",
                AttributeName.AGI => "Ловкость",
                AttributeName.INT => "Интеллект",
                AttributeName.WIS => "Мудрость",
                AttributeName.STM => "Стойкость",
                _ => name.ToString()
            };
        }
    }
}

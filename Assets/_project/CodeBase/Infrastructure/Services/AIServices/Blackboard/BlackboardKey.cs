using CodeBase.Infrastructure.Services.AIServices.Blackboard.BlackboardExtensions;
using System;

namespace CodeBase.Infrastructure.Services.AIServices.Blackboard
{
    [Serializable]
    public readonly struct BlackboardKey : IEquatable<BlackboardKey>
    {
        private readonly string _name;
        private readonly int _hashedKey;

        public BlackboardKey(string name)
        {
            _name = name;
            _hashedKey = name.ComputeFNV1aHash();
        }

        public bool Equals(BlackboardKey other) => _hashedKey == other._hashedKey;

        public override bool Equals(object obj) => obj is BlackboardKey other && Equals(other);
        public override int GetHashCode() => _hashedKey;
        public override string ToString() => _name;

        public static bool operator ==(BlackboardKey left, BlackboardKey right) => left._hashedKey == right._hashedKey;
        public static bool operator !=(BlackboardKey left, BlackboardKey right) => !(left == right);
    }
}

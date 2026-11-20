using System;
using System.Collections.Generic;
using PetProject.Navigation;

namespace PetProject.Encounter
{
    public readonly struct EncounterGroupMember
    {
        public string Id { get; }
        public string GroupId { get; }
        public NavigationPoint Position { get; }
        public bool IsAlive { get; }

        public EncounterGroupMember(string id, string groupId, NavigationPoint position, bool isAlive)
        {
            Id = id;
            GroupId = groupId;
            Position = position;
            IsAlive = isAlive;
        }
    }

    public static class EncounterGroupResolver
    {
        public static IReadOnlyList<string> Resolve(IEnumerable<EncounterGroupMember> members, string groupId,
            NavigationPoint origin, float radius)
        {
            if (members == null) throw new ArgumentNullException(nameof(members));
            if (string.IsNullOrWhiteSpace(groupId)) throw new ArgumentException("Group ID required", nameof(groupId));
            if (radius < 0) throw new ArgumentOutOfRangeException(nameof(radius));

            var result = new List<string>();
            foreach (var member in members)
            {
                if (!member.IsAlive || member.GroupId != groupId) continue;
                if (NavigationPoint.Distance(origin, member.Position) <= radius) result.Add(member.Id);
            }
            return result;
        }
    }
}

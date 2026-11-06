using System;
using System.Collections.Generic;
using Code.Hero.Data;
using UnityEngine;

namespace CodeBase.CharacterIntegration
{
    internal sealed class CharacterClassCatalog : IDisposable
    {
        private readonly Dictionary<int, HeroClass> _definitions = new();

        public IReadOnlyDictionary<int, HeroClass> Definitions => _definitions;

        public void Load(string json)
        {
            var catalog = JsonUtility.FromJson<HeroClassesRoot>(json);
            if (catalog?.heroClasses == null || catalog.heroClasses.Count == 0)
                throw new InvalidOperationException("Пустой каталог классов");
            foreach (var definition in catalog.heroClasses)
                if (!_definitions.TryAdd(definition.id, new HeroClass(definition)))
                    throw new InvalidOperationException("Повторный Id класса");
        }

        public void Dispose()
        {
            foreach (var definition in _definitions.Values) definition.Dispose();
            _definitions.Clear();
        }
    }
}

using System;
using CodeBase.StaticData;
using NUnit.Framework;
using UnityEngine;

namespace PetProject.Architecture.Tests
{
    public sealed class LevelDataValidationTests
    {
        [Test] public void ExactDuplicateSpawnersKeepOriginalSaveIdentity()
        {
            var first = new EnemySpawnerData("old-save-id", MonsterTypeID.Sekeleton, Vector3.one);
            var duplicate = new EnemySpawnerData("old-save-id", MonsterTypeID.Sekeleton, Vector3.one);
            var result = LevelDataValidation.UniqueSpawners(new[] { first, duplicate });
            Assert.That(result.Count, Is.EqualTo(1)); Assert.That(result[0], Is.SameAs(first));
        }
        [Test] public void ConflictingIdsCannotHideAnotherSpawner()
        {
            var first = new EnemySpawnerData("same", MonsterTypeID.Sekeleton, Vector3.zero);
            var conflict = new EnemySpawnerData("same", MonsterTypeID.Sekeleton, Vector3.one);
            Assert.Throws<ArgumentException>(() => LevelDataValidation.UniqueSpawners(new[] { first, conflict }));
        }
        [Test] public void EmptyIdIsRejected()
        {
            Assert.Throws<ArgumentException>(() => LevelDataValidation.UniqueSpawners(new[] { new EnemySpawnerData("", MonsterTypeID.Sekeleton, Vector3.zero) }));
        }
    }
}

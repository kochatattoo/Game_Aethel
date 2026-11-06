using System;
using NUnit.Framework;

namespace PetProject.Tactics.Tests
{
    public sealed class RandomRulesTests
    {
        [Test] public void VersionOneReferenceStreamIsStable()
        {
            var random = new DeterministicRandom(1);
            var expected = new uint[] { 270369, 67634689, 2647435461 };
            foreach (var state in expected)
            {
                var roll = random.NextInt(1, 21);
                Assert.That(random.State, Is.EqualTo(state));
                Assert.That(roll, Is.InRange(1, 20));
            }
        }

        [Test] public void CapturedStateContinuesSameStream()
        {
            var original = new DeterministicRandom(718);
            for (var i = 0; i < 10; ++i) original.NextInt(1, 21);
            var resumed = new DeterministicRandom(original.State);
            for (var i = 0; i < 100; ++i) Assert.That(resumed.NextInt(1, 21), Is.EqualTo(original.NextInt(1, 21)));
        }

        [Test] public void InvalidRangeDoesNotConsumeRandomness()
        {
            var random = new DeterministicRandom(718); var state = random.State;
            Assert.Throws<ArgumentOutOfRangeException>(() => random.NextInt(2, 2));
            Assert.That(random.State, Is.EqualTo(state));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DeterministicRandom(0));
        }

        [Test] public void FullSignedRangeIsSafe()
        {
            var random = new DeterministicRandom(19);
            for (var i = 0; i < 1000; ++i)
                Assert.That(random.NextInt(int.MinValue, int.MaxValue), Is.InRange(int.MinValue, int.MaxValue - 1));
        }
    }
}

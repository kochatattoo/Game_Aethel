using System;
using System.Collections.Generic;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.ObjectPool;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.SaveLoad;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PetProject.Architecture.Tests
{
    public sealed class ArchitectureRegressionTests
    {
        private GameObject _prefab;
        [SetUp] public void SetUp() => _prefab = new GameObject("Pool probe", typeof(PoolProbe));
        [TearDown] public void TearDown() => Object.DestroyImmediate(_prefab);

        [Test] public void DoubleReturnDoesNotDuplicateAvailableInstance()
        {
            using var pool = new Pool<PoolProbe>(_prefab.GetComponent<PoolProbe>(), 1);
            var instance = pool.Spawn(); pool.Despawn(instance); pool.Despawn(instance);
            Assert.That(pool.FreeCount, Is.EqualTo(1));
            Assert.That(instance.Returns, Is.EqualTo(1));
            var first = pool.SpawnExpandable(); var second = pool.SpawnExpandable();
            Assert.That(second, Is.Not.SameAs(first));
        }
        [Test] public void BoundedPoolRecyclesOldestActiveObject()
        {
            using var pool = new Pool<PoolProbe>(_prefab.GetComponent<PoolProbe>(), 1);
            var first = pool.Spawn(); var recycled = pool.Spawn();
            Assert.That(recycled, Is.SameAs(first));
            Assert.That(first.Spawns, Is.EqualTo(2)); Assert.That(first.Returns, Is.EqualTo(1));
        }
        [Test] public void ExpandablePoolPreservesActiveObjects()
        {
            using var pool = new Pool<PoolProbe>(_prefab.GetComponent<PoolProbe>(), 1);
            var first = pool.SpawnExpandable(); var second = pool.SpawnExpandable();
            Assert.That(second, Is.Not.SameAs(first)); Assert.That(first.Returns, Is.Zero);
            pool.DespawnAllActive(); Assert.That(pool.FreeCount, Is.EqualTo(2));
        }
        [Test] public void FailedInitializerReturnsAcquiredObject()
        {
            using var pool = new Pool<PoolProbe>(_prefab.GetComponent<PoolProbe>(), 1);
            Assert.Throws<InvalidOperationException>(() => pool.Spawn(initializer: _ => throw new InvalidOperationException()));
            Assert.That(pool.FreeCount, Is.EqualTo(1));
        }
        [Test] public void ReturningToWrongPoolHasNoEffect()
        {
            using var first = new Pool<PoolProbe>(_prefab.GetComponent<PoolProbe>());
            using var second = new Pool<PoolProbe>(_prefab.GetComponent<PoolProbe>());
            var instance = first.SpawnExpandable(); second.Despawn(instance);
            Assert.That(second.FreeCount, Is.Zero); Assert.That(instance.gameObject.activeSelf, Is.True);
        }
        [Test] public void DisposedPoolCannotSpawnAndReleasesOwnedRoot()
        {
            var pool = new Pool<PoolProbe>(_prefab.GetComponent<PoolProbe>());
            var instance = pool.Spawn(); var root = instance.transform.parent;
            pool.Dispose(); pool.Dispose();
            Assert.That(root == null, Is.True); Assert.That(instance == null, Is.True);
            Assert.Throws<ObjectDisposedException>(() => pool.Spawn());
        }
        [Test] public void PrewarmDoesNotInvokeGameplayCallbacks()
        {
            var pools = new Dictionary<Type, IPool>();
            pools.AddPoolAsync(_prefab.GetComponent<PoolProbe>(), 5, false).GetAwaiter().GetResult();
            using var pool = (Pool<PoolProbe>)pools[typeof(PoolProbe)];
            Assert.That(pool.FreeCount, Is.EqualTo(5));
            var instance = pool.Spawn();
            Assert.That(instance.Spawns, Is.EqualTo(1)); Assert.That(instance.Returns, Is.Zero);
        }
        [Test] public void RegistryDeduplicatesAndUnregistersReaders()
        {
            var registry = new ProgressRegistry(); var reader = new Reader();
            registry.Register(reader); registry.Register(reader); registry.Capture(new PlayerProgress("test"));
            Assert.That(reader.Saves, Is.EqualTo(1)); Assert.That(registry.Readers.Count, Is.EqualTo(1));
            registry.Unregister(reader); registry.Capture(new PlayerProgress("test"));
            Assert.That(reader.Saves, Is.EqualTo(1));
        }
        [Test] public void RegistryAllowsUnregisterDuringCapture()
        {
            var registry = new ProgressRegistry(); var first = new Reader(); var second = new Reader();
            first.Saving = () => registry.Unregister(first); registry.Register(first); registry.Register(second);
            registry.Capture(new PlayerProgress("test")); registry.Capture(new PlayerProgress("test"));
            Assert.That(first.Saves, Is.EqualTo(1)); Assert.That(second.Saves, Is.EqualTo(2));
        }
        [Test] public void AsyncRestoreUsesAsyncContractExactlyOnce()
        {
            var registry = new ProgressRegistry(); var reader = new AsyncReader(); registry.Register(reader);
            registry.ApplyAsync(new PlayerProgress("test")).GetAwaiter().GetResult();
            Assert.That(reader.AsyncLoads, Is.EqualTo(1)); Assert.That(reader.Loads, Is.Zero);
        }
        [Test] public void RegistryClearDisconnectsPreviousWorld()
        {
            var registry = new ProgressRegistry(); var reader = new Reader(); registry.Register(reader); registry.Clear();
            registry.Apply(new PlayerProgress("test")); registry.Capture(new PlayerProgress("test"));
            Assert.That(reader.Loads + reader.Saves, Is.Zero);
        }
        private class Reader : ISavedProgress
        {
            public int Loads, Saves; public Action Saving;
            public void LoadProgress(PlayerProgress progress) => ++Loads;
            public void UpdateProgress(PlayerProgress progress) { ++Saves; Saving?.Invoke(); }
        }
        private sealed class AsyncReader : Reader, IAsyncProgressReader
        {
            public int AsyncLoads;
            public UniTask LoadProgressAsync(PlayerProgress progress) { ++AsyncLoads; return UniTask.CompletedTask; }
        }
    }
}

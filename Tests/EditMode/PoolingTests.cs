using Mossmallow.Systems.Pooling;
using NUnit.Framework;
using UnityEngine;

namespace Mossmallow.Systems.Tests
{
    public class PoolingTests
    {
        PrefabPool pool;
        GameObject prefab;

        [SetUp]
        public void SetUp()
        {
            pool = new PrefabPool();
            prefab = new GameObject("TestPrefab");
        }

        [TearDown]
        public void TearDown()
        {
            pool.Clear();
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void Spawn_PlacesActiveInstance()
        {
            var go = pool.Spawn(prefab, new Vector3(1f, 2f, 3f), Quaternion.identity);

            Assert.IsTrue(go.activeSelf);
            Assert.AreEqual(new Vector3(1f, 2f, 3f), go.transform.position);
            Assert.AreEqual(1, pool.CountActive);
        }

        [Test]
        public void Release_ThenSpawn_ReusesSameInstance()
        {
            var first = pool.Spawn(prefab, Vector3.zero, Quaternion.identity);
            pool.Release(first);

            Assert.IsFalse(first.activeSelf);
            Assert.AreEqual(1, pool.CountFree(prefab));

            var second = pool.Spawn(prefab, Vector3.zero, Quaternion.identity);
            Assert.AreSame(first, second);
            Assert.AreEqual(0, pool.CountFree(prefab));
        }

        [Test]
        public void ReleasingTwice_DoesNotDuplicateInstance()
        {
            var go = pool.Spawn(prefab, Vector3.zero, Quaternion.identity);
            pool.Release(go);
            pool.Release(go);

            Assert.AreEqual(1, pool.CountFree(prefab));
        }

        [Test]
        public void DestroyedInstance_IsSkipped()
        {
            var go = pool.Spawn(prefab, Vector3.zero, Quaternion.identity);
            pool.Release(go);
            Object.DestroyImmediate(go); // e.g. unloaded with a scene

            var fresh = pool.Spawn(prefab, Vector3.zero, Quaternion.identity);
            Assert.IsTrue(fresh != null);
            Assert.IsTrue(fresh.activeSelf);
        }

        [Test]
        public void Release_ForeignObject_DestroysIt()
        {
            var foreign = new GameObject("Foreign");
            pool.Release(foreign);

            Assert.IsTrue(foreign == null);
        }
    }
}

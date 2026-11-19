using System;
using System.IO;
using Mossmallow.Systems.Saving;
using NUnit.Framework;
using UnityEngine;

namespace Mossmallow.Systems.Tests
{
    public class SavingTests
    {
        [Serializable]
        class TestSave : SaveData
        {
            public int coins;
            public string playerName;
        }

        string dir;
        string file;

        [SetUp]
        public void SetUp()
        {
            dir = Path.Combine(Path.GetTempPath(), "mossmallow_save_tests_" + Guid.NewGuid().ToString("N"));
            file = Path.Combine(dir, "slot_1.json");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }

        [Test]
        public void Load_WithoutFile_ReturnsFreshDataAtCurrentVersion()
        {
            var data = new VersionedJsonStore<TestSave>(file, 3).Load();

            Assert.AreEqual(3, data.version);
            Assert.AreEqual(0, data.coins);
        }

        [Test]
        public void SaveThenLoad_RoundTrips()
        {
            var store = new VersionedJsonStore<TestSave>(file, 1);
            store.Save(new TestSave { coins = 42, playerName = "Ada" });

            var loaded = store.Load();
            Assert.AreEqual(42, loaded.coins);
            Assert.AreEqual("Ada", loaded.playerName);
            Assert.IsFalse(File.Exists(file + ".tmp"));
        }

        [Test]
        public void SecondSave_KeepsPreviousFileAsBackup()
        {
            var store = new VersionedJsonStore<TestSave>(file, 1);
            store.Save(new TestSave { coins = 1 });
            store.Save(new TestSave { coins = 2 });

            Assert.IsTrue(File.Exists(store.BackupPath));
            var backup = JsonUtility.FromJson<TestSave>(File.ReadAllText(store.BackupPath));
            Assert.AreEqual(1, backup.coins);
        }

        [Test]
        public void CorruptMainFile_FallsBackToBackup()
        {
            var store = new VersionedJsonStore<TestSave>(file, 1);
            store.Save(new TestSave { coins = 7 });
            store.Save(new TestSave { coins = 8 });
            File.WriteAllText(file, "{ not valid json");

            var loaded = store.Load();

            Assert.AreEqual(7, loaded.coins);
        }

        [Test]
        public void OldVersion_RunsMigrationsInOrder()
        {
            Directory.CreateDirectory(dir);
            File.WriteAllText(file, "{\"version\":1,\"coins\":10,\"playerName\":\"\"}");

            var store = new VersionedJsonStore<TestSave>(file, 3)
                .AddMigration(1, d => d.coins *= 2)            // v1 -> v2: currency was halved
                .AddMigration(2, d => d.playerName = "Player"); // v2 -> v3: name added

            var loaded = store.Load();
            Assert.AreEqual(3, loaded.version);
            Assert.AreEqual(20, loaded.coins);
            Assert.AreEqual("Player", loaded.playerName);
        }
    }
}

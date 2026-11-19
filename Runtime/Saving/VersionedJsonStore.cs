using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Mossmallow.Systems.Saving
{
    /// <summary>
    /// JSON save file with a schema version, step-by-step migrations and crash-safe writes.
    /// Each write goes to a .tmp file first and the previous file is kept as .bak,
    /// so a crash mid-write never leaves the player without a readable save.
    /// </summary>
    public sealed class VersionedJsonStore<T> where T : SaveData, new()
    {
        readonly string path;
        readonly int currentVersion;
        readonly Dictionary<int, Action<T>> migrations = new Dictionary<int, Action<T>>();

        public string FilePath => path;
        public string BackupPath => path + ".bak";
        public int CurrentVersion => currentVersion;

        public VersionedJsonStore(string path, int currentVersion)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("Save path is empty.", nameof(path));
            if (currentVersion < 1) throw new ArgumentOutOfRangeException(nameof(currentVersion));
            this.path = path;
            this.currentVersion = currentVersion;
        }

        /// <summary>Registers an upgrade from <paramref name="fromVersion"/> to fromVersion + 1.</summary>
        public VersionedJsonStore<T> AddMigration(int fromVersion, Action<T> migrate)
        {
            migrations[fromVersion] = migrate ?? throw new ArgumentNullException(nameof(migrate));
            return this;
        }

        /// <summary>Loads the save, falling back to the backup, then to a fresh object. Older versions are migrated in order.</summary>
        public T Load()
        {
            T data = Read(path) ?? Read(BackupPath);
            if (data == null) return new T { version = currentVersion };

            if (data.version > currentVersion)
                Debug.LogWarning($"[Save] {path} is version {data.version}, newer than {currentVersion}. Loaded as-is.");

            while (data.version < currentVersion)
            {
                if (migrations.TryGetValue(data.version, out var migrate)) migrate(data);
                data.version++;
            }
            return data;
        }

        public void Save(T data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            data.version = currentVersion;

            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            string tmp = path + ".tmp";
            File.WriteAllText(tmp, JsonUtility.ToJson(data, true));
            if (File.Exists(path)) File.Replace(tmp, path, BackupPath);
            else File.Move(tmp, path);
        }

        static T Read(string file)
        {
            try
            {
                if (!File.Exists(file)) return null;
                string json = File.ReadAllText(file);
                if (string.IsNullOrWhiteSpace(json)) return null;
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Save] Could not read {file}: {e.Message}");
                return null;
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Mossmallow.Systems.Pooling
{
    /// <summary>
    /// GameObject pool keyed by prefab, for short-lived objects (hit sparks, projectiles, debris).
    /// Instances destroyed by a scene change are skipped instead of being handed out again.
    /// </summary>
    public sealed class PrefabPool
    {
        readonly Dictionary<GameObject, Stack<GameObject>> free = new Dictionary<GameObject, Stack<GameObject>>();
        readonly Dictionary<GameObject, GameObject> owner = new Dictionary<GameObject, GameObject>();
        Transform root;

        public static PrefabPool Shared { get; } = new PrefabPool();

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null) return null;
            if (!free.TryGetValue(prefab, out var stack)) free[prefab] = stack = new Stack<GameObject>();

            GameObject go = null;
            while (stack.Count > 0 && go == null) go = stack.Pop(); // Unity's null check skips destroyed instances

            if (go == null)
            {
                go = Object.Instantiate(prefab, Root);
                go.SetActive(false);
                owner[go] = prefab;
            }

            go.transform.SetParent(parent != null ? parent : Root, false);
            go.transform.SetPositionAndRotation(position, rotation);
            go.SetActive(true);
            return go;
        }

        /// <summary>Returns an instance to its pool. Objects that did not come from this pool are destroyed.</summary>
        public void Release(GameObject go)
        {
            if (go == null) return;
            if (!owner.TryGetValue(go, out var prefab))
            {
                DestroySafe(go);
                return;
            }
            if (!go.activeSelf) return; // already released
            go.SetActive(false);
            go.transform.SetParent(Root, false);
            free[prefab].Push(go);
        }

        public int CountFree(GameObject prefab) => free.TryGetValue(prefab, out var s) ? s.Count : 0;

        public int CountActive
        {
            get
            {
                int n = 0;
                foreach (var kv in owner) if (kv.Key != null && kv.Key.activeSelf) n++;
                return n;
            }
        }

        /// <summary>Destroys every pooled instance and forgets all prefabs.</summary>
        public void Clear()
        {
            foreach (var kv in owner) if (kv.Key != null) DestroySafe(kv.Key);
            owner.Clear();
            free.Clear();
            if (root != null) DestroySafe(root.gameObject);
            root = null;
        }

        Transform Root
        {
            get
            {
                if (root == null) root = new GameObject("[PrefabPool]").transform;
                return root;
            }
        }

        static void DestroySafe(Object o)
        {
            if (Application.isPlaying) Object.Destroy(o);
            else Object.DestroyImmediate(o);
        }
    }
}

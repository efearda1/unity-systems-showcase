using UnityEngine;

namespace Mossmallow.Systems.Pooling
{
    /// <summary>Puts a pooled instance back into <see cref="PrefabPool.Shared"/> after a fixed lifetime.</summary>
    public sealed class ReleaseAfter : MonoBehaviour
    {
        [Min(0f)] public float lifetime = 1f;
        float timer;

        void OnEnable() => timer = 0f;

        void Update()
        {
            timer += Time.deltaTime;
            if (timer >= lifetime) PrefabPool.Shared.Release(gameObject);
        }
    }
}

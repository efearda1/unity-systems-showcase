using UnityEngine;

namespace Mossmallow.Systems.Shake
{
    /// <summary>
    /// Trauma-based shake: hits add trauma (0..1), trauma decays linearly, and the visible shake is
    /// trauma squared times smooth Perlin noise. Small hits stay subtle, big hits read clearly.
    /// </summary>
    public sealed class Trauma
    {
        public float Value { get; private set; }
        public float DecayPerSecond { get; set; }

        public Trauma(float decayPerSecond = 2.2f) => DecayPerSecond = decayPerSecond;

        public float Intensity => Value * Value;

        public void Add(float amount) => Value = Mathf.Clamp01(Value + amount);

        public void Tick(float deltaTime) => Value = Mathf.Max(0f, Value - DecayPerSecond * deltaTime);

        /// <summary>Noise in [-1, 1] scaled by <see cref="Intensity"/>. Use a different channel per axis.</summary>
        public float Sample(float time, float frequency, int channel)
        {
            if (Value <= 0f) return 0f;
            float n = Mathf.PerlinNoise(channel * 17.31f, time * frequency) * 2f - 1f;
            return Intensity * Mathf.Clamp(n, -1f, 1f);
        }
    }
}

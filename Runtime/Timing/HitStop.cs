using UnityEngine;

namespace Mossmallow.Systems.Timing
{
    /// <summary>
    /// Drives Time.timeScale from a <see cref="HitStopClock"/>. Counts in unscaled time, so the freeze ends on schedule.
    /// The helper object exists only while a hit-stop is running and restores the previous time scale when done.
    /// </summary>
    public sealed class HitStop : MonoBehaviour
    {
        static HitStop instance;
        static readonly HitStopClock clock = new HitStopClock();

        /// <summary>Settings-menu multiplier (0 disables hit-stop).</summary>
        public static float Strength = 1f;

        float baseScale = 1f;

        public static bool IsActive => instance != null;

        public static void Freeze(float seconds, float tailSeconds = 0f)
        {
            seconds *= Strength;
            if (seconds <= 0f || !Application.isPlaying) return;
            if (instance == null)
            {
                var go = new GameObject("[HitStop]");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<HitStop>();
                instance.baseScale = Time.timeScale;
            }
            clock.Request(Time.unscaledTime, seconds, tailSeconds * Strength);
            Time.timeScale = instance.baseScale * clock.Scale(Time.unscaledTime);
        }

        void Update()
        {
            Time.timeScale = baseScale * clock.Scale(Time.unscaledTime);
            if (!clock.IsActive(Time.unscaledTime)) Destroy(gameObject);
        }

        void OnDestroy()
        {
            if (instance != this) return;
            Time.timeScale = baseScale;
            clock.Reset();
            instance = null;
        }
    }
}

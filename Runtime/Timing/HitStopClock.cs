using System;

namespace Mossmallow.Systems.Timing
{
    /// <summary>
    /// Timing logic for hit-stop: a short freeze followed by an optional slow-motion tail.
    /// Overlapping requests extend to the latest end time instead of stacking.
    /// Time is passed in, so it runs on unscaled time and is easy to test.
    /// </summary>
    public sealed class HitStopClock
    {
        public float TailScale { get; }

        float freezeUntil = float.NegativeInfinity;
        float tailUntil = float.NegativeInfinity;

        public HitStopClock(float tailScale = 0.3f)
        {
            if (tailScale < 0f || tailScale > 1f) throw new ArgumentOutOfRangeException(nameof(tailScale));
            TailScale = tailScale;
        }

        public void Request(float now, float freezeSeconds, float tailSeconds = 0f)
        {
            if (freezeSeconds <= 0f) return;
            freezeUntil = Math.Max(freezeUntil, now + freezeSeconds);
            if (tailSeconds > 0f) tailUntil = Math.Max(tailUntil, freezeUntil + tailSeconds);
        }

        /// <summary>0 while frozen, <see cref="TailScale"/> during the tail, 1 otherwise.</summary>
        public float Scale(float now)
        {
            if (now < freezeUntil) return 0f;
            if (now < tailUntil) return TailScale;
            return 1f;
        }

        public bool IsActive(float now) => now < freezeUntil || now < tailUntil;

        public void Reset()
        {
            freezeUntil = float.NegativeInfinity;
            tailUntil = float.NegativeInfinity;
        }
    }
}

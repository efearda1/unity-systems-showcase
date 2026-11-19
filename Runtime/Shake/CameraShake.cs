using UnityEngine;

namespace Mossmallow.Systems.Shake
{
    /// <summary>
    /// Put this on a child of the camera rig. It only writes a local offset around the rest pose,
    /// so it never fights the follow logic on the parent.
    /// </summary>
    public sealed class CameraShake : MonoBehaviour
    {
        public static CameraShake Main { get; private set; }

        /// <summary>Settings-menu multiplier (0 = shake off).</summary>
        public static float Scale = 1f;

        [SerializeField] float maxOffset = 0.3f;
        [SerializeField] float maxAngle = 2f;
        [SerializeField] float frequency = 22f;
        [SerializeField] float decayPerSecond = 2.2f;

        readonly Trauma trauma = new Trauma();
        Vector3 restPosition;
        Quaternion restRotation;

        public float CurrentTrauma => trauma.Value;

        public static void Add(float amount)
        {
            if (Main != null) Main.trauma.Add(amount * Scale);
        }

        void OnEnable()
        {
            Main = this;
            restPosition = transform.localPosition;
            restRotation = transform.localRotation;
        }

        void OnDisable()
        {
            transform.localPosition = restPosition;
            transform.localRotation = restRotation;
            if (Main == this) Main = null;
        }

        void LateUpdate()
        {
            trauma.DecayPerSecond = decayPerSecond;
            trauma.Tick(Time.unscaledDeltaTime); // keeps shaking during hit-stop
            float t = Time.unscaledTime;
            var offset = new Vector3(trauma.Sample(t, frequency, 0), trauma.Sample(t, frequency, 1), 0f) * maxOffset;
            float roll = trauma.Sample(t, frequency, 2) * maxAngle;
            transform.localPosition = restPosition + offset;
            transform.localRotation = restRotation * Quaternion.Euler(0f, 0f, roll);
        }
    }
}

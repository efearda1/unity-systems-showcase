using Mossmallow.Systems.Shake;
using NUnit.Framework;

namespace Mossmallow.Systems.Tests
{
    public class ShakeTests
    {
        [Test]
        public void Add_ClampsToOne()
        {
            var t = new Trauma();
            t.Add(0.7f);
            t.Add(0.7f);

            Assert.AreEqual(1f, t.Value);
        }

        [Test]
        public void Tick_DecaysLinearlyAndStopsAtZero()
        {
            var t = new Trauma(2f);
            t.Add(1f);

            t.Tick(0.25f);
            Assert.AreEqual(0.5f, t.Value, 1e-5f);

            t.Tick(1f);
            Assert.AreEqual(0f, t.Value);
        }

        [Test]
        public void Intensity_IsTraumaSquared()
        {
            var t = new Trauma();
            t.Add(0.5f);

            Assert.AreEqual(0.25f, t.Intensity, 1e-5f);
        }

        [Test]
        public void Sample_StaysWithinIntensity()
        {
            var t = new Trauma();
            t.Add(0.6f);

            for (int i = 0; i < 200; i++)
            {
                float s = t.Sample(i * 0.013f, 22f, i % 3);
                Assert.LessOrEqual(System.Math.Abs(s), t.Intensity + 1e-5f);
            }
        }

        [Test]
        public void Sample_IsZeroWithoutTrauma()
        {
            Assert.AreEqual(0f, new Trauma().Sample(1.23f, 22f, 0));
        }
    }
}

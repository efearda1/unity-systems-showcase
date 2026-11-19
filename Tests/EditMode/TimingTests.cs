using Mossmallow.Systems.Timing;
using NUnit.Framework;

namespace Mossmallow.Systems.Tests
{
    public class TimingTests
    {
        [Test]
        public void Idle_ScaleIsOne()
        {
            var clock = new HitStopClock();
            Assert.AreEqual(1f, clock.Scale(0f));
            Assert.IsFalse(clock.IsActive(0f));
        }

        [Test]
        public void Freeze_ThenTail_ThenNormal()
        {
            var clock = new HitStopClock(0.3f);
            clock.Request(10f, 0.1f, 0.2f);

            Assert.AreEqual(0f, clock.Scale(10.05f));
            Assert.AreEqual(0.3f, clock.Scale(10.2f));
            Assert.AreEqual(1f, clock.Scale(10.31f));
            Assert.IsFalse(clock.IsActive(10.31f));
        }

        [Test]
        public void OverlappingRequests_ExtendToLatestEnd()
        {
            var clock = new HitStopClock();
            clock.Request(0f, 0.1f);
            clock.Request(0.05f, 0.1f); // ends at 0.15, not 0.2

            Assert.AreEqual(0f, clock.Scale(0.14f));
            Assert.AreEqual(1f, clock.Scale(0.16f));
        }

        [Test]
        public void ShorterRequest_DoesNotCutLongerFreeze()
        {
            var clock = new HitStopClock();
            clock.Request(0f, 0.5f);
            clock.Request(0.1f, 0.05f);

            Assert.AreEqual(0f, clock.Scale(0.4f));
        }

        [Test]
        public void ZeroDuration_IsIgnored()
        {
            var clock = new HitStopClock();
            clock.Request(0f, 0f, 1f);

            Assert.AreEqual(1f, clock.Scale(0.5f));
        }
    }
}

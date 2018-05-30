using System;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class OnTimeoutTest
    {
        private RetryHelper _target;

        [SetUp]
        public void SetUp()
        {
            _target = new RetryHelper();
            _target.DefaultTryInterval = TimeSpan.FromMilliseconds(RetryHelperTest.Interval);
        }

        [Test]
        [Timeout(1000)]
        public void TestOnTimeoutShouldNotFire()
        {
            var times = 5;
            var generator = new Generator(times);
            _target.Try(() => generator.Next())
                   .OnTimeout(t => Assert.Fail())
                   .Until(t => t);
        }

        [Test]
        [Timeout(1000)]
        public void TestOnTimeoutWithNoParameter()
        {
            var times = 5;
            var generator = new Generator(times);
            var onTimeoutTriggered = false;
            Assert.That(() =>
                _target.Try(() => generator.Next())
                       .WithMaxTryCount(times - 1)
                       .OnTimeout(() => onTimeoutTriggered = true)
                       .Until(t => t),
                Throws.TypeOf<TimeoutException>());
            Assert.That(onTimeoutTriggered, Is.True);
        }

        [Test]
        [Timeout(1000)]
        public void TestOnTimeoutAfterFiveTimes()
        {
            var times = 5;
            var generator = new Generator(times);
            var onTimeoutTriggered = false;
            Assert.That(() =>
                _target.Try(() => generator.Next())
                       .WithMaxTryCount(times - 1)
                       .OnTimeout(t =>
                       {
                           Assert.IsFalse(t);
                           onTimeoutTriggered = true;
                       })
                       .Until(t => t),
                Throws.TypeOf<TimeoutException>());
            Assert.That(onTimeoutTriggered, Is.True);
        }

        [Test]
        [Timeout(1000)]
        public void TestOnTimeoutWithTriedCount()
        {
            var times = 5;
            var generator = new Generator(times);
            var onTimeoutTriggered = false;
            var maxTryCount = times - 1;
            Assert.That(() =>
                _target.Try(() => generator.Next())
                       .WithMaxTryCount(maxTryCount)
                       .OnTimeout((t, count) =>
                       {
                           Assert.That(count, Is.EqualTo(maxTryCount));
                           onTimeoutTriggered = true;
                       })
                       .Until(t => t),
                Throws.TypeOf<TimeoutException>());
            Assert.That(onTimeoutTriggered, Is.True);
        }

        [Test]
        [Timeout(1000)]
        public void TestMultipleOnTimeout()
        {
            var times = 5;
            var generator = new Generator(times);
            var onTimeoutTriggered1 = false;
            var onTimeoutTriggered2 = false;
            Assert.That(() =>
                _target.Try(() => generator.Next())
                       .WithMaxTryCount(times - 1)
                       .OnTimeout(t => onTimeoutTriggered1 = true)
                       .OnTimeout(t => onTimeoutTriggered2 = true)
                       .Until(t => t),
                Throws.TypeOf<TimeoutException>());
            Assert.That(onTimeoutTriggered1, Is.True);
            Assert.That(onTimeoutTriggered2, Is.True);
        }
    }
}
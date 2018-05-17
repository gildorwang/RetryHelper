using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class OnTimeoutAsyncTest
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
        public async Task TestOnTimeoutShouldNotFireAsync()
        {
            var times = 5;
            var generator = new Generator(times);
            var onTimeoutTriggered = false;
            await _target.TryAsync(() => generator.Next())
                   .OnTimeout(t => onTimeoutTriggered = true)
                   .Until(t => t);
            Assert.That(onTimeoutTriggered, Is.False);
        }

        [Test]
        [Timeout(1000)]
        public void TestOnTimeoutAfterFiveTimesAsync()
        {
            var times = 5;
            var generator = new Generator(times);
            var onTimeoutTriggered = false;
            Assert.ThrowsAsync<TimeoutException>(() =>
                _target.TryAsync(() => generator.Next())
                    .WithMaxTryCount(times - 1)
                    .OnTimeout(t => onTimeoutTriggered = true)
                    .Until(t => t));
            Assert.That(onTimeoutTriggered, Is.True);
        }

        [Test]
        [Timeout(1000)]
        public void TestOnTimeoutAsyncAfterFiveTimesAsync()
        {
            var times = 5;
            var generator = new Generator(times);
            var onTimeoutTriggered = false;
            Assert.ThrowsAsync<TimeoutException>(() =>
                _target.TryAsync(() => generator.Next())
                    .WithMaxTryCount(times - 1)
                    .OnTimeout(async t =>
                    {
                        await Task.Delay(100);
                        onTimeoutTriggered = true;
                    })
                    .Until(t => t));
            Assert.That(onTimeoutTriggered, Is.True);
        }

        [Test]
        [Timeout(1000)]
        public void TestOnTimeoutWithTriedCountAsync()
        {
            var times = 5;
            var generator = new Generator(times);
            var onTimeoutTriggered = false;
            var maxTryCount = times - 1;
            Assert.ThrowsAsync<TimeoutException>(() =>
                _target.TryAsync(() => generator.Next())
                    .WithMaxTryCount(maxTryCount)
                    .OnTimeout((t, count) =>
                    {
                        Assert.That(count, Is.EqualTo(maxTryCount));
                        onTimeoutTriggered = true;
                    })
                    .Until(t => t));
            Assert.That(onTimeoutTriggered, Is.True);
        }

        [Test]
        [Timeout(1000)]
        public void TestMultipleOnTimeoutAsync()
        {
            var times = 5;
            var generator = new Generator(times);
            var onTimeoutTriggered1 = false;
            var onTimeoutTriggered2 = false;
            Assert.ThrowsAsync<TimeoutException>(() =>
                _target.TryAsync(() => generator.Next())
                       .WithMaxTryCount(times - 1)
                       .OnTimeout(t => onTimeoutTriggered1 = true)
                       .OnTimeout(t => onTimeoutTriggered2 = true)
                       .Until(t => t));
            Assert.That(onTimeoutTriggered1, Is.True);
            Assert.That(onTimeoutTriggered2, Is.True);
        }
    }
}
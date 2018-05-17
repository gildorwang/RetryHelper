using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class OnSuccessAsyncTest
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
        public async Task TestOnSuccessAfterFiveTimesAsync()
        {
            var times = 5;
            var generator = new Generator(times);
            var onSuccessTriggered = false;
            await _target.TryAsync(() => generator.Next())
                .OnSuccess(t => onSuccessTriggered = true)
                .Until(t => t);
            Assert.That(onSuccessTriggered, Is.True);
        }

        [Test]
        [Timeout(1000)]
        public void TestOnSuccessShouldNotFireAsync()
        {
            var times = 5;
            var generator = new Generator(times);
            var onSuccessTriggered = false;
            Assert.ThrowsAsync<TimeoutException>(() =>
                _target.TryAsync(() => generator.Next())
                    .WithMaxTryCount(times - 1)
                    .OnSuccess(t => onSuccessTriggered = true)
                    .Until(t => t));
            Assert.That(onSuccessTriggered, Is.False);
        }

        [Test]
        [Timeout(1000)]
        public async Task TestOnSuccessWithTriedCountAsync()
        {
            var times = 5;
            var generator = new Generator(times);
            var onSuccessTriggered = false;
            await _target.TryAsync(() => generator.Next())
                .OnSuccess((t, count) =>
                {
                    Assert.That(count, Is.EqualTo(times + 1));
                    onSuccessTriggered = true;
                })
                .Until(t => t);
            Assert.That(onSuccessTriggered, Is.True);
        }

        [Test]
        [Timeout(1000)]
        public async Task TestMultipleOnSuccessAsync()
        {
            var times = 5;
            var generator = new Generator(times);
            var onSuccessTriggered1 = false;
            var onSuccessTriggered2 = false;
            await _target.TryAsync(() => generator.Next())
                   .OnSuccess(t => onSuccessTriggered1 = true)
                   .OnSuccess(t => onSuccessTriggered2 = true)
                   .Until(t => t);
            Assert.That(onSuccessTriggered1, Is.True);
            Assert.That(onSuccessTriggered2, Is.True);
        }
    }
}
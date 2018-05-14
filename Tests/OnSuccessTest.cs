using System;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class OnSuccessTest
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
        public void TestOnSuccessAfterFiveTimes()
        {
            var times = 5;
            var generator = new Generator(times);
            var onSuccessTriggered = false;
            _target.Try(() => generator.Next())
                   .OnSuccess(t => onSuccessTriggered = true)
                   .Until(t => t);
            Assert.That(onSuccessTriggered, Is.True);
        }

        [Test]
        [Timeout(1000)]
        public void TestOnSuccessShouldNotFire()
        {
            var times = 5;
            var generator = new Generator(times);
            var onSuccessTriggered = false;
            Assert.That(() =>
                _target.Try(() => generator.Next())
                       .WithMaxTryCount(times - 1)
                       .OnSuccess(t => onSuccessTriggered = true)
                       .Until(t => t),
                Throws.TypeOf<TimeoutException>());
            Assert.That(onSuccessTriggered, Is.False);
        }

        [Test]
        [Timeout(1000)]
        public void TestOnSuccessWithTriedCount()
        {
            var times = 5;
            var generator = new Generator(times);
            var onSuccessTriggered = false;
            _target.Try(() => generator.Next())
                   .OnSuccess((t, count) =>
                   {
                       Assert.That(count, Is.EqualTo(times + 1));
                       onSuccessTriggered = true;
                   })
                   .Until(t => t);
            Assert.That(onSuccessTriggered, Is.True);
        }
    }
}
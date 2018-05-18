using System;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class TryWithMaxCountTest
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
        public void TestTryUntilWithMaxTryCount()
        {
            var times = 5;
            var generator = new Generator(times);
            bool result = false;
            Assert.That(RetryHelperTest.MeasureTime(() =>
                result = _target.Try(() => generator.Next()).WithMaxTryCount(times + 1).Until(t => t)),
                Is.EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.Tolerance));
            Assert.That(result, Is.True);
        }

        [Test]
        [Timeout(1000)]
        public void TestTryUntilWithMaxTryCountExceeded()
        {
            var times = 5;
            var generator = new Generator(times);
            bool result = false;
            Assert.That(RetryHelperTest.MeasureTime(() =>
                Assert.That(() =>
                    result = _target.Try(() => generator.Next()).WithMaxTryCount(times).Until(t => t),
                    Throws.TypeOf<TimeoutException>())),
                Is.EqualTo(RetryHelperTest.Interval * (times - 1)).Within(RetryHelperTest.Tolerance));
            Assert.That(result, Is.False);
        }
    }
}
using System;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class TryWithTimeLimitTest
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
        public void TestTryUntilWithTimeLimit()
        {
            var times = 5;
            var generator = new Generator(times);
            bool result = false;
            Assert.That(RetryHelperTest.CountTime(() =>
                result = _target.Try(() => generator.Next()).WithTimeLimit(RetryHelperTest.Interval * times + RetryHelperTest.Tolerance).Until(t => t)),
                Is.EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.Tolerance));
            Assert.That(result, Is.True);
        }

        [Test]
        [Timeout(1000)]
        public void TestTryUntilWithTimeLimitExceeded()
        {
            var times = 5;
            var generator = new Generator(times + 1);
            bool result = false;
            Assert.That(() =>
                Assert.That(RetryHelperTest.CountTime(() =>
                    result = _target.Try(() => generator.Next()).WithTimeLimit(RetryHelperTest.Interval * times).Until(t => t)),
                    Is.EqualTo(RetryHelperTest.Interval * times + RetryHelperTest.Tolerance).Within(RetryHelperTest.Tolerance)),
                Throws.TypeOf<TimeoutException>());
            Assert.That(result, Is.False);
        }
    }
}
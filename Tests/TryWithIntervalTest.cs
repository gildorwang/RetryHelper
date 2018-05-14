using System;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class TryWithIntervalTest
    {
        private RetryHelper _target;

        [SetUp]
        public void SetUp()
        {
            _target = new RetryHelper();
            _target.DefaultTryInterval = TimeSpan.FromMilliseconds(RetryHelperTest.Interval);
        }

        [Test]
        [Timeout(2000)]
        public void TestTryUntilWithTryInterval()
        {
            var times = 5;
            var generator = new Generator(times);
            bool result = false;
            Assert.That(RetryHelperTest.CountTime(() =>
                result = _target.Try(() => generator.Next()).WithTryInterval(RetryHelperTest.Interval * 2).Until(t => t)),
                Is.EqualTo(RetryHelperTest.Interval * 2 * times).Within(RetryHelperTest.Tolerance));
            Assert.That(result, Is.True);
        }
    }
}
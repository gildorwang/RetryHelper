using System;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class TryWithTimeLimitTest : AssertionHelper
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
            Expect(RetryHelperTest.CountTime(() =>
                result = _target.Try(() => generator.Next()).WithTimeLimit(RetryHelperTest.Interval * times + RetryHelperTest.Tolerance).Until(t => t)),
                EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.Tolerance));
            Expect(result, True);
        }

        [Test]
        [Timeout(1000)]
        public void TestTryUntilWithTimeLimitExceeded()
        {
            var times = 5;
            var generator = new Generator(times + 1);
            bool result = false;
            Expect(() =>
                Expect(RetryHelperTest.CountTime(() =>
                    result = _target.Try(() => generator.Next()).WithTimeLimit(RetryHelperTest.Interval * times).Until(t => t)),
                    EqualTo(RetryHelperTest.Interval * times + RetryHelperTest.Tolerance).Within(RetryHelperTest.Tolerance)),
                Throws.TypeOf<TimeoutException>());
            Expect(result, False);
        }
    }
}
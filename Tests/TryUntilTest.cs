using System;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class TryUntilTest : AssertionHelper
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
        public void TestTryUntilAfterFiveTimes()
        {
            var times = 5;
            var generator = new Generator(times);
            bool result = false;
            Expect(RetryHelperTest.CountTime(() =>
                result = _target.Try(() => generator.Next()).Until(t => t)),
                EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.Tolerance));
            Expect(generator.TriedTimes, EqualTo(times + 1));
            Expect(result, True);
        }

        [Test]
        [Timeout(100)]
        public void TestTryUntilSuccessFirstTime()
        {
            var times = 0;
            var generator = new Generator(times);
            bool result = false;
            Expect(RetryHelperTest.CountTime(() =>
                result = _target.Try(() => generator.Next()).Until(t => t)),
                EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.Tolerance));
            Expect(generator.TriedTimes, EqualTo(times + 1));
            Expect(result, True);
        }
    }
}
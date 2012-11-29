using System;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class TryUntilNoExceptionTest : AssertionHelper
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
        public void TestTryUntilNoExceptionAfterFiveTimes()
        {
            var times = 10;
            var generator = new Generator(times, true);
            generator.RandomExceptionType = true;
            bool result = false;
            Expect(RetryHelperTest.CountTime(() =>
                result = _target.Try(() => generator.Next()).UntilNoException()),
                EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.Tolerance));
            Expect(generator.TriedTimes, EqualTo(times + 1));
            Expect(result, True);
        }

        [Test]
        [Timeout(100)]
        public void TestTryUntilNoExceptionSuccessFirstTime()
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

        [Test]
        [Timeout(2000)]
        public void TestTryUntilNoExceptionOfTypeAfterFiveTimes()
        {
            var times = 10;
            var generator = new Generator(times, true);
            bool result = false;
            Expect(RetryHelperTest.CountTime(() =>
                result = _target.Try(() => generator.Next()).UntilNoException<ApplicationException>()),
                EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.Tolerance));
            Expect(generator.TriedTimes, EqualTo(times + 1));
            Expect(result, True);
        }

        [Test]
        [Timeout(2000)]
        public void TestTryUntilNoExceptionOfTypeHavingOtherException()
        {
            var times = 10;
            var generator = new Generator(times, true);
            generator.RandomExceptionType = true;
            bool result = false;
            Expect(() =>
                result = _target.Try(() => generator.Next()).UntilNoException<ApplicationException>(),
                Throws.TypeOf<InvalidOperationException>());
            Expect(result, False);
        }
    }
}
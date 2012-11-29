using System;
using System.Diagnostics;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public sealed class RetryHelperTest : AssertionHelper
    {
        public const int Tolerance = 70;
        public const int Interval = 100;

        public static long CountTime(Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            return stopwatch.ElapsedMilliseconds;
        }

        [Test]
        public void TestConstructor()
        {
            var target = new RetryHelper();
            Expect(target.DefaultMaxTryCount, EqualTo(RetryHelper.Instance.DefaultMaxTryCount));
            Expect(target.DefaultMaxTryTime, EqualTo(RetryHelper.Instance.DefaultMaxTryTime));
            Expect(target.DefaultTryInterval, EqualTo(RetryHelper.Instance.DefaultTryInterval));
        }

        [Test]
        public void TestDefaultInstance()
        {
            Expect(RetryHelper.Instance.DefaultMaxTryCount, EqualTo(int.MaxValue));
            Expect(RetryHelper.Instance.DefaultMaxTryTime, EqualTo(TimeSpan.MaxValue));
            Expect(RetryHelper.Instance.DefaultTryInterval, EqualTo(TimeSpan.FromMilliseconds(500)));
        }
    }
}
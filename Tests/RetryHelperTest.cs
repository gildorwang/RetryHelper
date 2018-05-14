using System;
using System.Diagnostics;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public sealed class RetryHelperTest
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
            Assert.That(target.DefaultMaxTryCount, Is.EqualTo(RetryHelper.Instance.DefaultMaxTryCount));
            Assert.That(target.DefaultMaxTryTime, Is.EqualTo(RetryHelper.Instance.DefaultMaxTryTime));
            Assert.That(target.DefaultTryInterval, Is.EqualTo(RetryHelper.Instance.DefaultTryInterval));
        }

        [Test]
        public void TestDefaultInstance()
        {
            Assert.That(RetryHelper.Instance.DefaultMaxTryCount, Is.EqualTo(int.MaxValue));
            Assert.That(RetryHelper.Instance.DefaultMaxTryTime, Is.EqualTo(TimeSpan.MaxValue));
            Assert.That(RetryHelper.Instance.DefaultTryInterval, Is.EqualTo(TimeSpan.FromMilliseconds(500)));
        }
    }
}
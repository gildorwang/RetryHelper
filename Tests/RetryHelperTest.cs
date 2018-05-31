using System;
using System.Diagnostics;
using NUnit.Framework;
using Retry;
using System.Threading.Tasks;

namespace Tests
{
    [TestFixture]
    public sealed class RetryHelperTest
    {
        public const int Tolerance = 70;

        /// <summary>
        /// Async version is less precise.
        /// </summary>
        public const int AsyncTolerance = 200;

        public const int Interval = 100;

        public static long MeasureTime(Action action)
        {
            return MeasureTime(() => Task.Run(action)).Result;
        }

        public static async Task<long> MeasureTime(Func<Task> asyncAction)
        {
            var stopwatch = Stopwatch.StartNew();
            await asyncAction();
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
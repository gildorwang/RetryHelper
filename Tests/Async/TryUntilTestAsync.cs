using System;
using NUnit.Framework;
using Retry;
using System.Threading.Tasks;

namespace Tests
{
    [TestFixture]
    public class TryUntilTestAsync
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
        public async Task TestTryUntilAfterFiveTimesAsync()
        {
            var times = 5;
            var generator = new Generator(times);
            bool result = false;
            Assert.That(
                await RetryHelperTest.MeasureTime(async () =>
                    result = await _target
                        .TryAsync(async () => await generator.NextAsync())
                        .Until(t => t)),
                Is.EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.Tolerance));
            Assert.That(generator.TriedTimes, Is.EqualTo(times + 1));
            Assert.That(result, Is.True);
        }

        [Test]
        [Timeout(100)]
        public async Task TestTryUntilSuccessFirstTimeAsync()
        {
            var times = 0;
            var generator = new Generator(times);
            bool result = false;
            Assert.That(
                await RetryHelperTest.MeasureTime(async () =>
                    result = await _target
                        .TryAsync(async () => await generator.NextAsync())
                        .Until(t => t)),
                Is.EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.Tolerance));
            Assert.That(generator.TriedTimes, Is.EqualTo(times + 1));
            Assert.That(result, Is.True);
        }
    }
}

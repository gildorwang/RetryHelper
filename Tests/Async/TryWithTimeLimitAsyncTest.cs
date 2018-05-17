using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class TryWithTimeLimitAsyncTest
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
        public async Task TestTryUntilWithTimeLimitAsync()
        {
            var times = 5;
            var generator = new Generator(times);
            bool result = false;
            Assert.That(await RetryHelperTest.MeasureTime(async () =>
                result = await _target.Try(async () => await generator.NextAsync()).WithTimeLimit(RetryHelperTest.Interval * times + RetryHelperTest.Tolerance).Until(t => t)),
                Is.EqualTo(RetryHelperTest.Interval * times).Within(RetryHelperTest.Tolerance));
            Assert.That(result, Is.True);
        }

        [Test]
        [Timeout(1000)]
        public void TestTryUntilWithTimeLimitExceededAsync()
        {
            var times = 5;
            var generator = new Generator(times + 1);
            Assert.ThrowsAsync<TimeoutException>(async () =>
                await _target
                    .Try(async () => await generator.NextAsync())
                    .WithTimeLimit(RetryHelperTest.Interval * times)
                    .Until(t => t));
        }
    }
}
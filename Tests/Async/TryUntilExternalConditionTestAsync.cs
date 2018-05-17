using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class TryUntilExternalConditionTestAsync
    {
        private RetryHelper _target;

        [SetUp]
        public void SetUp()
        {
            _target = new RetryHelper();
            _target.DefaultTryInterval = TimeSpan.FromMilliseconds(100);
        }

        [Test]
        [Timeout(1500)]
        public async Task TestTryUntilExpectedTimeAsync()
        {
            var expectedStopTime = DateTime.Now.AddSeconds(1);
            await _target
                .Try(async () => await Task.Delay(0))
                .Until(() => DateTime.Now >= expectedStopTime);
            Assert.That(DateTime.Now, Is.EqualTo(expectedStopTime).Within(TimeSpan.FromMilliseconds(200)));
        }

        [Test]
        [Timeout(1500)]
        public async Task TestTryAsyncUntilExpectedTimeAsync()
        {
            var expectedStopTime = DateTime.Now.AddSeconds(1);
            await _target
                .TryAsync(() => { })
                .Until(() => DateTime.Now >= expectedStopTime);
            Assert.That(DateTime.Now, Is.EqualTo(expectedStopTime).Within(TimeSpan.FromMilliseconds(200)));
        }
    }
}
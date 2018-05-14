using System;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class TryUntilExternalConditionTest
    {
        private RetryHelper _target;

        [SetUp]
        public void SetUp()
        {
            _target = new RetryHelper();
            _target.DefaultTryInterval = TimeSpan.FromMilliseconds(500);
        }

        [Test]
        [Timeout(3500)]
        public void TestTryUntilExpectedTime()
        {
            var expectedStopTime = DateTime.Now.AddSeconds(3);
            _target.Try(() => DateTime.Now).Until(() => DateTime.Now >= expectedStopTime);
            Assert.That(DateTime.Now, Is.EqualTo(expectedStopTime).Within(TimeSpan.FromMilliseconds(300)));
        }
    }
}
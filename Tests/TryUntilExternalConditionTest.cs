using System;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class TryUntilExternalConditionTest : AssertionHelper
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
            Expect(DateTime.Now, EqualTo(expectedStopTime).Within(TimeSpan.FromMilliseconds(300)));
        }
    }
}
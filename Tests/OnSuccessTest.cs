using System;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public class OnSuccessTest : AssertionHelper
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
        public void TestOnSuccessAfterFiveTimes()
        {
            var times = 5;
            var generator = new Generator(times);
            var onSuccessTriggered = false;
            _target.Try(() => generator.Next())
                   .OnSuccess(t => onSuccessTriggered = true)
                   .Until(t => t);
            Expect(onSuccessTriggered, True);
        }

        [Test]
        [Timeout(1000)]
        public void TestOnSuccessShouldNotFire()
        {
            var times = 5;
            var generator = new Generator(times);
            var onSuccessTriggered = false;
            Expect(() => 
                _target.Try(() => generator.Next())
                       .WithMaxTryCount(times - 1)
                       .OnSuccess(t => onSuccessTriggered = true)
                       .Until(t => t), 
                Throws.TypeOf<TimeoutException>());
            Expect(onSuccessTriggered, False);
        }
    }
}
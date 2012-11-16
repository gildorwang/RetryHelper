using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public sealed class RetryHelperTest : AssertionHelper
    {
        private const int Interval = 100;
        private const int Tolerance = 70;

        [Test]
        public void TestDefaultInstance()
        {
            Expect(RetryHelper.Instance.DefaultMaxTryCount, EqualTo(int.MaxValue));
            Expect(RetryHelper.Instance.DefaultMaxTryTime, EqualTo(TimeSpan.MaxValue));
            Expect(RetryHelper.Instance.DefaultTryInterval, EqualTo(TimeSpan.FromMilliseconds(500)));
        }

        [Test]
        public void TestConstructor()
        {
            var target = new RetryHelper();
            Expect(target.DefaultMaxTryCount, EqualTo(RetryHelper.Instance.DefaultMaxTryCount));
            Expect(target.DefaultMaxTryTime, EqualTo(RetryHelper.Instance.DefaultMaxTryTime));
            Expect(target.DefaultTryInterval, EqualTo(RetryHelper.Instance.DefaultTryInterval));
        }

        public static long CountTime(Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            return stopwatch.ElapsedMilliseconds;
        }

        [TestFixture]
        public class TryUntilTest : AssertionHelper
        {
            private RetryHelper _target;

            [SetUp]
            public void SetUp()
            {
                _target = new RetryHelper();
                _target.DefaultTryInterval = TimeSpan.FromMilliseconds(Interval);
            }

            [Test]
            [Timeout(1000)]
            public void TestTryUntilAfterFiveTimes()
            {
                var times = 5;
                var generator = new Generator(times);
                bool result = false;
                Expect(CountTime(() =>
                    result = _target.Try(() => generator.Next()).Until(t => t)),
                    EqualTo(Interval * times).Within(Tolerance));
                Expect(generator.TriedTimes, EqualTo(times + 1));
                Expect(result, True);
            }

            [Test]
            [Timeout(100)]
            public void TestTryUntilSuccessFirstTime()
            {
                var times = 0;
                var generator = new Generator(times);
                bool result = false;
                Expect(CountTime(() =>
                    result = _target.Try(() => generator.Next()).Until(t => t)),
                    EqualTo(Interval * times).Within(Tolerance));
                Expect(generator.TriedTimes, EqualTo(times + 1));
                Expect(result, True);
            }
        }

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

        [TestFixture]
        public class TryUntilNoExceptionTest : AssertionHelper
        {
            private RetryHelper _target;

            [SetUp]
            public void SetUp()
            {
                _target = new RetryHelper();
                _target.DefaultTryInterval = TimeSpan.FromMilliseconds(Interval);
            }

            [Test]
            [Timeout(2000)]
            public void TestTryUntilNoExceptionAfterFiveTimes()
            {
                var times = 10;
                var generator = new Generator(times, true);
                generator.RandomExceptionType = true;
                bool result = false;
                Expect(CountTime(() =>
                    result = _target.Try(() => generator.Next()).UntilNoException()),
                    EqualTo(Interval * times).Within(Tolerance));
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
                Expect(CountTime(() =>
                    result = _target.Try(() => generator.Next()).Until(t => t)),
                    EqualTo(Interval * times).Within(Tolerance));
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
                Expect(CountTime(() =>
                    result = _target.Try(() => generator.Next()).UntilNoException<ApplicationException>()),
                    EqualTo(Interval * times).Within(Tolerance));
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

        [TestFixture]
        public class TryWithTimeLimitTest : AssertionHelper
        {
            private RetryHelper _target;

            [SetUp]
            public void SetUp()
            {
                _target = new RetryHelper();
                _target.DefaultTryInterval = TimeSpan.FromMilliseconds(Interval);
            }

            [Test]
            [Timeout(1000)]
            public void TestTryUntilWithTimeLimit()
            {
                var times = 5;
                var generator = new Generator(times);
                bool result = false;
                Expect(CountTime(() =>
                    result = _target.Try(() => generator.Next()).WithTimeLimit(Interval * times + Tolerance).Until(t => t)),
                    EqualTo(Interval * times).Within(Tolerance));
                Expect(result, True);
            }

            [Test]
            [Timeout(1000)]
            public void TestTryUntilWithTimeLimitExceeded()
            {
                var times = 5;
                var generator = new Generator(times + 1);
                bool result = false;
                Expect(() =>
                    Expect(CountTime(() =>
                        result = _target.Try(() => generator.Next()).WithTimeLimit(Interval * times).Until(t => t)),
                        EqualTo(Interval * times + Tolerance).Within(Tolerance)),
                    Throws.TypeOf<TimeoutException>());
                Expect(result, False);
            }
        }

        [TestFixture]
        public class TryWithIntervalTest : AssertionHelper
        {
            private RetryHelper _target;

            [SetUp]
            public void SetUp()
            {
                _target = new RetryHelper();
                _target.DefaultTryInterval = TimeSpan.FromMilliseconds(Interval);
            }

            [Test]
            [Timeout(2000)]
            public void TestTryUntilWithTryInterval()
            {
                var times = 5;
                var generator = new Generator(times);
                bool result = false;
                Expect(CountTime(() =>
                    result = _target.Try(() => generator.Next()).WithTryInterval(Interval * 2).Until(t => t)),
                    EqualTo(Interval * 2 * times).Within(Tolerance));
                Expect(result, True);
            }
        }

        [TestFixture]
        public class TryWithMaxCountTest : AssertionHelper
        {
            private RetryHelper _target;

            [SetUp]
            public void SetUp()
            {
                _target = new RetryHelper();
                _target.DefaultTryInterval = TimeSpan.FromMilliseconds(Interval);
            }

            [Test]
            [Timeout(1000)]
            public void TestTryUntilWithMaxTryCount()
            {
                var times = 5;
                var generator = new Generator(times);
                bool result = false;
                Expect(CountTime(() =>
                    result = _target.Try(() => generator.Next()).WithMaxTryCount(times + 1).Until(t => t)),
                    EqualTo(Interval * times).Within(Tolerance));
                Expect(result, True);
            }

            [Test]
            [Timeout(1000)]
            public void TestTryUntilWithMaxTryCountExceeded()
            {
                var times = 5;
                var generator = new Generator(times);
                bool result = false;
                Expect(CountTime(() =>
                    Expect(() =>
                        result = _target.Try(() => generator.Next()).WithMaxTryCount(times).Until(t => t),
                        Throws.TypeOf<TimeoutException>())),
                    EqualTo(Interval * (times - 1)).Within(Tolerance));
                Expect(result, False);
            }
        }

        [TestFixture]
        public class OnSuccessTest : AssertionHelper
        {
            private RetryHelper _target;

            [SetUp]
            public void SetUp()
            {
                _target = new RetryHelper();
                _target.DefaultTryInterval = TimeSpan.FromMilliseconds(Interval);
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

        [TestFixture]
        public class OnTimeoutTest : AssertionHelper
        {
            private RetryHelper _target;

            [SetUp]
            public void SetUp()
            {
                _target = new RetryHelper();
                _target.DefaultTryInterval = TimeSpan.FromMilliseconds(Interval);
            }

            [Test]
            [Timeout(1000)]
            public void TestOnTimeoutShouldNotFire()
            {
                var times = 5;
                var generator = new Generator(times);
                var onTimeoutTriggered = false;
                _target.Try(() => generator.Next())
                       .OnTimeout(t => onTimeoutTriggered = true)
                       .Until(t => t);
                Expect(onTimeoutTriggered, False);
            }

            [Test]
            [Timeout(1000)]
            public void TestOnTimeoutAfterFiveTimes()
            {
                var times = 5;
                var generator = new Generator(times);
                var onTimeoutTriggered = false;
                Expect(() =>
                    _target.Try(() => generator.Next())
                           .WithMaxTryCount(times - 1)
                           .OnTimeout(t => onTimeoutTriggered = true)
                           .Until(t => t),
                    Throws.TypeOf<TimeoutException>());
                Expect(onTimeoutTriggered, True);
            }
        }


    }
}

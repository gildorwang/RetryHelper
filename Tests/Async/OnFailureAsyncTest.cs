using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Retry;

namespace Tests
{
    [TestFixture]
    public sealed class OnFailureTestAsync
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
        public async Task TestOnFailureWithNoParameter()
        {
            var times = 5;
            var generator = new Generator(times);
            var onFailureTriggered = 0;
            await _target.TryAsync(() => generator.Next())
                .OnFailure(() => onFailureTriggered++)
                .Until(t => t);
            Assert.That(onFailureTriggered, Is.EqualTo(times));
        }

        [Test]
        [Timeout(1000)]
        public async Task TestOnFailureAfterFiveTimesAsync()
        {
            var times = 5;
            var generator = new Generator(times);
            var onFailureTriggered = 0;
            await _target.TryAsync(() => generator.Next())
                .OnFailure(t =>
                {
                    Assert.That(t, Is.False);
                    onFailureTriggered++;
                })
                .Until(t => t);
            Assert.That(onFailureTriggered, Is.EqualTo(times));
        }

        [Test]
        [Timeout(2000)]
        public async Task TestOnFailureAsyncAfterFiveTimesAsync()
        {
            var times = 5;
            var generator = new Generator(times);
            var onFailureTriggered = 0;
            await _target.TryAsync(() => generator.Next())
                .OnFailure(async (t, count) =>
                {
                    Assert.That(t, Is.False);
                    onFailureTriggered++;
                    // Perform some delay. If the task is not awaited, onFailureTriggered would be
                    // increased by subsequent tries and the assertion below would fail
                    await Task.Delay(RetryHelperTest.Interval * 2);
                    Assert.That(onFailureTriggered, Is.EqualTo(count));
                })
                .Until(t => t);
            Assert.That(onFailureTriggered, Is.EqualTo(times));
        }

        [Test]
        [Timeout(1000)]
        public async Task TestOnFailureAsyncWithNoParameterAsync()
        {
            var times = 5;
            var generator = new Generator(times);
            var onFailureTriggered = 0;
            await _target.TryAsync(() => generator.Next())
                .OnFailure(async () => await Task.Run(() => onFailureTriggered++))
                .Until(t => t);
            Assert.That(onFailureTriggered, Is.EqualTo(times));
        }

        [Test]
        [Timeout(1000)]
        public async Task TestOnFailureShouldNotFireIfSucceedAtFirstTimeAsync()
        {
            var times = 0;
            var generator = new Generator(times);
            var onFailureTriggered = 0;
            await _target.TryAsync(() => generator.Next())
                .OnFailure(() => onFailureTriggered++)
                .Until(t => t);
            Assert.That(onFailureTriggered, Is.EqualTo(0));
        }

        [Test]
        public async Task TestCircularReadStreamAsync()
        {
            const int len = 100;
            var stream = new MemoryStream();
            for (int i = 0; i < len; i++)
            {
                stream.WriteByte((byte)i);
            }
            stream.Seek(0, SeekOrigin.Begin);
            var binaryReader = new BinaryReader(stream);
            for (int i = 0; i < len * 3; i++)
            {
                var b = await RetryHelper.Instance
                    .TryAsync(() => binaryReader.ReadByte())
                    .WithTryInterval(0)
                    .OnFailure(t => stream.Seek(0, SeekOrigin.Begin))
                    .UntilNoException<EndOfStreamException>();
                Console.Write("{0} ", b);
            }
        }

        [Test]
        public async Task TestOnFailureWithTryCountAsync()
        {
            var times = 5;
            var generator = new Generator(times);
            var onFailureTriggered = 0;
            await _target.TryAsync(() => generator.Next())
                .OnFailure((t, count) =>
                {
                    Assert.That(t, Is.False);
                    Assert.That(count, Is.EqualTo(++onFailureTriggered));
                })
                .Until(t => t);
            Assert.That(onFailureTriggered, Is.EqualTo(times));
        }

        [Test]
        [Timeout(1000)]
        public async Task TestMultipleOnFailureAsync()
        {
            var times = 5;
            var generator = new Generator(times);
            var onFailureTriggered1 = 0;
            var onFailureTriggered2 = 0;
            await _target.TryAsync(() => generator.Next())
                .OnFailure(t =>
                {
                    Assert.That(t, Is.False);
                    onFailureTriggered1++;
                })
                .OnFailure(t =>
                {
                    Assert.That(t, Is.False);
                    onFailureTriggered2++;
                })
                .Until(t => t);
            Assert.That(onFailureTriggered1, Is.EqualTo(times));
            Assert.That(onFailureTriggered2, Is.EqualTo(times));
        }
    }
}

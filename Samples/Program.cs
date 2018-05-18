using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Retry;

namespace Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            // Example 1: I want to retry until I get a return value < 0.1 from the method TryGetSomething.
            Example1();

            // Example 2: I want to retry method TryDoSomething until there's no exception thrown.
            Example2();

            // Example 3: I want to retry until I get a return value < 0.1 from an asynchronous method TryGetValueAsync.
            Example3().Wait();
        }

        private static void Example1()
        {
            // Basic usage
            RetryHelper.Instance.Try(() => TryGetValue()).Until(result => result < 0.1);

            // Get the result from the retried method
            var resultSmallEnough = RetryHelper.Instance.Try(() => TryGetValue()).Until(result => result < 0.1);

            // Specify interval
            RetryHelper.Instance.Try(() => TryGetValue()).WithTryInterval(100).Until(result => result < 0.1);

            // Specify max try count, will throw TimeoutException if exceeded
            RetryHelper.Instance.Try(() => TryGetValue()).WithMaxTryCount(20).Until(result => result < 0.1);

            // Can also constrain the total try time
            RetryHelper.Instance.Try(() => TryGetValue()).WithTimeLimit(TimeSpan.FromSeconds(10)).Until(result => result < 0.1);

            // Specify the extra success/fail/timeout action
            RetryHelper.Instance.Try(() => TryGetValue())
                .WithMaxTryCount(20)
                .OnSuccess(result => Trace.TraceInformation($"Got result {result}."))
                .OnFailure(result => Trace.TraceWarning($"Try failed. Got {result}."))
                .OnTimeout(lastResult => Trace.TraceError("Did not get result under 0.1 in 20 times."))
                .Until(result => result < 0.1);
        }

        private static void Example2()
        {
            // Retry on any (non-fatal) exception
            RetryHelper.Instance.Try(() => TryDoSomething()).UntilNoException();

            // Retry on specific exception
            RetryHelper.Instance.Try(() => TryDoSomething()).UntilNoException<ApplicationException>();

            // Change the global default settings
            RetryHelper.Instance.DefaultMaxTryCount = 3;
            RetryHelper.Instance.DefaultMaxTryTime = TimeSpan.FromSeconds(10);
            RetryHelper.Instance.DefaultTryInterval = TimeSpan.FromMilliseconds(100);


            // Get another RetryHelper instance with custom TraceSource and seperate configration
            var retryHelper = new RetryHelper(new TraceSource("MyTraceSource"))
            {
                DefaultMaxTryCount = 10,
                DefaultMaxTryTime = TimeSpan.FromSeconds(30),
                DefaultTryInterval = TimeSpan.FromMilliseconds(500)
            };
        }

        private static async Task Example3()
        {
            // Basic usage
            await RetryHelper.Instance.Try(async () => await TryGetValueAsync()).Until(result => result < 0.1);

            // async/await keywords can be omitted for simplicity in this case
            await RetryHelper.Instance.Try(() => TryGetValueAsync()).Until(result => result < 0.1);

            // Asynchronous until condition is also supported
            await RetryHelper.Instance.Try(async () => await TryGetValueAsync()).Until(async result => result + await TryGetValueAsync() < 0.2);

            // In case the operation is not asynchronous, but you want to use an asynchronous until condition, use TryAsync
            await RetryHelper.Instance.TryAsync(() => TryGetValue()).Until(async result => result + await TryGetValueAsync() < 0.2);

            // Asynchronous OnSuccess/OnFailure/OnTimeout
            // Note that asyhronous operation taken in OnFailure counts against TimeLimit,
            // i.e. when retrying with time limit, the more time taken in OnFailure, the less
            // retries can be performed.
            await RetryHelper.Instance.Try(async () => await TryGetValueAsync())
                .WithTryInterval(100)
                .WithMaxTryCount(20)
                .OnSuccess(async result => await LogToServerAsync($"Got result {result}."))
                .OnFailure(async result => await LogToServerAsync($"Try failed. Got {result}."))
                .OnTimeout(async lastResult => await LogToServerAsync("Did not get result under 0.1 in 20 times."))
                .Until(result => result < 0.1);
        }

        static readonly Random _random = new Random();

        static double TryGetValue()
        {
            // This method returns random double in [0, 1)
            var randomValue = _random.NextDouble();
            Console.WriteLine($"[{nameof(TryGetValue)}]generated: {randomValue}.");
            return randomValue;
        }

        static async Task<double> TryGetValueAsync()
        {
            // This method simulates an asynchronous operation to fetch value.
            // It delays 100 ms, and then returns random double in [0, 1)
            await Task.Delay(100);
            var randomValue = _random.NextDouble();
            Console.WriteLine($"[{nameof(TryGetValueAsync)}]generated: {randomValue}.");
            return randomValue;
        }

        static void TryDoSomething()
        {
            // This method throws exception if generated number >= 0.1
            // It represents some operation that may fail, like deleting a file
            var randomValue = _random.NextDouble();
            Console.WriteLine($"[{nameof(TryDoSomething)}]generated: {randomValue}.");
            if (randomValue >= 0.1)
            {
                throw new ApplicationException();
            }
        }

        static async Task LogToServerAsync(string message)
        {
            // Pretend that we are logging to server
            await Task.Delay(100);
            Trace.TraceInformation(message);
        }
    }
}

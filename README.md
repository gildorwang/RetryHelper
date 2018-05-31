RetryHelper
=======

This is a generic helper to help try some action until the given condition is met. It now works seamlessly with C# `async`/`await` keywords for asynchronous operations, which is very common in a scenario that requires retry logic.

Usage
----
Write retry logic for operations like web request or file operation in a more readable way rather than a try-catch nested in a loop. You can specify end conditions based on return value or exception, config the retry interval, maximum retry count and maximum retry time limitation.

Examples:
-------

````csharp
using Retry;
````

### One-line example
````csharp
RetryHelper.Instance.Try(() => TryDoSomething()).UntilNoException();
````

### One-line async example
````csharp
await RetryHelper.Instance.Try(async () => await TryGetValueAsync()).Until(async result => result < await GetQuota());
````

### Example 1: Retry until I get a return value < 0.1 from the method *TryGetValue*.
````csharp
// Basic usage - keep trying every 500ms forever
RetryHelper.Instance.Try(() => TryGetValue()).Until(result => result < 0.1);

// Get the result from the retried method
var resultSmallEnough = RetryHelper.Instance.Try(() => TryGetValue()).Until(result => result < 0.1);

// Specify interval as 100 ms
RetryHelper.Instance.Try(() => TryGetValue()).WithTryInterval(100).Until(result => result < 0.1);

// Try 20 times maximum and throw TimeoutException if exceeded
RetryHelper.Instance.Try(() => TryGetValue()).WithMaxTryCount(20).Until(result => result < 0.1);

// Can also limit the total try time duration
RetryHelper.Instance.Try(() => TryGetValue()).WithTimeLimit(TimeSpan.FromSeconds(10)).Until(result => result < 0.1);

// Specify the extra success/fail/timeout action
RetryHelper.Instance.Try(() => TryGetValue())
    .WithMaxTryCount(20)
    .OnSuccess(result => Trace.TraceInformation($"Got result {result}."))
    .OnFailure(result => Trace.TraceWarning($"Try failed. Got {result}."))
    .OnTimeout(lastResult => Trace.TraceError("Did not get result under 0.1 in 20 times."))
    .Until(result => result < 0.1);
````
#### Callbacks explained
- `OnSuccess`: Executed after the condition is met.
- `OnFailure`: Executed after each failed attempt and before the next attempt.
- `OnTimeout`: Executed after all allowed attempts have failed.

Multiple callbacks of the same type can be registered. In this case, the order of invocation is not guaranteed.
````csharp
RetryHelper.Instance.Try(() => TryGetValue())
    .OnFailure(result => Trace.TraceWarning($"Try failed. Got {result}."))
    .OnFailure(() => Trace.TraceWarning($"As I said or will say, it failed."))
    .Until(result => result < 0.1);
````


### Example 2: Retry method *TryDoSomething* until there's no exception thrown.

````csharp
// Retry on any (non-fatal) exception
RetryHelper.Instance.Try(() => TryDoSomething()).UntilNoException();

// Retry on specific exception
RetryHelper.Instance.Try(() => TryDoSomething()).UntilNoException<ApplicationException>();

// Or pass the Type object as parameter
RetryHelper.Instance.Try(() => TryDoSomething()).UntilNoException(typeof(ApplicationException));
````


### Example 3: Retry until I get a return value < 0.1 from an asynchronous method *TryGetValueAsync*

````csharp
// Basic usage
await RetryHelper.Instance.Try(async () => await TryGetValueAsync()).Until(result => result < 0.1);

// async/await keywords can be omitted for simplicity in this case
await RetryHelper.Instance.Try(() => TryGetValueAsync()).Until(result => result < 0.1);

// Asynchronous until condition is also supported
await RetryHelper.Instance.Try(async () => await TryGetValueAsync()).Until(async result => result + await TryGetValueAsync() < 0.2);

// In case the operation is not asynchronous, but you want to use an asynchronous until condition, use TryAsync
await RetryHelper.Instance.TryAsync(() => TryGetValue()).Until(async result => result + await TryGetValueAsync() < 0.2);

// Asynchronous OnSuccess/OnFailure/OnTimeout
// Note that asynchronous operation taken in OnFailure counts against TimeLimit,
// i.e. when retrying with time limit, the more time taken in OnFailure, the less
// retries can be performed.
await RetryHelper.Instance.Try(async () => await TryGetValueAsync())
    .WithTryInterval(100)
    .WithMaxTryCount(20)
    .OnSuccess(async result => await LogToServerAsync($"Got result {result}."))
    .OnFailure(async result => await LogToServerAsync($"Try failed. Got {result}."))
    .OnTimeout(async lastResult => await LogToServerAsync("Did not get result under 0.1 in 20 times."))
    .Until(result => result < 0.1);
````

*Just like the synchronous version above, multiple asynchronous callbacks of the same type can be registered. In this case, callbacks will be invoked and awaited one by one, although the order of invocation is not guaranteed. Asynchronous callbacks will not be invoked concurrently.*


### Change the global default settings

````csharp
RetryHelper.Instance.DefaultMaxTryCount = 3;
RetryHelper.Instance.DefaultMaxTryTime = TimeSpan.FromSeconds(10);
RetryHelper.Instance.DefaultTryInterval = TimeSpan.FromMilliseconds(100);
````


### Get another RetryHelper instance with custom TraceSource and unique configuration

````csharp
var retryHelper = new RetryHelper(new TraceSource("MyTraceSource"))
{
    DefaultMaxTryCount = 10,
    DefaultMaxTryTime = TimeSpan.FromSeconds(30),
    DefaultTryInterval = TimeSpan.FromMilliseconds(500),
};
````


Change Log
==========
### v2.1.0 (2018/5/30)
* Support passing exception type to `UntilNoException`
* Allow `OnFailure`, `OnSuccess` and `OnTimeout` callbacks to take no parameter
* Fixed a bug that if multiple callbacks of the same type are registered with async retry tasks, only the last one is awaited
* Replaced `Thread.Sleep` with `Task.Delay` for `AsyncRetryTask`
* Made `Extensions.MakeFunc` obsolete which should not have been public

### v2.0.0 (2018/5/17)
* Support asynchronous operations, conditions and callbacks (`async`/`await` keywords)
* Updated target framework to .NET 4.5
* Fixed a bug that `OnFailure` is not respected if not registered last


LICENSE
=======
[Apache 2.0 License](https://github.com/gildorwang/RetryHelper/blob/master/LICENSE)

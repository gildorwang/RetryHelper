RetryHelper
=======

This is a generic helper to help try some action until given condition is met.

Usage
----
Write retry logic for operations like web request or file operation in a more readable way rather than a try-catch nested in a loop. You can specify end conditions based on return value or exception, config the retry interval, maximum retry count and maximum retry time flexibly, and more.

Examples:
-------

### One-line sample

    RetryHelper.Instance.Try(() => TryDoSomething()).UntilNoException();

### Example 1: Retry until I get a return value < 0.1 from the method *TryGetSomething*.

    // Basic usage
    RetryHelper.Instance.Try(() => TryGetSomething()).Until(result => result < 0.1);

    // Get the result from the retried method
    var resultSmallEnough = RetryHelper.Instance.Try(() => TryGetSomething()).Until(result => result < 0.1);

    // Specify interval
    RetryHelper.Instance.Try(() => TryGetSomething()).WithTryInterval(100).Until(result => result < 0.1);

    // Specify max try count, will throw TimeoutException if exceeded
    RetryHelper.Instance.Try(() => TryGetSomething()).WithMaxTryCount(20).Until(result => result < 0.1);

    // Can also constrain the total try time
    RetryHelper.Instance.Try(() => TryGetSomething()).WithTimeLimit(TimeSpan.FromSeconds(10)).Until(result => result < 0.1);

    // Specify the extra success/fail/timeout action
    RetryHelper.Instance.Try(() => TryGetSomething())
        .WithMaxTryCount(20)
        .OnSuccess(result => Trace.TraceInformation(string.Format("Get result {0}.", result)))
        .OnFailure(result => Trace.TraceWarning(string.Format("Try failed. Got {0}", result)))
        .OnTimeout(lastResult => Trace.TraceError("Did not get result under 0.1 in 20 times."))
        .Until(result => result < 0.1);

    // Also get the tried count.
    RetryHelper.Instance.Try(() => TryGetSomething())
        .WithMaxTryCount(20)
        .OnSuccess((result, triedCount) => Trace.TraceInformation(string.Format("Get expected result {0} after {1} times.", result, triedCount)))
        .OnFailure((lastResult, triedCount) => Trace.TraceWarning(string.Format("Try failed after {0} times. Got {1}", triedCount, lastResult)))
        .OnTimeout((lastResult, triedCount) => Trace.TraceError(string.Format("Did not get result under 0.1 in {0} times.", triedCount)))
        .Until(result => result < 0.1);

- OnSuccess: Executed after the condition is met.
- OnFailure: Executed after each failed attempt and before the next attempt.
- OnTimeout: Executed after all allowed attempts are failed.


### Example 2: Retry method *TryDoSomething* until there's no exception thrown.

    // Retry on any (non-fatal) exception
    RetryHelper.Instance.Try(() => TryDoSomething()).UntilNoException();

    // Retry on specific exception
    RetryHelper.Instance.Try(() => TryDoSomething()).UntilNoException<ApplicationException>();

    // Lambda can be simplified in this case
    RetryHelper.Instance.Try(TryDoSomething).UntilNoException();


### Change the global default settings

    RetryHelper.Instance.DefaultMaxTryCount = 3;
    RetryHelper.Instance.DefaultMaxTryTime = TimeSpan.FromSeconds(10);
    RetryHelper.Instance.DefaultTryInterval = TimeSpan.FromMilliseconds(100);


### Get another RetryHelper instance with custom TraceSource and separate configration

    var retryHelper = new RetryHelper(new TraceSource("MyTraceSource"))
    {
        DefaultMaxTryCount = 10,
        DefaultMaxTryTime = TimeSpan.FromSeconds(30),
        DefaultTryInterval = TimeSpan.FromMilliseconds(500)
    }

LICENSE
=======
[Apache 2.0 License](https://github.com/gildorwang/RetryHelper/blob/master/LICENSE.md)

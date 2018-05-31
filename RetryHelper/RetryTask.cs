using System;
using System.Diagnostics;

namespace Retry
{
    /// <summary>
    /// Represents the task to be retried.
    /// </summary>
    public class RetryTask
    {
        public static readonly TimeSpan DefaultTryInterval = TimeSpan.FromMilliseconds(500);

        public static readonly TimeSpan DefaultMaxTryTime = TimeSpan.MaxValue;

        public static readonly int DefaultMaxTryCount = int.MaxValue;

        protected RetryTask<int> Task;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryTask"/> class.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="traceSource">The trace source.</param>
        public RetryTask(Action task, TraceSource traceSource)
        {
            Task = new RetryTask<int>(task.MakeFunc<int>(), traceSource);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryTask"/> class.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="traceSource">The trace source.</param>
        /// <param name="maxTryTime">The max try time.</param>
        /// <param name="maxTryCount">The max try count.</param>
        /// <param name="tryInterval">The try interval.</param>
        public RetryTask(Action task, TraceSource traceSource,
            TimeSpan maxTryTime, int maxTryCount, TimeSpan tryInterval)
        {
            Task = new RetryTask<int>(task.MakeFunc<int>(), traceSource,
                maxTryTime, maxTryCount, tryInterval);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryTask"/> class.
        /// </summary>
        protected RetryTask()
        {
        }

        /// <summary>
        ///   Retries the task until the specified end condition is satisfied, 
        ///   or the max try time/count is exceeded, or an exception is thrown druing task execution.
        /// </summary>
        /// <param name = "endCondition">The end condition.</param>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public void Until(Func<bool> endCondition)
        {
            Task.Until(endCondition);
        }

        /// <summary>
        ///   Retries the task until no exception is thrown during the task execution.
        /// </summary>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public void UntilNoException()
        {
            Task.UntilNoException();
        }

        /// <summary>
        ///   Retries the task until the specified exception or any derived exception is not thrown during the task execution.
        ///   Any other exception thrown is re-thrown.
        /// </summary>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public void UntilNoException<TException>()
        {
            Task.UntilNoException<TException>();
        }

        /// <summary>
        ///   Retries the task until the specified exception or any derived exception is not thrown during the task execution.
        ///   Any other exception thrown is re-thrown.
        /// </summary>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public void UntilNoException(Type exceptionType)
        {
            Task.UntilNoException(exceptionType);
        }

        /// <summary>
        ///   Configures the max try time limit in milliseconds.
        /// </summary>
        /// <param name = "milliseconds">The max try time limit in milliseconds.</param>
        /// <returns></returns>
        public RetryTask WithTimeLimit(int milliseconds)
        {
            return WithTimeLimit(TimeSpan.FromMilliseconds(milliseconds));
        }

        /// <summary>
        ///   Configures the max try time limit.
        /// </summary>
        /// <param name = "maxTryTime">The max try time limit.</param>
        /// <returns></returns>
        public RetryTask WithTimeLimit(TimeSpan maxTryTime)
        {
            return new RetryTask { Task = Task.WithTimeLimit(maxTryTime) };
        }

        /// <summary>
        ///   Configures the try interval time in milliseconds.
        /// </summary>
        /// <param name = "milliseconds">The try interval time in milliseconds.</param>
        /// <returns></returns>
        public RetryTask WithTryInterval(int milliseconds)
        {
            return WithTryInterval(TimeSpan.FromMilliseconds(milliseconds));
        }

        /// <summary>
        ///   Configures the try interval time.
        /// </summary>
        /// <param name = "tryInterval">The try interval time.</param>
        /// <returns></returns>
        public RetryTask WithTryInterval(TimeSpan tryInterval)
        {
            return new RetryTask { Task = Task.WithTryInterval(tryInterval) };
        }

        /// <summary>
        ///   Configures the max try count limit.
        /// </summary>
        /// <param name = "maxTryCount">The max try count.</param>
        /// <returns></returns>
        public RetryTask WithMaxTryCount(int maxTryCount)
        {
            return new RetryTask { Task = Task.WithMaxTryCount(maxTryCount) };
        }

        /// <summary>
        /// Configures the action to take when the try action timed out before success. 
        /// </summary>
        /// <param name="timeoutAction">The action to take on timeout.</param>
        /// <returns></returns>
        public RetryTask OnTimeout(Action timeoutAction)
        {
            return new RetryTask { Task = Task.OnTimeout(timeoutAction) };
        }

        /// <summary>
        /// Configures the action to take when the try action timed out before success. 
        /// The total count of attempts is passed as parameter to the <paramref name="timeoutAction"/>.
        /// </summary>
        /// <param name="timeoutAction">The action to take on timeout.</param>
        /// <returns></returns>
        public RetryTask OnTimeout(Action<int> timeoutAction)
        {
            return new RetryTask { Task = Task.OnTimeout((result, tryCount) => timeoutAction(tryCount)) };
        }

        /// <summary>
        /// Configures the action to take after each time the try action fails and before the next try. 
        /// </summary>
        /// <param name="failureAction">The action to take on failure.</param>
        /// <returns></returns>
        public RetryTask OnFailure(Action failureAction)
        {
            return new RetryTask { Task = Task.OnFailure(failureAction) };
        }

        /// <summary>
        /// Configures the action to take after each time the try action fails and before the next try. 
        /// The total count of attempts so far is passed as parameter to the <paramref name="failureAction"/>.
        /// </summary>
        /// <param name="failureAction">The action to take on failure.</param>
        /// <returns></returns>
        public RetryTask OnFailure(Action<int> failureAction)
        {
            return new RetryTask { Task = Task.OnFailure((result, tryCount) => failureAction(tryCount)) };
        }

        /// <summary>
        /// Configures the action to take when the try action succeeds.
        /// </summary>
        /// <param name="successAction">The action to take on success.</param>
        /// <returns></returns>
        public RetryTask OnSuccess(Action successAction)
        {
            return new RetryTask { Task = Task.OnSuccess(successAction) };
        }

        /// <summary>
        /// Configures the action to take when the try action succeeds.
        /// The total count of attempts is passed as parameter to the <paramref name="successAction"/>.
        /// This count includes the final successful one.
        /// </summary>
        /// <param name="successAction">The action to take on success.</param>
        /// <returns></returns>
        public RetryTask OnSuccess(Action<int> successAction)
        {
            return new RetryTask { Task = Task.OnSuccess((result, tryCount) => successAction(tryCount)) };
        }
    }
}

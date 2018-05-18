using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Retry
{
    /// <summary>
    /// Represents the task to be retried.
    /// </summary>
    public class AsyncRetryTask
    {
        public static readonly TimeSpan DefaultTryInterval = TimeSpan.FromMilliseconds(500);

        public static readonly TimeSpan DefaultMaxTryTime = TimeSpan.MaxValue;

        public static readonly int DefaultMaxTryCount = int.MaxValue;

        protected AsyncRetryTask<int> Task;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRetryTask"/> class.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="traceSource">The trace source.</param>
        public AsyncRetryTask(Func<Task> task, TraceSource traceSource)
        {
            Task = new AsyncRetryTask<int>(task.MakeFunc<int>(), traceSource);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRetryTask"/> class.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="traceSource">The trace source.</param>
        /// <param name="maxTryTime">The max try time.</param>
        /// <param name="maxTryCount">The max try count.</param>
        /// <param name="tryInterval">The try interval.</param>
        public AsyncRetryTask(Func<Task> task, TraceSource traceSource,
            TimeSpan maxTryTime, int maxTryCount, TimeSpan tryInterval)
        {
            Task = new AsyncRetryTask<int>(task.MakeFunc<int>(), traceSource,
                maxTryTime, maxTryCount, tryInterval);
        }

        protected AsyncRetryTask()
        {
        }

        /// <summary>
        ///   Retries the task until the specified end condition is satisfied, 
        ///   or the max try time/count is exceeded, or an exception is thrown druing task execution.
        /// </summary>
        /// <param name = "endCondition">The end condition.</param>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public async Task Until(Func<bool> endCondition)
        {
            await Task.Until(endCondition);
        }

        /// <summary>
        ///   Retries the task until the specified end condition is satisfied, 
        ///   or the max try time/count is exceeded, or an exception is thrown druing task execution.
        /// </summary>
        /// <param name = "endCondition">The end condition.</param>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public async Task Until(Func<Task<bool>> endCondition)
        {
            await Task.Until(endCondition);
        }

        /// <summary>
        ///   Retries the task until no exception is thrown during the task execution.
        /// </summary>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public async Task UntilNoException()
        {
            await Task.UntilNoException();
        }

        /// <summary>
        ///   Retries the task until the specified exception is not thrown during the task execution.
        ///   Any other exception thrown is re-thrown.
        /// </summary>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public async Task UntilNoException<TException>()
        {
            await Task.UntilNoException<TException>();
        }

        /// <summary>
        ///   Configures the max try time limit in milliseconds.
        /// </summary>
        /// <param name = "milliseconds">The max try time limit in milliseconds.</param>
        /// <returns></returns>
        public AsyncRetryTask WithTimeLimit(int milliseconds)
        {
            return WithTimeLimit(TimeSpan.FromMilliseconds(milliseconds));
        }

        /// <summary>
        ///   Configures the max try time limit.
        /// </summary>
        /// <param name = "maxTryTime">The max try time limit.</param>
        /// <returns></returns>
        public AsyncRetryTask WithTimeLimit(TimeSpan maxTryTime)
        {
            return new AsyncRetryTask { Task = Task.WithTimeLimit(maxTryTime) };
        }

        /// <summary>
        ///   Configures the try interval time in milliseconds.
        /// </summary>
        /// <param name = "milliseconds">The try interval time in milliseconds.</param>
        /// <returns></returns>
        public AsyncRetryTask WithTryInterval(int milliseconds)
        {
            return WithTryInterval(TimeSpan.FromMilliseconds(milliseconds));
        }

        /// <summary>
        ///   Configures the try interval time.
        /// </summary>
        /// <param name = "tryInterval">The try interval time.</param>
        /// <returns></returns>
        public AsyncRetryTask WithTryInterval(TimeSpan tryInterval)
        {
            return new AsyncRetryTask { Task = Task.WithTryInterval(tryInterval) };
        }

        /// <summary>
        ///   Configures the max try count limit.
        /// </summary>
        /// <param name = "maxTryCount">The max try count.</param>
        /// <returns></returns>
        public AsyncRetryTask WithMaxTryCount(int maxTryCount)
        {
            return new AsyncRetryTask { Task = Task.WithMaxTryCount(maxTryCount) };
        }

        /// <summary>
        /// Configures the action to take when the try action timed out before success. 
        /// </summary>
        /// <param name="timeoutAction">The action to take on timeout.</param>
        /// <returns></returns>
        public AsyncRetryTask OnTimeout(Action timeoutAction)
        {
            return new AsyncRetryTask { Task = Task.OnTimeout(t => timeoutAction()) };
        }

        /// <summary>
        /// Configures the action to take when the try action timed out before success.
        /// The total number of attempts is passed as parameter.
        /// </summary>
        /// <param name="timeoutAction">The action to take on timeout.</param>
        /// <returns></returns>
        public AsyncRetryTask OnTimeout(Action<int> timeoutAction)
        {
            return new AsyncRetryTask { Task = Task.OnTimeout((t, tryCount) => timeoutAction(tryCount)) };
        }

        /// <summary>
        /// Configures the asynchronous action to take when the try action timed out before success. 
        /// </summary>
        /// <param name="timeoutAction">The action to take on timeout.</param>
        /// <returns></returns>
        public AsyncRetryTask OnTimeout(Func<Task> timeoutAction)
        {
            return new AsyncRetryTask { Task = Task.OnTimeout(t => timeoutAction()) };
        }

        /// <summary>
        /// Configures the asynchronous action to take when the try action timed out before success.
        /// The total number of attempts is passed as parameter.
        /// </summary>
        /// <param name="timeoutAction">The action to take on timeout.</param>
        /// <returns></returns>
        public AsyncRetryTask OnTimeout(Func<int, Task> timeoutAction)
        {
            return new AsyncRetryTask { Task = Task.OnTimeout((t, tryCount) => timeoutAction(tryCount)) };
        }

        /// <summary>
        /// Configures the action to take after each time the try action fails and before the next try. 
        /// </summary>
        /// <param name="failureAction">The action to take on failure.</param>
        /// <returns></returns>
        public AsyncRetryTask OnFailure(Action failureAction)
        {
            return new AsyncRetryTask { Task = Task.OnFailure(t => failureAction()) };
        }

        /// <summary>
        /// Configures the action to take after each time the try action fails and before the next try. 
        /// The total number of attempts is passed as parameter.
        /// </summary>
        /// <param name="failureAction">The action to take on failure.</param>
        /// <returns></returns>
        public AsyncRetryTask OnFailure(Action<int> failureAction)
        {
            return new AsyncRetryTask { Task = Task.OnFailure((result, tryCount) => failureAction(tryCount)) };
        }

        /// <summary>
        /// Configures the asynchronous action to take after each time the try action fails and before the next try. 
        /// </summary>
        /// <param name="failureAction">The action to take on failure.</param>
        /// <returns></returns>
        public AsyncRetryTask OnFailure(Func<Task> failureAction)
        {
            return new AsyncRetryTask { Task = Task.OnFailure(t => failureAction()) };
        }

        /// <summary>
        /// Configures the asynchronous action to take after each time the try action fails and before the next try. 
        /// The total number of attempts is passed as parameter.
        /// </summary>
        /// <param name="failureAction">The action to take on failure.</param>
        /// <returns></returns>
        public AsyncRetryTask OnFailure(Func<int, Task> failureAction)
        {
            return new AsyncRetryTask { Task = Task.OnFailure((result, tryCount) => failureAction(tryCount)) };
        }

        /// <summary>
        /// Configures the action to take when the try action succeeds.
        /// </summary>
        /// <param name="successAction">The action to take on success.</param>
        /// <returns></returns>
        public AsyncRetryTask OnSuccess(Action successAction)
        {
            return new AsyncRetryTask { Task = Task.OnSuccess(t => successAction()) };
        }

        /// <summary>
        /// Configures the action to take when the try action succeeds.
        /// The total number of attempts is passed as parameter.
        /// </summary>
        /// <param name="successAction">The action to take on success.</param>
        /// <returns></returns>
        public AsyncRetryTask OnSuccess(Action<int> successAction)
        {
            return new AsyncRetryTask { Task = Task.OnSuccess((t, tryCount) => successAction(tryCount)) };
        }

        /// <summary>
        /// Configures the asynchronous action to take when the try action succeeds.
        /// </summary>
        /// <param name="successAction">The action to take on success.</param>
        /// <returns></returns>
        public AsyncRetryTask OnSuccess(Func<Task> successAction)
        {
            return new AsyncRetryTask { Task = Task.OnSuccess(t => successAction()) };
        }

        /// <summary>
        /// Configures the asynchronous action to take when the try action succeeds.
        /// The total number of attempts is passed as parameter.
        /// </summary>
        /// <param name="successAction">The action to take on success.</param>
        /// <returns></returns>
        public AsyncRetryTask OnSuccess(Func<int, Task> successAction)
        {
            return new AsyncRetryTask { Task = Task.OnSuccess((t, tryCount) => successAction(tryCount)) };
        }
    }
}
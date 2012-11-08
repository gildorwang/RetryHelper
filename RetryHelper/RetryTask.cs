using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace RetryHelper
{
    /// <summary>
    /// Represents the task to be retried.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the retried delegate.</typeparam>
    public class RetryTask<T>
    {
        protected readonly Func<T> Task;
        protected Func<T, bool> EndCondition;
        protected bool RetryOnException;

        protected int MaxTryCount;
        protected TimeSpan MaxTryTime;
        protected TimeSpan TryInterval;

        protected Stopwatch Stopwatch;
        protected string TimeoutErrorMsg;
        protected TraceSource TraceSource;
        protected int TriedCount;

        protected Type ExpectedExceptionType = typeof(Exception);
        protected Exception LastException;

        protected Action<T> OnTimeoutAction = t => { };
        protected Action<T> OnSuccessAction = t => { };

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryTask&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="traceSource">The trace source.</param>
        public RetryTask(Func<T> task, TraceSource traceSource)
            : this(task, traceSource, RetryTask.DefaultMaxTryTime, RetryTask.DefaultMaxTryCount, RetryTask.DefaultTryInterval)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryTask&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="traceSource">The trace source.</param>
        /// <param name="maxTryTime">The max try time.</param>
        /// <param name="maxTryCount">The max try count.</param>
        /// <param name="tryInterval">The try interval.</param>
        public RetryTask(Func<T> task, TraceSource traceSource,
            TimeSpan maxTryTime, int maxTryCount, TimeSpan tryInterval)
        {
            Task = task;
            TraceSource = traceSource;
            MaxTryTime = maxTryTime;
            MaxTryCount = maxTryCount;
            TryInterval = tryInterval;
        }

        /// <summary>
        ///   Retries the task until the specified end condition is satisfied, 
        ///   or the max try time/count is exceeded, or an exception is thrown druing task execution.
        ///   Then returns the value returned by the task.
        /// </summary>
        /// <param name = "endCondition">The end condition.</param>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public T Until(Func<T, bool> endCondition)
        {
            EndCondition = endCondition;
            return TryImpl();
        }

        /// <summary>
        ///   Retries the task until the specified end condition is satisfied, 
        ///   or the max try time/count is exceeded, or an exception is thrown druing task execution.
        ///   Then returns the value returned by the task.
        /// </summary>
        /// <param name = "endCondition">The end condition.</param>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public T Until(Func<bool> endCondition)
        {
            EndCondition = t => endCondition();
            return TryImpl();
        }

        /// <summary>
        ///   Retries the task until no exception is thrown during the task execution.
        /// </summary>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public T UntilNoException()
        {
            RetryOnException = true;
            EndCondition = t => true;
            return TryImpl();
        }

        /// <summary>
        ///   Retries the task until the specified exception is not thrown during the task execution.
        ///   Any other exception thrown is re-thrown.
        /// </summary>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public T UntilNoException<TException>()
        {
            ExpectedExceptionType = typeof(TException);
            return UntilNoException();
        }

        /// <summary>
        ///   Configures the max try time limit in milliseconds.
        /// </summary>
        /// <param name = "milliseconds">The max try time limit in milliseconds.</param>
        /// <returns></returns>
        public RetryTask<T> WithTimeLimit(int milliseconds)
        {
            return WithTimeLimit(TimeSpan.FromMilliseconds(milliseconds));
        }

        /// <summary>
        ///   Configures the max try time limit.
        /// </summary>
        /// <param name = "maxTryTime">The max try time limit.</param>
        /// <returns></returns>
        public RetryTask<T> WithTimeLimit(TimeSpan maxTryTime)
        {
            var retryTask = Clone();
            retryTask.MaxTryTime = maxTryTime;
            return retryTask;
        }

        /// <summary>
        ///   Configures the try interval time in milliseconds.
        /// </summary>
        /// <param name = "milliseconds">The try interval time in milliseconds.</param>
        /// <returns></returns>
        public RetryTask<T> WithTryInterval(int milliseconds)
        {
            return WithTryInterval(TimeSpan.FromMilliseconds(milliseconds));
        }

        /// <summary>
        ///   Configures the try interval time.
        /// </summary>
        /// <param name = "tryInterval">The try interval time.</param>
        /// <returns></returns>
        public RetryTask<T> WithTryInterval(TimeSpan tryInterval)
        {
            var retryTask = Clone();
            retryTask.TryInterval = tryInterval;
            return retryTask;
        }

        /// <summary>
        ///   Configures the max try count limit.
        /// </summary>
        /// <param name = "maxTryCount">The max try count.</param>
        /// <returns></returns>
        public RetryTask<T> WithMaxTryCount(int maxTryCount)
        {
            var retryTask = Clone();
            retryTask.MaxTryCount = maxTryCount;
            return retryTask;
        }

        /// <summary>
        /// Configures the action to take when the try action timed out before success. 
        /// Note that for <see cref="UntilNoException"/>, the parameter passed to the action 
        /// is always <c>default(T)</c>
        /// </summary>
        /// <param name="timeoutAction">The action to take on timeout.</param>
        /// <returns></returns>
        public RetryTask<T> OnTimeout(Action<T> timeoutAction)
        {
            var retryTask = Clone();
            retryTask.OnTimeoutAction += timeoutAction;
            return retryTask;
        }

        /// <summary>
        /// Configures the action to take when the try action succeeds.
        /// </summary>
        /// <param name="successAction">The action to take on success.</param>
        /// <returns></returns>
        public RetryTask<T> OnSuccess(Action<T> successAction)
        {
            var retryTask = Clone();
            retryTask.OnSuccessAction += successAction;
            return retryTask;
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        protected virtual RetryTask<T> Clone()
        {
            return new RetryTask<T>(Task, TraceSource, MaxTryTime, MaxTryCount, TryInterval)
            {
                OnTimeoutAction = OnTimeoutAction,
                OnSuccessAction = OnSuccessAction
            };
        }

        #region Private methods

        private T TryImpl()
        {
            TraceSource.TraceVerbose("Starting trying with max try time {0} and max try count {1}.",
                MaxTryTime, MaxTryCount);
            TriedCount = 0;
            Stopwatch = Stopwatch.StartNew();

            // Start the try loop.
            T result;
            do
            {
                TraceSource.TraceVerbose("Trying time {0}, elapsed time {1}.", TriedCount, Stopwatch.Elapsed);
                result = default(T);

                try
                {
                    result = Task();
                }
                catch(Exception ex)
                {
                    if(ShouldThrow(ex))
                    {
                        throw;
                    }
                    // Otherwise, store the exception and continue.
                    LastException = ex;
                    continue;
                }

                if(EndCondition(result))
                {
                    TraceSource.TraceVerbose("Trying succeeded after time {0} and total try count {1}.",
                        Stopwatch.Elapsed, TriedCount + 1);
                    OnSuccessAction(result);
                    return result;
                }
            } while(ShouldContinue());

            // Should not continue. 
            OnTimeoutAction(result);
            throw new TimeoutException(TimeoutErrorMsg, LastException);
        }

        private bool ShouldThrow(Exception exception)
        {
            // If exception is not recoverable,
            if(exception is OutOfMemoryException || exception is AccessViolationException ||
                // or exception is not expected or not of expected type.
                !RetryOnException || !ExpectedExceptionType.IsInstanceOfType(exception))
            {
                TraceSource.TraceError("{0} detected when trying; throwing...", exception.GetType().Name);
                return true;
            }

            TraceSource.TraceVerbose("{0} detected when trying; continue trying...; details: {1}", exception.GetType().Name, exception);
            return false;
        }

        private bool ShouldContinue()
        {
            if(Stopwatch.Elapsed >= MaxTryTime)
            {
                TimeoutErrorMsg = string.Format(CultureInfo.InvariantCulture,
                    "The maximum try time {0} for the operation has been exceeded.", MaxTryTime);
                return false;
            }
            if(++TriedCount >= MaxTryCount)
            {
                TimeoutErrorMsg = string.Format(CultureInfo.InvariantCulture,
                    "The maximum try count {0} for the operation has been exceeded.", MaxTryCount);
                return false;
            }

            // If should continue, wait some time before that.
            Thread.Sleep(TryInterval);
            return true;
        }

        #endregion
    }

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
        ///   Retries the task until the specified exception is not thrown during the task execution.
        ///   Any other exception thrown is re-thrown.
        /// </summary>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public void UntilNoException<TException>()
        {
            Task.UntilNoException<TException>();
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
            return new RetryTask { Task = Task.OnTimeout(t => timeoutAction()) };
        }

        /// <summary>
        /// Configures the action to take when the try action succeeds.
        /// </summary>
        /// <param name="successAction">The action to take on success.</param>
        /// <returns></returns>
        public RetryTask OnSuccess(Action successAction)
        {
            return new RetryTask { Task = Task.OnSuccess(t => successAction()) };
        }
    }
}
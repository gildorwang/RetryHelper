using System;
using System.Diagnostics;

namespace Retry
{
    /// <summary>
    /// Help retry a delegate until a given condition is met.
    /// </summary>
    public class RetryHelper
    {
        public static readonly string DefaultTraceSourceName = "retryHelperTrace";
        
        protected readonly TraceSource TraceSource;

        #region Singleton

        private static readonly Lazy<RetryHelper> _instance =
            new Lazy<RetryHelper>(() => new RetryHelper());

        /// <summary>
        /// Get the default <see cref="RetryHelper"/> instance which uses the default trace source name.
        /// </summary>
        public static RetryHelper Instance
        {
            get { return _instance.Value; }
        }

        #endregion

        /// <summary>
        ///   Initializes a new instance of the <see cref = "RetryHelper" /> class with the default trace source name.
        /// </summary>
        public RetryHelper() : this(new TraceSource(DefaultTraceSourceName))
        {

        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "RetryHelper" /> class.
        /// </summary>
        /// <param name = "traceSource">The trace source.</param>
        public RetryHelper(TraceSource traceSource)
        {
            TraceSource = traceSource;
            DefaultMaxTryCount = RetryTask.DefaultMaxTryCount;
            DefaultTryInterval = RetryTask.DefaultTryInterval;
            DefaultMaxTryTime = RetryTask.DefaultMaxTryTime;
        }

        /// <summary>
        /// Gets or sets the default max try time.
        /// </summary>
        /// <value>
        /// The default max try time.
        /// </value>
        public TimeSpan DefaultMaxTryTime { get; set; }

        /// <summary>
        /// Gets or sets the default try interval.
        /// </summary>
        /// <value>
        /// The default try interval.
        /// </value>
        public TimeSpan DefaultTryInterval { get; set; }

        /// <summary>
        /// Gets or sets the default max try count.
        /// </summary>
        /// <value>
        /// The default max try count.
        /// </value>
        public int DefaultMaxTryCount { get; set; }

        /// <summary>
        /// Builds a retry task from the specified delegate.
        /// </summary>
        /// <typeparam name="T">Type of return value of the delegate.</typeparam>
        /// <param name="func">The delegate to try.</param>
        /// <returns></returns>
        public RetryTask<T> Try<T>(Func<T> func)
        {
            return new RetryTask<T>(func, TraceSource, DefaultMaxTryTime, DefaultMaxTryCount, DefaultTryInterval);
        }

        /// <summary>
        /// Builds a retry task from the specified delegate.
        /// </summary>
        /// <param name="action">The delegate to try.</param>
        /// <returns></returns>
        public RetryTask Try(Action action)
        {
            return new RetryTask(action, TraceSource, DefaultMaxTryTime, DefaultMaxTryCount, DefaultTryInterval);
        }
    }
}
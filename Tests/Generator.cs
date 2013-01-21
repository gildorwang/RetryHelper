using System;

namespace Tests
{
    class Generator
    {
        private readonly int _trueAfterTimes;
        private readonly bool _throwsException;

        private int _currentTimes;

        public bool RandomExceptionType { get; set; }

        public int TriedTimes
        {
            get { return _currentTimes; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Generator" /> class.
        /// </summary>
        /// <param name="trueAfterTimes">The result will become <c>true</c> after the specified times.</param>
        /// <param name="throwsException">If set to <c>true</c>, an exception will be thrown for each failure.</param>
        public Generator(int trueAfterTimes, bool throwsException = false)
        {
            _trueAfterTimes = trueAfterTimes;
            _throwsException = throwsException;
        }

        public bool Next()
        {
            var result =  (_currentTimes = TriedTimes + 1) > _trueAfterTimes;
            if (!result && _throwsException)
            {
                if (RandomExceptionType && TriedTimes % 2 == 0)
                {
                    Console.WriteLine("Throwing InvalidOperationException");
                    throw new InvalidOperationException();
                }
                else
                {
                    Console.WriteLine("Throwing ApplicationException");
                    throw new ApplicationException();
                }
            }
            return result;
        }
    }
}
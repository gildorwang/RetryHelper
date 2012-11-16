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
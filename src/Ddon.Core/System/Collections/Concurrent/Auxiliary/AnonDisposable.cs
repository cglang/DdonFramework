﻿namespace System.Collections.Concurrent.Auxiliary
{
    internal class AnonDisposable : IDisposable
    {
        private Action? _dispose;
        public AnonDisposable(Action dispose)
        {
            _dispose = dispose;
        }

        ~AnonDisposable()
        {
            Dispose(false);
        }

        protected void Dispose(bool disposing)
        {
            if (_dispose != null)
            {
                _dispose();
            }
            _dispose = null;
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}

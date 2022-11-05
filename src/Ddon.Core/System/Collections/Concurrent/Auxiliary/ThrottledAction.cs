﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace System.Collections.Concurrent.Auxiliary
{
    internal class ThrottledAction
    {
        private Action _action;
        private TimeSpan _timeBetweenInvokations;
        private Task _actionTask = Task.Run(() => { });
        private int _actionsQueued = 0;

        public ThrottledAction(Action action, TimeSpan timeBetweenInvokations)
        {
            _action = action;
            _timeBetweenInvokations = timeBetweenInvokations;
        }

        public void InvokeAction()
        {
            if (_actionsQueued < 1)
            {
                Interlocked.Increment(ref _actionsQueued);
                _actionTask = _actionTask.ContinueWith((t) =>
                {
                    Interlocked.Decrement(ref _actionsQueued);
                    _action();
                    Thread.Sleep(_timeBetweenInvokations);
                });
            }
        }
    }
}

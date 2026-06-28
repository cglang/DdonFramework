using System;
using System.Threading.Tasks;

namespace Ddon.Socket.Session
{
    internal class RequestEventListener
    {
        private readonly TaskCompletionSource<string> _taskCompletion;

        public Guid Id { get; }

        public bool IsCompleted { get; private set; }

        public RequestEventListener()
        {
            _taskCompletion = new TaskCompletionSource<string>();
            Id = Guid.NewGuid();
        }

        public Task<string> ResultAsync()
        {
            TimeoutRecordProcessor.Add(this);
            return _taskCompletion.Task;
        }

        public void ActionHandler(string data)
        {
            _taskCompletion.SetResult(data);
            IsCompleted = true;
            TimeoutRecordProcessor.Remove(Id);
        }

        public void ExceptionHandler(Exception ex)
        {
            _taskCompletion.SetException(ex);
            IsCompleted = true;
            TimeoutRecordProcessor.Remove(Id);
        }
    }
}

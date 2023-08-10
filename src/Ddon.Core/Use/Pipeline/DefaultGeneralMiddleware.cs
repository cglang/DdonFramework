using System;
using System.Threading.Tasks;

namespace Ddon.Core.Use.Pipeline;

public class DefaultGeneralMiddleware<T> : IGeneralMiddleware<T>
{
    private readonly Action<T> _actionExecuting;

    private readonly Action<T>? _actionExecuted;

    public DefaultGeneralMiddleware(Action<T> actionExecuting)
    {
        _actionExecuting = actionExecuting;
    }

    public DefaultGeneralMiddleware(Action<T> actionExecuting, Action<T> actionExecuted)
    {
        _actionExecuting = actionExecuting;
        _actionExecuted = actionExecuted;
    }

    public async Task InvokeAsync(T context, MiddlewareDelegate<T> next)
    {
        _actionExecuting.Invoke(context);

        await next(context);

        _actionExecuted?.Invoke(context);
    }
}

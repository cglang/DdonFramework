using Ddon.Pipeline.StatePipe;
using Test.ConsoleApp;

//PluginTest.RunAsync();


await GeneralCustomStatePipe.GeneralCustomPipeline(
    new StatePipeMiddleware<object>(
        con => Task.FromResult(true),
        async con =>
        {
            Console.WriteLine("Middleware 1");
            await Task.Delay(1000);
            Console.WriteLine("Middleware 1.1");
        },
        con => Task.CompletedTask,
        con => Task.CompletedTask
    ),
    new StatePipeMiddleware<object>(
        con => Task.FromResult(true),
        async con =>
        {
            Console.WriteLine("Middleware 2");
            await Task.Delay(1000);
            Console.WriteLine("Middleware 2.1");
        },
        con => Task.CompletedTask,
        con => Task.CompletedTask
    )
).ExecuteAsync(new object());

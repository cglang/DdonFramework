using System.IO;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Ddon.Core.Use.Plugin;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Test.ConsoleApp
{
    public class PluginTest
    {
        public static void RunAsync()
        {
            var logger = new ConsoleLogger("test");

            logger.LogInformation("test2222");

            IOptions<PluginOptions> options = Options.Create(new PluginOptions("plugin"));

            IPluginFileListenerServer fileL = new PluginFileListenerServer(new ConsoleLogger("test"), options);

            fileL.Start();

            Console.ReadKey();
        }
    }

    class ConsoleLogger : ILogger
    {
        public string _category;

        public ConsoleLogger(string Category)
        {
            _category = Category;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var logEntry = new LogEntry<TState>(logLevel, _category, eventId, state, exception, formatter);

            Console.WriteLine(logEntry.State);
        }
    }

    public class PluginDemoOne : IPlugin
    {
        public PluginInfo GetPluginInformation()
        {
            return new PluginInfo("测试插件", "0.0.1", "cglang", DateTime.Now);
        }

        public Task LoadAsync()
        {
            Console.WriteLine("插件加载了");
            return Task.CompletedTask;
        }

        public void Dispose()
        {

        }
    }
}

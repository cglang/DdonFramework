using Avalonia;

namespace Test.Desktop.Avalonia
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .StartWithClassicDesktopLifetime(args);
        }
    }
}

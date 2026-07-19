using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Threading;
using Ddon.Desktop.Bridge;
using Ddon.Desktop.Transport;

namespace Ddon.Desktop.Avalonia;

public partial class MainWindow : Window
{
    private AvaloniaWebViewTransport? _transport;

    public IBridgeDispatcher? BridgeDispatcher { get; set; }

    private Func<Task>? _onClosing;

    public MainWindow()
    {
        InitializeComponent();
        Closing += async (s, e) =>
        {
            if (_onClosing is not null)
                await _onClosing();
        };
    }

    public void SetOnClosing(Func<Task> onClosing)
    {
        _onClosing = onClosing;
    }

    public void ApplyConfig(string title, string loadingTitle, string loadingText,
        WindowState windowState = WindowState.Maximized, int width = 800, int height = 500)
    {
        Title = title;
        WindowState = windowState;
        Width = width;
        Height = height;
        LoadingTitleBlock.Text = loadingTitle;
        LoadingTextBlock.Text = loadingText;
    }

    public void SetLoadingText(string text)
    {
        Dispatcher.UIThread.InvokeAsync(() => LoadingTextBlock.Text = text);
    }

    public Task InitializeWebViewAsync(string url)
    {
        _transport = new AvaloniaWebViewTransport
        {
            WebView = WebView,
            OnInvoke = async (method, payload) =>
            {
                if (BridgeDispatcher is not null)
                    return await BridgeDispatcher.DispatchAsync(method, payload);
                throw new InvalidOperationException("Dispatcher not configured");
            }
        };

        WebView.EnvironmentRequested += (sender, args) =>
        {
            if (args is global::Avalonia.Platform.LinuxWpeWebViewEnvironmentRequestedEventArgs linuxArgs)
            {
                linuxArgs.PreferWebKitGtkInstead = true;
            }
        };

        WebView.WebMessageReceived += async (sender, args) =>
        {
            if (_transport is not null && args.Body is not null)
                await _transport.HandleMessage(args.Body);
        };

        WebView.NavigationCompleted += async (sender, args) =>
        {
            WebView.IsVisible = true;
            LoadingPanel.Transitions = new Transitions
            {
                new DoubleTransition
                {
                    Property = OpacityProperty,
                    Duration = TimeSpan.FromMilliseconds(400)
                }
            };
            LoadingPanel.Opacity = 0;
            await Task.Delay(400);
            LoadingPanel.IsVisible = false;
            LoadingPanel.Opacity = 1;
            LoadingPanel.Transitions = null;
            if (_transport is not null)
                await _transport.InjectBridgeAsync();
        };

        WebView.Navigate(new Uri(url));
        return Task.CompletedTask;
    }
}

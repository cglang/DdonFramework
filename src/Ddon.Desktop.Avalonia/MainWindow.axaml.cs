using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Threading;
using Ddon.Desktop.Avalonia.Transport;

namespace Ddon.Desktop.Avalonia;

public partial class MainWindow : Window
{
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

    public Task InitializeWebViewAsync(string url, AvaloniaWebViewTransport transport)
    {
        transport.WebView = WebView;

        WebView.EnvironmentRequested += (sender, args) =>
        {
            if (args is global::Avalonia.Platform.LinuxWpeWebViewEnvironmentRequestedEventArgs linuxArgs)
            {
                linuxArgs.PreferWebKitGtkInstead = true;
            }
        };

        WebView.WebMessageReceived += async (sender, args) =>
        {
            if (args.Body is null) return;

            // args.Body 为 JSON 字符串,在 transport 内反序列化为具体的消息对象
            await transport.HandleMessage(args.Body);
        };

        WebView.NavigationCompleted += async (sender, args) =>
        {
            await HideLoadingAsync();
            await ShowWebViewAsync();
            await transport.InjectBridgeAsync();
        };

        WebView.Navigate(new Uri(url));

        return Task.CompletedTask;
    }

    private async Task HideLoadingAsync()
    {
        const int transitionsMicroseconds = 300;
        LoadingPanel.Transitions =
        [
            new DoubleTransition
            {
                Property = OpacityProperty,
                Duration = TimeSpan.FromSeconds((double)transitionsMicroseconds / 1000),
                Easing = new CubicEaseOut()
            }
        ];

        LoadingPanel.Opacity = 0;

        await Task.Delay(transitionsMicroseconds);

        LoadingPanel.IsVisible = false;
        LoadingPanel.Transitions = null;
        LoadingPanel.Opacity = 1;
    }

    private Task ShowWebViewAsync()
    {
        // 显示 WebView
        WebView.IsVisible = true;
        return Task.CompletedTask;
    }
}

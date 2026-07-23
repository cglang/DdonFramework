using System.Windows;
using System.Windows.Threading;
using Ddon.Desktop.Core.Bridge;
using Ddon.Desktop.Core.Transport;

namespace Ddon.Desktop.Wpf;

public partial class MainWindow : Window
{
    private WebViewTransport? _transport;

    public IBridgeDispatcher? BridgeDispatcher { get; set; }

    public MainWindow()
    {
        InitializeComponent();
    }

    public void ApplyConfig(string title, string loadingTitle, string loadingText)
    {
        Title = title;
        LoadingTitleBlock.Text = loadingTitle;
        LoadingTextBlock.Text = loadingText;
    }

    public void SetLoadingText(string text)
    {
        Dispatcher.Invoke(() => LoadingTextBlock.Text = text);
    }

    public async Task InitializeWebViewAsync(string url)
    {
        await WebView.EnsureCoreWebView2Async();

        _transport = new WebViewTransport
        {
            ChromeWebView = WebView.CoreWebView2,
            OnInvoke = async (method, payload) =>
            {
                if (BridgeDispatcher is not null)
                    return await BridgeDispatcher.DispatchAsync(method, payload);
                throw new InvalidOperationException("Dispatcher not configured");
            }
        };

        WebView.CoreWebView2.WebMessageReceived += async (sender, args) =>
        {
            if (_transport is not null)
                await _transport.HandleMessage(args.TryGetWebMessageAsString());
        };

        LoadingPanel.Visibility = Visibility.Collapsed;
        WebView.Visibility = Visibility.Visible;
        WebView.CoreWebView2.Navigate(url);
    }
}

using Ddon.Desktop.Annotations;
using Ddon.Desktop.Bridge;

namespace Test.Desktop.Avalonia.Services;

[BridgeService(Name = "Auth")]
public class AuthService
{
    private readonly IUiBridge _bridge;
    private PlcEngine? _engine;
    private string? _currentToken;

    public AuthService(IUiBridge bridge)
    {
        _bridge = bridge;
    }

    [BridgeMethod(Name = "Login")]
    public Task<string?> Login(string username, string password)
    {
        if (username == "admin" && password == "123456")
        {
            _currentToken = Guid.NewGuid().ToString();
            return Task.FromResult<string?>(_currentToken);
        }
        return Task.FromResult<string?>(null);
    }

    [BridgeMethod(Name = "Logout")]
    public Task Logout()
    {
        _engine?.Stop();
        _currentToken = null;
        return Task.CompletedTask;
    }

    [BridgeMethod(Name = "StartPlcSimulator")]
    public Task StartPlcSimulator()
    {
        _engine = new PlcEngine(_bridge);
        _engine.Start();
        return Task.CompletedTask;
    }

    public bool IsAuthenticated => _currentToken is not null;
}

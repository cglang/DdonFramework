using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Opc.Ua;
using Opc.Ua.Configuration;
using Opc.Ua.Server;
using Ddon.OpcUaServer.NodeManager;

namespace Ddon.OpcUaServer.Server;

/// <summary>
/// OPC UA Server 实现。
/// 负责 Server 的生命周期管理、证书处理和地址空间初始化。
/// 参考 Gateway.OPCUA 实现模式：使用 ApplicationInstance + StandardServer + INodeManagerFactory。
/// SDK 1.5.378.156 提供完整的 async API。
/// </summary>
internal sealed class VitrinUaServer : IVitrinUaServer
{
    private readonly VitrinUaServerOptions _options;
    private readonly ILogger<VitrinUaServer> _logger;
    private readonly VitrinNodeManagerImpl _nodeManager;

    private ApplicationInstance? _application;
    private UaServerCore? _serverCore;
    private bool _isRunning;
    private bool _disposed;

    public bool IsRunning => _isRunning;
    public string EndpointUrl => _options.EndpointUrl;
    public IVitrinNodeManager NodeManager => _nodeManager;

    public event EventHandler<ServerStatusChangedEventArgs>? StatusChanged;

    public VitrinUaServer(
        IOptions<VitrinUaServerOptions> options,
        IEnumerable<INodeProvider> providers,
        ILogger<VitrinUaServer> logger)
    {
        _options = options.Value;
        _logger = logger;
        _nodeManager = new VitrinNodeManagerImpl(providers, logger);
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        if (_isRunning)
        {
            _logger.LogWarning("OPC UA Server 已在运行中。");
            return;
        }

        _logger.LogInformation("正在启动 OPC UA Server '{ServerName}'...", _options.ServerName);

        try
        {
            // 1. 创建 ApplicationConfiguration（参考 Gateway 模式）
            var config = CreateServerConfiguration();

            // 2. 验证配置（参考 Gateway：await config.ValidateAsync）
            await config.ValidateAsync(ApplicationType.Server);

            // 3. 创建 ApplicationInstance（参考 Gateway：带 externalConfig = null 的 2 参数构造函数）
            _application = new ApplicationInstance(config, null)
            {
                ApplicationName = _options.ServerName,
                ApplicationType = ApplicationType.Server,
            };

            // 4. 处理证书（参考 Gateway：CheckApplicationInstanceCertificatesAsync，注意复数 Certificates）
            await EnsureCertificateAsync(ct);

            // 5. 创建 Server 实例
            _serverCore = new UaServerCore(_nodeManager, _logger);

            // 6. 启动 Server（参考 Gateway：await _application.StartAsync）
            await _application.StartAsync(_serverCore);
            _isRunning = true;

            _logger.LogInformation(
                "OPC UA Server '{ServerName}' 已启动，端点: {Endpoint}",
                _options.ServerName, _options.EndpointUrl);

            OnStatusChanged(true, "Server 已启动");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OPC UA Server 启动失败。");
            OnStatusChanged(false, $"启动失败: {ex.Message}");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        if (!_isRunning) return;

        _logger.LogInformation("正在停止 OPC UA Server '{ServerName}'...", _options.ServerName);

        try
        {
            // 参考 Gateway：await _application.StopAsync()
            if (_application != null)
            {
                await _application.StopAsync();
            }

            _serverCore?.Dispose();
            _serverCore = null;
            _application = null;
            _isRunning = false;

            _logger.LogInformation("OPC UA Server '{ServerName}' 已停止。", _options.ServerName);
            OnStatusChanged(false, "Server 已停止");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OPC UA Server 停止时发生异常。");
            OnStatusChanged(false, $"停止异常: {ex.Message}");
        }
    }

    private static ApplicationConfiguration CreateServerConfiguration(VitrinUaServerOptions options)
    {
        var config = new ApplicationConfiguration
        {
            ApplicationName = options.ServerName,
            ApplicationUri = $"urn:{Environment.MachineName}:{options.ServerName}",
            ProductUri = "https://github.com/cglang/DdonFramework",
            ApplicationType = ApplicationType.Server,

            ServerConfiguration = new ServerConfiguration
            {
                BaseAddresses = { options.EndpointUrl },
                SecurityPolicies = new ServerSecurityPolicyCollection
                {
                    new ServerSecurityPolicy
                    {
                        SecurityMode = MessageSecurityMode.None,
                        SecurityPolicyUri = SecurityPolicies.None,
                    },
                },
                MinRequestThreadCount = 2,
                MaxRequestThreadCount = 10,
                MaxQueuedRequestCount = 200,
            },

            TransportQuotas = new TransportQuotas
            {
                MaxMessageSize = 4194304,
                MaxByteStringLength = 1048576,
                MaxStringLength = 1048576,
                MaxArrayLength = 65535,
                MaxBufferSize = 65535,
                OperationTimeout = 120000,
            },

            SecurityConfiguration = new SecurityConfiguration
            {
                // 必须指定 ApplicationCertificate，否则启动时报 "ApplicationCertificate must be specified"
                ApplicationCertificate = new CertificateIdentifier
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = "OPC UA/CertificateStores/MachineDefault",
                    SubjectName = options.ServerName,
                },
                TrustedIssuerCertificates = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = "OPC UA/CertificateStores/UA Certificate Authorities",
                },
                TrustedPeerCertificates = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = "OPC UA/CertificateStores/UA Applications",
                },
                RejectedCertificateStore = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = "OPC UA/CertificateStores/RejectedCertificates",
                },
                AutoAcceptUntrustedCertificates = options.AllowAnonymous,
                RejectSHA1SignedCertificates = false,
                MinimumCertificateKeySize = 1024,
            },

            TransportConfigurations = new TransportConfigurationCollection(),
            TraceConfiguration = new TraceConfiguration(),
            ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
        };

        // 添加 UserTokenPolicy（参考 Gateway）
        config.ServerConfiguration.UserTokenPolicies.Add(new UserTokenPolicy(UserTokenType.Anonymous));

        return config;
    }

    private ApplicationConfiguration CreateServerConfiguration()
    {
        return CreateServerConfiguration(_options);
    }

    private async Task EnsureCertificateAsync(CancellationToken ct)
    {
        if (_application == null) return;

        try
        {
            // 参考 Gateway：CheckApplicationInstanceCertificatesAsync（async，复数 Certificates）
            await _application.CheckApplicationInstanceCertificatesAsync(
                false, CertificateFactory.DefaultKeySize, ct);

            _logger.LogInformation("OPC UA 证书已就绪。");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OPC UA 证书处理失败，将尝试无证书启动。");
        }
    }

    private void OnStatusChanged(bool running, string message)
    {
        StatusChanged?.Invoke(this, new ServerStatusChangedEventArgs
        {
            IsRunning = running,
            Message = message,
            Timestamp = DateTime.UtcNow,
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        await StopAsync();
        _serverCore?.Dispose();
        _serverCore = null;
        _application = null;

        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// 基于 OPC UA SDK StandardServer 的 Server 内核。
/// 参考 Gateway：使用 INodeManagerFactory + AddNodeManager 模式。
/// </summary>
internal sealed class UaServerCore : StandardServer
{
    private readonly VitrinNodeManagerImpl _nodeManagerImpl;
    private readonly ILogger _logger;

    public UaServerCore(VitrinNodeManagerImpl nodeManagerImpl, ILogger logger)
    {
        _nodeManagerImpl = nodeManagerImpl;
        _logger = logger;
    }

    protected override void OnServerStarting(ApplicationConfiguration configuration)
    {
        _logger.LogInformation("OPC UA Server 内部正在启动...");

        // 参考 Gateway：通过 AddNodeManager(INodeManagerFactory) 注册自定义 NodeManager
        AddNodeManager(new VitrinNodeManagerFactory(_nodeManagerImpl));

        base.OnServerStarting(configuration);
    }

    protected override void OnServerStarted(IServerInternal server)
    {
        base.OnServerStarted(server);
        _logger.LogInformation("OPC UA Server 内部已启动");
    }
}

/// <summary>
/// NodeManager 工厂。
/// 参考 Gateway：实现 INodeManagerFactory 接口，由 AddNodeManager 在 OnServerStarting 中调用。
/// </summary>
internal sealed class VitrinNodeManagerFactory : INodeManagerFactory
{
    private readonly VitrinNodeManagerImpl _nodeManagerImpl;

    public VitrinNodeManagerFactory(VitrinNodeManagerImpl nodeManagerImpl)
    {
        _nodeManagerImpl = nodeManagerImpl;
    }

    public StringCollection NamespacesUris => new StringCollection
    {
        VitrinNodeManagerImpl.DefaultNamespaceUri,
    };

    public INodeManager Create(IServerInternal server, ApplicationConfiguration configuration)
    {
        // 参考 Gateway：在 Create 中构造自定义 NodeManager
        var nodeManager = new VitrinSdkNodeManager(server, _nodeManagerImpl, _nodeManagerImpl.Logger);
        _nodeManagerImpl.SetSdkNodeManager(nodeManager);
        return nodeManager;
    }
}

using Ddon.Desktop.Core.Annotations;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Clients;
using Ddon.VitrinPLC.Clients.Mitsubishi;
using Ddon.VitrinPLC.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using S7.Net;
using VitrinRuntime.Desktop.Exceptions;
using VitrinRuntime.Desktop.Stores;

namespace VitrinRuntime.Desktop.Services;

[BridgeService(Name = "PlcManager")]
public sealed class PlcManager
{
    private readonly IPlcHub _hub;
    private readonly IPlcConfigStore _store;
    private readonly TagSubscriptionManager _subscriptionManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PlcManager> _logger;

    public PlcManager(
        IPlcHub hub,
        IPlcConfigStore store,
        TagSubscriptionManager subscriptionManager,
        IServiceProvider serviceProvider,
        ILogger<PlcManager> logger)
    {
        _hub = hub;
        _store = store;
        _subscriptionManager = subscriptionManager;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    [BridgeMethod(Name = "ListPlcs")]
    public List<PlcConfig> ListPlcs()
    {
        var plcs = _store.GetAllPlcs();
        foreach (var plc in plcs)
        {
            try
            {
                var session = _hub.For(plc.Name);
                plc.IsConnected = session.Mirror.LastUpdateTime > DateTime.MinValue;
            }
            catch
            {
                plc.IsConnected = false;
            }
        }
        return plcs;
    }

    [BridgeMethod(Name = "AddPlc")]
    public async Task<PlcConfig> AddPlc(AddPlcRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            throw new UserFriendlyException("PLC 名称不能为空。");

        var config = new PlcConfig
        {
            Name = req.Name,
            PlcType = req.PlcType,
            Ip = req.Ip,
            Port = req.Port,
            ScanInterval = req.ScanInterval,
            AutoConnect = req.AutoConnect,
            ConnectionOptions = req.ConnectionOptions,
            CreatedAt = DateTime.UtcNow
        };

        _store.AddPlc(config);

        try
        {
            var client = CreateClient(req.Name, req.PlcType, req.Ip, req.Port, req.ConnectionOptions);
            var endian = req.PlcType == "Siemens" ? EndianFormat.ABCD : EndianFormat.DCBA;

            await _hub.AddPlcAsync(req.Name, client, host =>
            {
                host.ScanInterval = req.ScanInterval;
                host.Endian = endian;
            });

            _store.UpdatePlcConnection(req.Name, true);
            _logger.LogInformation("PLC '{Name}' 已添加并连接。", req.Name);
        }
        catch (Exception ex)
        {
            _store.UpdatePlcConnection(req.Name, false, ex.Message);
            _logger.LogError(ex, "添加 PLC '{Name}' 失败。", req.Name);
            throw;
        }

        return config;
    }

    [BridgeMethod(Name = "RemovePlc")]
    public async Task RemovePlc(PlcNameRequest req)
    {
        try
        {
            await _hub.RemovePlcAsync(req.Name);
        }
        catch (KeyNotFoundException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "移除 PLC '{Name}' 时引擎停止异常。", req.Name);
        }

        _store.RemovePlc(req.Name);
        _logger.LogInformation("PLC '{Name}' 已移除。", req.Name);
    }

    [BridgeMethod(Name = "ConnectPlc")]
    public async Task ConnectPlc(PlcNameRequest req)
    {
        var config = _store.GetPlc(req.Name)
            ?? throw new UserFriendlyException($"PLC '{req.Name}' 未找到。");

        try
        {
            var client = CreateClient(req.Name, config.PlcType, config.Ip, config.Port, config.ConnectionOptions);
            var endian = config.PlcType == "Siemens" ? EndianFormat.ABCD : EndianFormat.DCBA;

            try { await _hub.RemovePlcAsync(req.Name); } catch { }

            await _hub.AddPlcAsync(req.Name, client, host =>
            {
                host.ScanInterval = config.ScanInterval;
                host.Endian = endian;

                var tags = _store.GetAllTagsForPlc(req.Name);
                foreach (var tag in tags)
                {
                    var group = _store.GetGroup(tag.GroupId);
                    var fullName = group is not null ? $"{req.Name}.{group.Name}.{tag.Name}" : tag.Name;
                    host.MapTag(fullName, tag.Address, tag.DataType, tag.StringLength);
                }
            });

            _subscriptionManager.SubscribeAllTags(req.Name);

            _store.UpdatePlcConnection(req.Name, true);
            _logger.LogInformation("PLC '{Name}' 已连接。", req.Name);
        }
        catch (Exception ex)
        {
            _store.UpdatePlcConnection(req.Name, false, ex.Message);
            _logger.LogError(ex, "连接 PLC '{Name}' 失败。", req.Name);
            throw;
        }
    }

    [BridgeMethod(Name = "DisconnectPlc")]
    public async Task DisconnectPlc(PlcNameRequest req)
    {
        _subscriptionManager.UnsubscribePlc(req.Name);

        try
        {
            await _hub.RemovePlcAsync(req.Name);
            _store.UpdatePlcConnection(req.Name, false);
            _logger.LogInformation("PLC '{Name}' 已断开。", req.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "断开 PLC '{Name}' 失败。", req.Name);
            throw;
        }
    }

    [BridgeMethod(Name = "UpdatePlc")]
    public async Task<PlcConfig> UpdatePlc(UpdatePlcRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            throw new UserFriendlyException("PLC 名称不能为空。");

        var oldConfig = _store.GetPlc(req.OldName)
            ?? throw new UserFriendlyException($"PLC '{req.OldName}' 未找到。");

        var newConfig = new PlcConfig
        {
            Id = oldConfig.Id,
            Name = req.Name,
            PlcType = oldConfig.PlcType,
            Ip = req.Ip,
            Port = req.Port,
            ScanInterval = req.ScanInterval,
            AutoConnect = req.AutoConnect,
            IsConnected = oldConfig.IsConnected,
            CreatedAt = oldConfig.CreatedAt,
            LastConnectedAt = oldConfig.LastConnectedAt,
            ErrorMessage = oldConfig.ErrorMessage,
            ConnectionOptions = req.ConnectionOptions
        };

        _store.UpdatePlc(req.OldName, newConfig);

        if (oldConfig.IsConnected)
        {
            try
            {
                _subscriptionManager.UnsubscribePlc(req.OldName);
                await _hub.RemovePlcAsync(req.OldName);

                var client = CreateClient(req.Name, oldConfig.PlcType, req.Ip, req.Port, req.ConnectionOptions);
                var endian = oldConfig.PlcType == "Siemens" ? EndianFormat.ABCD : EndianFormat.DCBA;

                await _hub.AddPlcAsync(req.Name, client, host =>
                {
                    host.ScanInterval = req.ScanInterval;
                    host.Endian = endian;

                    var tags = _store.GetAllTagsForPlc(req.Name);
                    foreach (var tag in tags)
                    {
                        var group = _store.GetGroup(tag.GroupId);
                        var fullName = group is not null ? $"{req.Name}.{group.Name}.{tag.Name}" : tag.Name;
                        host.MapTag(fullName, tag.Address, tag.DataType, tag.StringLength);
                    }
                });

                _subscriptionManager.SubscribeAllTags(req.Name);
                newConfig.IsConnected = true;
                _logger.LogInformation("PLC '{OldName}' 配置已更新并重新连接。", req.OldName);
            }
            catch (Exception ex)
            {
                newConfig.IsConnected = false;
                _logger.LogWarning(ex, "PLC '{OldName}' 配置已更新但重新连接失败。", req.OldName);
            }
        }

        return newConfig;
    }

    [BridgeMethod(Name = "GetPlcStatus")]
    public object? GetPlcStatus(PlcNameRequest req)
    {
        var config = _store.GetPlc(req.Name);
        if (config is null) return null;

        try
        {
            var session = _hub.For(req.Name);
            return new
            {
                name = config.Name,
                ip = config.Ip,
                port = config.Port,
                isConnected = true,
                mirrorVersion = session.Mirror.Version,
                lastUpdateTime = session.Mirror.LastUpdateTime,
                tagCount = session.Tags.Count
            };
        }
        catch
        {
            return new
            {
                name = config.Name,
                ip = config.Ip,
                port = config.Port,
                isConnected = false,
                errorMessage = config.ErrorMessage
            };
        }
    }

    private IPlcClient CreateClient(string name, string plcType, string ip, int port, Dictionary<string, string> opts)
    {
        var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();

        return plcType switch
        {
            "Mitsubishi" => new McProtocolClient(
                name, ip, port,
                (McProtocolFrame)int.Parse(opts.GetValueOrDefault("mcProtocolFrame", "11"))),
            _ => new SiemensClient(new SiemensOptions
            {
                Name = name,
                Ip = ip,
                Port = port,
                Rack = int.Parse(opts.GetValueOrDefault("rack", "0")),
                Slot = int.Parse(opts.GetValueOrDefault("slot", "1")),
                CpuType = (CpuType)int.Parse(opts.GetValueOrDefault("cpuType", "40"))
            }, loggerFactory.CreateLogger<SiemensClient>())
        };
    }
}

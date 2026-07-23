using Ddon.Desktop.Core.Annotations;
using Ddon.VitrinPLC;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Clients;
using Ddon.VitrinPLC.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace VitrinRuntime.Services;

[BridgeService(Name = "PlcManager")]
public sealed class PlcManagerService
{
    private readonly IPlcHub _hub;
    private readonly IPlcConfigStore _store;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PlcManagerService> _logger;

    public PlcManagerService(
        IPlcHub hub,
        IPlcConfigStore store,
        IServiceProvider serviceProvider,
        ILogger<PlcManagerService> logger)
    {
        _hub = hub;
        _store = store;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    [BridgeMethod(Name = "ListPlcs")]
    public List<PlcConfig> ListPlcs()
    {
        var plcs = _store.GetAllPlcs();
        // 同步连接状态
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
            throw new ArgumentException("PLC 名称不能为空。");

        var config = new PlcConfig
        {
            Name = req.Name,
            Ip = req.Ip,
            Port = req.Port,
            Rack = req.Rack,
            Slot = req.Slot,
            CreatedAt = DateTime.UtcNow
        };

        _store.AddPlc(config);

        try
        {
            var connOpts = new SiemensOptions
            {
                Name = req.Name,
                Ip = req.Ip,
                Port = req.Port,
                Rack = req.Rack,
                Slot = req.Slot
            };

            var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
            var client = new SiemensClient(connOpts, loggerFactory.CreateLogger<SiemensClient>());

            await _hub.AddPlcAsync(req.Name, client, host =>
            {
                host.ScanInterval = 200;
                host.Endian = EndianFormat.ABCD;
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
            ?? throw new KeyNotFoundException($"PLC '{req.Name}' 未找到。");

        try
        {
            var connOpts = new SiemensOptions
            {
                Name = req.Name,
                Ip = config.Ip,
                Port = config.Port,
                Rack = config.Rack,
                Slot = config.Slot
            };

            var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
            var client = new SiemensClient(connOpts, loggerFactory.CreateLogger<SiemensClient>());

            try { await _hub.RemovePlcAsync(req.Name); } catch { }

            await _hub.AddPlcAsync(req.Name, client, host =>
            {
                host.ScanInterval = 200;
                host.Endian = EndianFormat.ABCD;

                var tags = _store.GetAllTagsForPlc(req.Name);
                foreach (var tag in tags)
                    host.MapTag(tag.Name, tag.Address, tag.DataType, tag.StringLength);

                // 预注册各 DB 块的镜像区域（使用自定义大小，替代默认的 4096）
                var groups = _store.GetGroupsByPlc(req.Name);
                foreach (var group in groups)
                {
                    var regionKey = $"DB{group.DbNumber}";
                    var area = $"DB{group.DbNumber}";
                    host.MapRegion(regionKey, area, 0, group.DbSize);
                }
            });

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
}

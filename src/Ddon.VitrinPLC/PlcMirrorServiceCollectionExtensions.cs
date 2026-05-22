using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ddon.VitrinPLC.Clients;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.TagEngine;
using Ddon.VitrinPLC.SyncEngine;
using Plc.Hosting;

namespace Ddon.VitrinPLC
{
    // ─────────────────────────────────────────────
    // IServiceCollection 扩展
    // ─────────────────────────────────────────────
    public static class PlcMirrorServiceCollectionExtensions
    {
        /// <summary>
        /// 注册 PLC 统一内存镜像框架所有服务。
        ///
        /// 示例：
        /// <code>
        /// services.AddPlcMirror(x =>
        /// {
        ///     x.UseSiemens("Main", plc =>
        ///     {
        ///         plc.Ip   = "192.168.1.10";
        ///         plc.Port = 102;
        ///     });
        ///     x.ScanInterval = 200;
        ///     x.MapTag("Temp", "DB1.DBD0",   PlcDataType.Float);
        ///     x.MapTag("Run",  "DB1.DBX10.0", PlcDataType.Bool);
        ///     x.MapTag("Count","D100",        PlcDataType.Int16);
        /// });
        /// </code>
        /// </summary>
        public static IServiceCollection AddPlcMirror(this IServiceCollection services, Action<PlcMirrorOptions> configure)
        {
            var options = new PlcMirrorOptions();

            configure(options);

            // ── 协议客户端 ────────────────────────────────
            services.AddSingleton<IPlcClient>(sp =>
            {
                var logFactory = sp.GetRequiredService<ILoggerFactory>();
                return options.Protocol switch
                {
                    PlcClientType.Siemens => new SiemensClient(options.Siemens,
                                                 logFactory.CreateLogger<SiemensClient>()),
                    PlcClientType.Mitsubishi => new MitsubishiClient(options.Mitsubishi,
                                                 logFactory.CreateLogger<MitsubishiClient>()),
                    PlcClientType.Omron => new OmronClient(options.Omron,
                                                 logFactory.CreateLogger<OmronClient>()),
                    _ => throw new InvalidOperationException(
                             "请调用 UseSiemens / UseMitsubishi / UseOmron 选择协议。")
                };
            });

            // ── Tag 注册表 ────────────────────────────────
            services.AddSingleton<ITagRegistry>(sp =>
            {
                var reg = new TagRegistry();
                foreach (var tag in options.Tags) reg.Register(tag);
                return reg;
            });

            // ── 内存镜像 ──────────────────────────────────
            services.AddSingleton<PlcMemoryMirror>(sp =>
            {
                var mirror = new PlcMemoryMirror();

                // 注册显式配置的区域
                foreach (var r in options.Regions)
                    mirror.RegisterRegion(r.Key, r.Area, r.Start, r.Length);

                // 根据 Tag 自动推断缺少的区域（默认 4096 字节）
                var registry = sp.GetRequiredService<ITagRegistry>();
                foreach (var tag in registry.GetAll())
                {
                    var addr = AddressParser.Parse(tag.Address, tag.Type);
                    try { mirror.RegisterRegion(addr.RegionKey, addr.Area, 0, 4096); }
                    catch { /* 已注册，忽略 */ }
                }
                return mirror;
            });
            services.AddSingleton<IPlcMemoryMirror>(sp => sp.GetRequiredService<PlcMemoryMirror>());
            services.AddSingleton<PlcMirrorOptions>(options);

            // ── 变化通知 ──────────────────────────────────
            services.AddSingleton<ChangeNotifier>();
            services.AddSingleton<IChangeNotifier>(sp => sp.GetRequiredService<ChangeNotifier>());

            // ── 写命令服务 ────────────────────────────────
            services.AddSingleton<IWriteCommandService, WriteCommandService>();

            // ── 同步引擎 ──────────────────────────────────
            //services.AddSingleton(new SyncEngineOptions { ScanInterval = options.ScanInterval });
            services.AddSingleton<PlcSyncEngine>();
            services.AddSingleton<IPlcSyncEngine>(sp => sp.GetRequiredService<PlcSyncEngine>());

            // ── Tag 服务（业务入口）──────────────────────
            services.AddSingleton<TagService>();
            services.AddSingleton<ITagService>(sp => sp.GetRequiredService<TagService>());

            // ── 托管后台服务（自动启动/停止）────────────
            services.AddHostedService<PlcMirrorHostedService>();

            return services;
        }
    }
}

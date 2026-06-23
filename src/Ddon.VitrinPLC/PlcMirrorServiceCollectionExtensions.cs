using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Models;
using Ddon.VitrinPLC.TagEngine;
using Ddon.VitrinPLC.SyncEngine;

namespace Ddon.VitrinPLC
{
    public static class PlcMirrorServiceCollectionExtensions
    {
        /// <summary>
        /// 注册多 PLC 支持。每个 PLC 拥有独立的 SyncEngine、Mirror 和 PlcSession，
        /// 通过 <see cref="IPlcHub"/> 按名称访问。
        ///
        /// 示例：
        /// <code>
        /// services.AddVitrinPlc(builder =>
        /// {
        ///     builder.AddSiemens("main",
        ///         c => { c.Ip = "192.168.1.10"; },
        ///         h => { h.ScanInterval = 200; h.MapTag("Temp", "DB1.DBD0", PlcDataType.Float); });
        ///
        ///     builder.AddMitsubishi("sub",
        ///         c => { c.Ip = "192.168.1.20"; },
        ///         h => { h.MapTag("Speed", "D100", PlcDataType.Int16); });
        ///
        ///     builder.AddClient("custom", new MyPlcClient(), h => { h.MapTag("Valve", "X0", PlcDataType.Bool); });
        ///
        ///     builder.AddClientFactory("custom2", new MyFactory(), h => { h.MapTag("Motor", "Y0", PlcDataType.Bool); });
        /// });
        /// </code>
        /// </summary>
        public static IServiceCollection AddVitrinPlc(
            this IServiceCollection services,
            Action<VitrinPlcBuilder> configure)
        {
            var builder = new VitrinPlcBuilder();
            configure(builder);

            services.AddSingleton<PlcHub>(sp =>
            {
                var logFactory = sp.GetRequiredService<ILoggerFactory>();
                var sessions = new Dictionary<string, IPlcSession>();
                var engines = new List<PlcSyncEngine>();

                foreach (var descriptor in builder.Descriptors)
                {
                    var client = descriptor.ClientFactory(sp);
                    var group = BuildPlcServices(client, descriptor.Options.Tags,
                        descriptor.Options.Regions, descriptor.Options.ScanInterval,
                        descriptor.Options.Endian, logFactory);
                    sessions[descriptor.Name] = group.Session;
                    engines.Add(group.Engine);
                }

                return new PlcHub(sessions, engines);
            });

            services.AddSingleton<IPlcHub>(sp => sp.GetRequiredService<PlcHub>());
            services.AddHostedService<VitrinPlcHostedService>();

            return services;
        }

        private static PlcServiceGroup BuildPlcServices(
            IPlcClient client,
            IReadOnlyList<TagDefinition> tags,
            IReadOnlyList<RegionConfig> regions,
            int scanInterval,
            EndianFormat endian,
            ILoggerFactory loggerFactory)
        {
            var registry = new TagRegistry();
            foreach (var tag in tags)
                registry.Register(tag);

            var mirror = new PlcMemoryMirror(endian);
            foreach (var r in regions)
                mirror.RegisterRegion(r.Key, r.Area, r.Start, r.Length);
            foreach (var tag in registry.GetAll())
            {
                var addr = AddressParser.Parse(tag.Address, tag.Type);
                try { mirror.RegisterRegion(addr.RegionKey, addr.Area, 0, 4096); }
                catch { }
            }

            var notifier = new ChangeNotifier();
            var writeService = new WriteCommandService(client, registry, endian,
                loggerFactory.CreateLogger<WriteCommandService>());

            var engine = new PlcSyncEngine(client, mirror, registry, notifier, scanInterval,
                loggerFactory.CreateLogger<PlcSyncEngine>());

            var session = new PlcSession(registry, mirror, writeService, notifier,
                loggerFactory.CreateLogger<PlcSession>());

            return new PlcServiceGroup(registry, mirror, notifier, writeService, engine, session);
        }

        private sealed record PlcServiceGroup(
            TagRegistry Registry,
            PlcMemoryMirror Mirror,
            ChangeNotifier Notifier,
            WriteCommandService WriteService,
            PlcSyncEngine Engine,
            PlcSession Session);
    }
}

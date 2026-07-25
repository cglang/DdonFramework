using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.SyncEngine;
using Ddon.VitrinPLC.TagEngine;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Ddon.VitrinPLC
{
    internal static class PlcServiceFactory
    {
        internal static PlcServiceGroup Build(
            IPlcClient client,
            PlcHostOptions options,
            IServiceProvider sp)
        {
            var registry = new TagRegistry();
            foreach (var tag in options.Tags)
                registry.Register(tag);

            var mirror = new PlcMemoryMirror(options.Endian);
            foreach (var r in options.Regions)
                mirror.RegisterRegion(r.Key, r.Area, r.Start, r.Length);
            foreach (var tag in registry.GetAll())
            {
                var addr = AddressParser.Parse(tag.Address, tag.Type);
                try { mirror.RegisterRegion(addr.RegionKey, addr.Area, 0, 4096); }
                catch { }
            }

            var notifier = new ChangeNotifier();

            var engine = ActivatorUtilities.CreateInstance<PlcSyncEngine>(sp, client, mirror, registry, notifier, options.ScanInterval);

            var session = ActivatorUtilities.CreateInstance<PlcSession>(sp, registry, mirror, notifier, client, options.Endian);

            return new PlcServiceGroup(registry, mirror, notifier, engine, session);
        }

        internal sealed record PlcServiceGroup(
            TagRegistry Registry,
            PlcMemoryMirror Mirror,
            ChangeNotifier Notifier,
            PlcSyncEngine Engine,
            PlcSession Session);
    }
}

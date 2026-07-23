using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Models;
using Ddon.VitrinPLC.TagEngine;
using Ddon.VitrinPLC.SyncEngine;

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

            var writeService = ActivatorUtilities.CreateInstance<WriteCommandService>(sp, client, registry, options.Endian);

            var engine = ActivatorUtilities.CreateInstance<PlcSyncEngine>(sp, client, mirror, registry, notifier, options.ScanInterval);

            var session = ActivatorUtilities.CreateInstance<PlcSession>(sp, registry, mirror, writeService, notifier);

            return new PlcServiceGroup(registry, mirror, notifier, writeService, engine, session);
        }

        internal sealed record PlcServiceGroup(
            TagRegistry Registry,
            PlcMemoryMirror Mirror,
            ChangeNotifier Notifier,
            WriteCommandService WriteService,
            PlcSyncEngine Engine,
            PlcSession Session);
    }
}

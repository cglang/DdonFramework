using System;
using System.Collections.Generic;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Clients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ddon.VitrinPLC
{
    /// <summary>
    /// 单个 PLC 主机的注册描述符（内部使用）。
    /// </summary>
    internal sealed class PlcHostDescriptor
    {
        public string Name { get; }

        /// <summary>延迟工厂：在 DI 容器构建后调用，以便获取 ILoggerFactory 等服务。</summary>
        public Func<IServiceProvider, IPlcClient> ClientFactory { get; }

        public PlcHostOptions Options { get; }

        public PlcHostDescriptor(string name, Func<IServiceProvider, IPlcClient> factory, PlcHostOptions options)
        {
            Name = name;
            ClientFactory = factory;
            Options = options;
        }
    }

    /// <summary>
    /// 多 PLC 注册 DSL。通过 <c>services.AddVitrinPlc(builder => ...)</c> 使用。
    /// </summary>
    public sealed class VitrinPlcBuilder
    {
        internal List<PlcHostDescriptor> Descriptors { get; } = new();

        // ── 内置协议 ──────────────────────────────────────────────

        public VitrinPlcBuilder AddSiemens(string name,
            Action<SiemensOptions> connection,
            Action<PlcHostOptions> host)
        {
            var connOpts = new SiemensOptions { Name = name };
            connection(connOpts);
            var hostOpts = new PlcHostOptions();
            host(hostOpts);

            Descriptors.Add(new PlcHostDescriptor(name,
                sp => new SiemensClient(connOpts, sp.GetRequiredService<ILoggerFactory>().CreateLogger<SiemensClient>()),
                hostOpts));
            return this;
        }

        public VitrinPlcBuilder AddMitsubishi(string name,
            Action<MitsubishiOptions> connection,
            Action<PlcHostOptions> host)
        {
            var connOpts = new MitsubishiOptions { Name = name };
            connection(connOpts);
            var hostOpts = new PlcHostOptions();
            host(hostOpts);

            Descriptors.Add(new PlcHostDescriptor(name,
                sp => new MitsubishiClient(connOpts, sp.GetRequiredService<ILoggerFactory>().CreateLogger<MitsubishiClient>()),
                hostOpts));
            return this;
        }

        public VitrinPlcBuilder AddOmron(string name,
            Action<OmronOptions> connection,
            Action<PlcHostOptions> host)
        {
            var connOpts = new OmronOptions { Name = name };
            connection(connOpts);
            var hostOpts = new PlcHostOptions();
            host(hostOpts);

            Descriptors.Add(new PlcHostDescriptor(name,
                sp => new OmronClient(connOpts, sp.GetRequiredService<ILoggerFactory>().CreateLogger<OmronClient>()),
                hostOpts));
            return this;
        }

        // ── 外部扩展入口 ──────────────────────────────────────────

        /// <summary>
        /// 注册外部已实例化的 <see cref="IPlcClient"/>。
        /// </summary>
        public VitrinPlcBuilder AddClient(string name,
            IPlcClient client,
            Action<PlcHostOptions> host)
        {
            var hostOpts = new PlcHostOptions();
            host(hostOpts);
            Descriptors.Add(new PlcHostDescriptor(name, _ => client, hostOpts));
            return this;
        }

        /// <summary>注册外部 <see cref="IPlcClientFactory"/> 实现，由框架在初始化时调用 Create。</summary>
        public VitrinPlcBuilder AddClientFactory(string name,
            IPlcClientFactory factory,
            Action<PlcHostOptions> host)
        {
            var hostOpts = new PlcHostOptions();
            host(hostOpts);
            Descriptors.Add(new PlcHostDescriptor(name, _ => factory.Create(name), hostOpts));
            return this;
        }
    }
}

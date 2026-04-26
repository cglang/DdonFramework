using Microsoft.Extensions.DependencyInjection;

namespace Ddon.UniPLC.DependencyInjection;

/// <summary>
/// PLC 框架扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 PLC 框架
    /// </summary>
    /// <example>
    /// services.AddPlc(builder =>
    /// {
    ///     builder.UseSiemens(options =>
    ///     {
    ///         options.Ip = "192.168.1.10";
    ///         options.Port = 102;
    ///         options.Rack = 0;
    ///         options.Slot = 1;
    ///     });
    ///     
    ///     builder.UseMemory("SimPLC");
    /// });
    /// </example>
    public static IServiceCollection AddPlc(
        this IServiceCollection services,
        Action<PlcBuilder>? configure = null)
    {
        var builder = new PlcBuilder(services);
        configure?.Invoke(builder);
        builder.Build();
        return services;
    }
}

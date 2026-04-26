using Ddon.UniPLC.Abstractions;
using Ddon.UniPLC.Clients.Siemens;
using Ddon.UniPLC.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.UniPLC.Examples;

/// <summary>
/// PLC 框架使用示例
/// </summary>
public class PlcFrameworkExample
{
    /// <summary>
    /// 基础使用示例
    /// </summary>
    public static async Task BasicUsageExample()
    {
        // 1. 配置依赖注入
        var services = new ServiceCollection();
        services.AddPlc(builder =>
        {
            // 配置 Siemens PLC
            builder.UseSiemens("MainPLC", options =>
            {
                options.Ip = "192.168.1.10";
                options.Port = 102;
                options.Rack = 0;
                options.Slot = 1;
                options.DbBlockSizes[1] = 256;
                options.DbBlockSizes[2] = 512;
            });

            // 配置内存模拟 PLC
            builder.UseMemory("SimPLC");
        });

        var serviceProvider = services.BuildServiceProvider();

        // 2. 获取 PLC 提供者
        var provider = serviceProvider.GetRequiredService<IPlcProvider>();

        // 3. 使用 Siemens 客户端
        var siemensClient = provider.GetClient("MainPLC");
        await siemensClient.ConnectAsync();

        if (siemensClient.IsConnected)
        {
            // 读取单个值
            try
            {
                var speed = await siemensClient.ReadAsync<float>("DB1.DBD4");
                Console.WriteLine($"速度: {speed}");

                // 写入单个值
                await siemensClient.WriteAsync("DB1.DBD4", 100.5f);

                // 读取字节数组
                var bytesResult = await siemensClient.ReadBytesAsync("DB1.DBX0.0", 10);
                if (bytesResult.IsSuccess && bytesResult.Value != null)
                {
                    Console.WriteLine($"字节数据: {string.Join(",", bytesResult.Value)}");
                }

                // 批量读取
                var batchResults = await siemensClient.BatchReadAsync(
                    "DB1.DBD0",
                    "DB1.DBD4"
                );
                foreach (var result in batchResults)
                {
                    Console.WriteLine($"地址: {result.Address}, 值: {result.Value}");
                }

                // 心跳检测
                var isAlive = await siemensClient.PingAsync();
                Console.WriteLine($"PLC 在线: {isAlive}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"操作错误: {ex.Message}");
            }
            finally
            {
                await siemensClient.DisconnectAsync();
            }
        }

        // 4. 使用模拟 PLC（用于测试）
        var simClient = provider.GetClient("SimPLC");
        await simClient.ConnectAsync();

        // 在模拟 PLC 中写入数据
        await simClient.WriteAsync("TestAddr1", 42);
        await simClient.WriteAsync("TestAddr2", 3.14f);
        await simClient.WriteAsync("TestAddr3", "Hello PLC");

        // 读取数据
        var intValue = await simClient.ReadAsync<int>("TestAddr1");
        var floatValue = await simClient.ReadAsync<float>("TestAddr2");
        var stringValue = await simClient.ReadAsync<string>("TestAddr3");

        Console.WriteLine($"\n模拟 PLC 数据:");
        Console.WriteLine($"整型: {intValue}");
        Console.WriteLine($"浮点: {floatValue}");
        Console.WriteLine($"字符串: {stringValue}");

        await simClient.DisconnectAsync();
    }

    /// <summary>
    /// 错误处理示例
    /// </summary>
    public static async Task ErrorHandlingExample()
    {
        var services = new ServiceCollection();
        services.AddPlc(builder => builder.UseMemory("TestPLC"));

        var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetRequiredService<IPlcProvider>();
        var client = provider.GetClient("TestPLC");

        // 未连接时读取
        try
        {
            await client.ReadAsync<int>("DB1.DBD0");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"未连接错误: {ex.Message}");
        }

        // 连接后读写
        await client.ConnectAsync();

        // 成功的读写
        await client.WriteAsync("Addr1", 123);
        var value = await client.ReadAsync<int>("Addr1");
        Console.WriteLine($"读取成功: {value}");

        // 读取不存在的地址（返回零初始化值）
        var unknownValue = await client.ReadAsync<int>("UnknownAddr");
        Console.WriteLine($"不存在的地址返回: {unknownValue}");

        await client.DisconnectAsync();
    }

    /// <summary>
    /// 地址解析示例
    /// </summary>
    public static void AddressParsingExample()
    {
        // 解析 Siemens 地址
        var addresses = new[]
        {
            "DB1.DBX0.0",    // DB块位寻址
            "DB1.DBW0",      // DB块字寻址
            "DB1.DBD0",      // DB块双字寻址
            "M0.0",          // M区位寻址
            "M0",            // M区字寻址
            "I0.0",          // I区位寻址
            "Q0.0"           // Q区位寻址
        };

        foreach (var address in addresses)
        {
            try
            {
                var plcAddr = SiemensAddressParser.Parse(address);
                Console.WriteLine($"原始地址: {address}");
                Console.WriteLine($"  区域: {plcAddr.Area}");
                Console.WriteLine($"  块号: {plcAddr.BlockNumber}");
                Console.WriteLine($"  偏移: {plcAddr.Offset}");
                Console.WriteLine($"  位: {plcAddr.Bit}");
                Console.WriteLine($"  类型: {plcAddr.DataType}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析失败 {address}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 多客户端管理示例
    /// </summary>
    public static async Task MultiClientExample()
    {
        var services = new ServiceCollection();
        services.AddPlc(builder =>
        {
            builder.UseSiemens("PLC1", options =>
            {
                options.Ip = "192.168.1.10";
                options.Rack = 0;
                options.Slot = 1;
            });

            builder.UseSiemens("PLC2", options =>
            {
                options.Name = "PLC2";
                options.Ip = "192.168.1.20";
                options.Rack = 0;
                options.Slot = 2;
            });

            builder.UseMemory("SimPLC");
        });

        var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetRequiredService<IPlcProvider>();

        // 同时管理多个 PLC 客户端
        var plc1 = provider.GetClient("PLC1");
        var plc2 = provider.GetClient("PLC2");
        var simPlc = provider.GetClient("SimPLC");

        // 连接所有 PLC
        await plc1.ConnectAsync();
        await plc2.ConnectAsync();
        await simPlc.ConnectAsync();

        // 在模拟 PLC 中进行演示
        await simPlc.WriteAsync("Addr1", 100);
        var value = await simPlc.ReadAsync<int>("Addr1");

        Console.WriteLine($"模拟 PLC 值: {value}");
        Console.WriteLine($"PLC1 连接状态: {plc1.IsConnected}");
        Console.WriteLine($"PLC2 连接状态: {plc2.IsConnected}");

        // 断开连接
        await plc1.DisconnectAsync();
        await plc2.DisconnectAsync();
        await simPlc.DisconnectAsync();
    }
}

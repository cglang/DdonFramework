# Ddon.UniPLC - 通用 PLC 通信框架

## 概述

Ddon.UniPLC 是一个可扩展的 PLC 通信框架，统一接入不同品牌 PLC（如西门子 Siemens、三菱 Mitsubishi、欧姆龙 Omron 等）。通过统一 API 实现 PLC 的连接管理、数据读写、批量操作等功能。

## 核心特性

### 第一阶段（当前版本）✅

- ✅ 统一的 IPlcClient 接口
- ✅ IPlcProvider 客户端管理
- ✅ Siemens PLC 客户端（基础实现）
- ✅ 内存模拟 PLC 客户端
- ✅ 类似 EFCore 的依赖注入注册方式
- ✅ 基础的读写和批量操作

### 第二阶段（计划中）

- 🔄 地址解析器
- 🔄 数据类型序列化
- 🔄 高级对象映射（ORM 风格）

### 第三阶段（计划中）

- 🔄 自动重连机制
- 🔄 值变化订阅
- 🔄 批量读取优化
- 🔄 插件扩展

### 第四阶段（计划中）

- 🔄 Mitsubishi 客户端
- 🔄 Omron 客户端
- 🔄 Modbus TCP 支持

## 安装

通过 NuGet 添加 Ddon.UniPLC 包：

```bash
dotnet add package Ddon.UniPLC
```

## 快速开始

### 1. 配置依赖注入

```csharp
using Ddon.UniPLC.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// 配置 PLC 框架
services.AddPlc(builder =>
{
    // 配置 Siemens PLC
    builder.UseSiemens(options =>
    {
        options.Ip = "192.168.1.10";
        options.Port = 102;
        options.Rack = 0;
        options.Slot = 1;
    });

    // 配置内存模拟 PLC（用于测试）
    builder.UseMemory("SimPLC");
});

var serviceProvider = services.BuildServiceProvider();
```

### 2. 使用 PLC 客户端

```csharp
using Ddon.UniPLC.Abstractions;

var provider = serviceProvider.GetRequiredService<IPlcProvider>();

// 获取 Siemens 客户端
var siemensClient = provider.GetClient("Siemens");

// 连接到 PLC
await siemensClient.ConnectAsync();

try
{
    // 检查连接状态
    if (siemensClient.IsConnected)
    {
        // 读取单个值
        var speed = await siemensClient.ReadAsync<float>("DB1.DBD4");
        Console.WriteLine($"Speed: {speed}");

        // 写入单个值
        await siemensClient.WriteAsync("DB1.DBD4", 100.5f);

        // 读取字节数组
        var result = await siemensClient.ReadBytesAsync("DB1.DBX0.0", 10);
        if (result.IsSuccess)
        {
            Console.WriteLine($"Bytes: {string.Join(",", result.Value)}");
        }

        // 批量读取
        var batchResult = await siemensClient.BatchReadAsync("DB1.DBD0", "DB1.DBD4");
        foreach (var item in batchResult)
        {
            Console.WriteLine($"Address: {item.Address}, Value: {item.Value}");
        }

        // 心跳检测
        var isAlive = await siemensClient.PingAsync();
        Console.WriteLine($"PLC is alive: {isAlive}");
    }
}
finally
{
    // 断开连接
    await siemensClient.DisconnectAsync();
}
```

## 地址格式

### Siemens 地址格式

| 类型 | 格式 | 示例 | 说明 |
|------|------|------|------|
| DB块位寻址 | DB块号.DBX字节.位 | DB1.DBX0.0 | DB1块第0字节第0位 |
| DB块字寻址 | DB块号.DBW字节 | DB1.DBW0 | DB1块第0字节（无符号短整型） |
| DB块双字寻址 | DB块号.DBD字节 | DB1.DBD0 | DB1块第0字节（整型） |
| M区位寻址 | M字节.位 | M0.0 | M区第0字节第0位 |
| M区字寻址 | M字节 | M0 | M区第0字节（无符号字节） |
| I区位寻址 | I字节.位 | I0.0 | I区第0字节第0位 |
| Q区位寻址 | Q字节.位 | Q0.0 | Q区第0字节第0位 |

## 数据类型支持

当前支持以下数据类型：

- Bool（布尔值）
- Byte（字节）
- Short（短整型）
- UShort（无符号短整型）
- Int（整型）
- UInt（无符号整型）
- Long（长整型）
- ULong（无符号长整型）
- Float（单精度浮点）
- Double（双精度浮点）
- String（字符串）
- DateTime（日期时间）
- Struct（结构体）
- Array（数组）
- Enum（枚举）

## 异常处理

框架提供了详细的异常体系：

```csharp
try
{
    await client.ReadAsync<int>("DB1.DBD0");
}
catch (PlcConnectionException ex)
{
    Console.WriteLine($"连接错误: {ex.Message}");
}
catch (PlcTimeoutException ex)
{
    Console.WriteLine($"操作超时: {ex.Message}");
}
catch (PlcAddressException ex)
{
    Console.WriteLine($"地址错误: {ex.Message}");
}
catch (PlcProtocolException ex)
{
    Console.WriteLine($"协议错误: {ex.Message}");
}
catch (PlcSerializationException ex)
{
    Console.WriteLine($"序列化错误: {ex.Message}");
}
catch (PlcException ex)
{
    Console.WriteLine($"PLC 错误: {ex.Message}");
}
```

## 完整示例

```csharp
using Ddon.UniPLC.Abstractions;
using Ddon.UniPLC.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

// 设置依赖注入
var services = new ServiceCollection();
services.AddPlc(builder =>
{
    builder.UseMemory("TestPLC");
});

var serviceProvider = services.BuildServiceProvider();
var provider = serviceProvider.GetRequiredService<IPlcProvider>();

// 获取测试 PLC 客户端
var client = provider.GetClient("TestPLC");

// 连接
await client.ConnectAsync();

// 写入测试数据
await client.WriteAsync("TestAddr1", 42);
await client.WriteAsync("TestAddr2", 3.14f);
await client.WriteAsync("TestAddr3", "Hello PLC");

// 读取数据
var intValue = await client.ReadAsync<int>("TestAddr1");
var floatValue = await client.ReadAsync<float>("TestAddr2");
var stringValue = await client.ReadAsync<string>("TestAddr3");

Console.WriteLine($"Int: {intValue}");
Console.WriteLine($"Float: {floatValue}");
Console.WriteLine($"String: {stringValue}");

// 批量读取
var results = await client.BatchReadAsync("TestAddr1", "TestAddr2", "TestAddr3");
foreach (var result in results)
{
    Console.WriteLine($"{result.Address}: {result.Value}");
}

// 断开连接
await client.DisconnectAsync();
```

## 项目结构

```
Ddon.UniPLC/
├── Abstractions/           # 核心接口
│   ├── IPlcClient.cs
│   ├── IPlcProvider.cs
│   └── IPlcClientFactory.cs
├── Models/                 # 数据模型
│   ├── PlcAddress.cs
│   ├── PlcOptions.cs
│   ├── PlcReadResult.cs
│   ├── PlcWriteResult.cs
│   └── PlcValueResult.cs
├── Exceptions/             # 异常体系
│   └── PlcException.cs
├── Clients/                # PLC 客户端实现
│   ├── MemoryPlcClient.cs
│   └── Siemens/
│       ├── SiemensPlcClient.cs
│       ├── SiemensPlcOptions.cs
│       ├── SiemensAddressParser.cs
│       └── SiemensPlcClientFactory.cs
├── Core/                   # 核心实现
│   ├── PlcProvider.cs
│   └── PlcClientFactoryRegistry.cs
└── DependencyInjection/    # DI 扩展
    ├── PlcBuilder.cs
    └── ServiceCollectionExtensions.cs
```

## 设计原则

### 1. 抽象统一（Abstraction）

通过 `IPlcClient` 接口屏蔽不同 PLC 的实现差异，提供统一的 API。

### 2. 可扩展（Extensible）

新增 PLC 品牌只需实现 `IPlcClient` 和 `IPlcClientFactory` 接口，无需修改现有代码。

### 3. 高性能（High Performance）

- 支持异步 I/O
- 内存池管理
- 批量读取优化（计划中）

### 4. 高可靠（Reliable）

- 详细的异常体系
- 连接状态管理
- 重连机制（计划中）

## 下一步计划

### Phase 2: 地址解析与数据映射

- 通用地址解析器
- 类型序列化框架
- 高级对象映射（如 ORM）

### Phase 3: 稳定性增强

- 自动重连
- 心跳检测
- 值变化订阅
- 断路器模式

### Phase 4: 多品牌支持

- Mitsubishi 客户端
- Omron 客户端
- Modbus TCP/RTU

## 贡献指南

欢迎提交 Issue 和 Pull Request！

## 许可证

MIT

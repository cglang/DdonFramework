# Ddon.UniPLC API 参考

## 核心接口

### IPlcClient

PLC 客户端的基础接口，所有 PLC 客户端实现都必须实现此接口。

```csharp
public interface IPlcClient : IAsyncDisposable
{
    string Name { get; }
    bool IsConnected { get; }

    Task ConnectAsync();
    Task DisconnectAsync();
    Task<bool> PingAsync();

    Task<PlcReadResult<byte[]>> ReadBytesAsync(string address, int length);
    Task<PlcWriteResult> WriteBytesAsync(string address, byte[] data);

    Task<T> ReadAsync<T>(string address);
    Task WriteAsync<T>(string address, T value);

    Task<IReadOnlyList<PlcValueResult>> BatchReadAsync(params string[] addresses);
}
```

#### 方法说明

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `ConnectAsync()` | `Task` | 建立连接到 PLC |
| `DisconnectAsync()` | `Task` | 断开与 PLC 的连接 |
| `PingAsync()` | `Task<bool>` | 检测 PLC 是否在线 |
| `ReadBytesAsync()` | `Task<PlcReadResult<byte[]>>` | 读取原始字节数据 |
| `WriteBytesAsync()` | `Task<PlcWriteResult>` | 写入字节数据 |
| `ReadAsync<T>()` | `Task<T>` | 泛型读取（自动类型转换） |
| `WriteAsync<T>()` | `Task` | 泛型写入（自动类型转换） |
| `BatchReadAsync()` | `Task<IReadOnlyList<PlcValueResult>>` | 批量读取多个地址 |

---

### IPlcProvider

PLC 客户端的提供者和管理器。

```csharp
public interface IPlcProvider
{
    IPlcClient GetClient(string name);
    T GetClient<T>() where T : IPlcClient;
}
```

#### 方法说明

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `GetClient(string name)` | `IPlcClient` | 按名称获取 PLC 客户端 |
| `GetClient<T>()` | `T` | 按类型获取 PLC 客户端 |

#### 异常

| 异常 | 条件 |
|------|------|
| `KeyNotFoundException` | 指定名称或类型的客户端不存在 |

---

### IPlcClientFactory

工厂接口，用于创建 PLC 客户端。

```csharp
public interface IPlcClientFactory
{
    IPlcClient Create(PlcOptions options);
}
```

---

## 数据模型

### PlcAddress

PLC 地址统一模型，用于解析和存储 PLC 地址信息。

```csharp
public class PlcAddress
{
    public string Area { get; set; }              // 地址区域（DB, M, I, Q等）
    public int BlockNumber { get; set; }          // 块号
    public int Offset { get; set; }               // 字节偏移量
    public int Bit { get; set; }                  // 位偏移量（-1表示无）
    public PlcDataType DataType { get; set; }     // 数据类型
    public string RawAddress { get; set; }        // 原始地址字符串

    public override string ToString() { ... }     // 获取规范化地址
}
```

#### PlcDataType 枚举

```csharp
public enum PlcDataType
{
    Bool,       // 布尔值
    Byte,       // 字节
    Short,      // 短整型
    UShort,     // 无符号短整型
    Int,        // 整型
    UInt,       // 无符号整型
    Long,       // 长整型
    ULong,      // 无符号长整型
    Float,      // 单精度浮点
    Double,     // 双精度浮点
    String,     // 字符串
    DateTime,   // 日期时间
    Struct,     // 结构体
    Array,      // 数组
    Enum        // 枚举
}
```

---

### PlcReadResult<T>

读取操作的结果模型。

```csharp
public class PlcReadResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }

    // 工厂方法
    public static PlcReadResult<T> Success(T value) { ... }
    public static PlcReadResult<T> Failure(string message, Exception? ex = null) { ... }
}
```

#### 使用示例

```csharp
var result = await client.ReadBytesAsync("DB1.DBD0", 4);
if (result.IsSuccess)
{
    var data = result.Value;
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
}
```

---

### PlcWriteResult

写入操作的结果模型。

```csharp
public class PlcWriteResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }

    // 工厂方法
    public static PlcWriteResult Success() { ... }
    public static PlcWriteResult Failure(string message, Exception? ex = null) { ... }
}
```

---

### PlcValueResult

批量读取操作中单个地址的结果。

```csharp
public class PlcValueResult
{
    public string Address { get; set; }          // 地址
    public bool IsSuccess { get; set; }          // 是否成功
    public object? Value { get; set; }           // 值
    public string? ErrorMessage { get; set; }    // 错误信息
    public Exception? Exception { get; set; }    // 异常
}
```

---

### PlcOptions

基础 PLC 配置选项。

```csharp
public class PlcOptions
{
    public string Name { get; set; } = "Default";
    public string Type { get; set; } = "Siemens";
    public string Ip { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 102;
    public int ConnectTimeout { get; set; } = 5000;
    public int OperationTimeout { get; set; } = 5000;
    public int ReconnectInterval { get; set; } = 3;
    public Dictionary<string, object> ExtendedOptions { get; set; } = new();
}
```

---

## Siemens 特定 API

### SiemensPlcOptions

Siemens PLC 特定的配置选项。

```csharp
public class SiemensPlcOptions : PlcOptions
{
    public int Rack { get; set; } = 0;
    public int Slot { get; set; } = 1;
    public Dictionary<int, int> DbBlockSizes { get; set; } = new();

    public SiemensPlcOptions()
    {
        Type = "Siemens";
        Port = 102;
    }
}
```

#### 配置示例

```csharp
var options = new SiemensPlcOptions
{
    Ip = "192.168.1.10",
    Port = 102,
    Rack = 0,
    Slot = 1
};

// 配置 DB 块大小
options.DbBlockSizes[1] = 256;
options.DbBlockSizes[2] = 512;
```

---

### SiemensAddressParser

Siemens 地址解析器。

```csharp
public class SiemensAddressParser
{
    public static PlcAddress Parse(string address) { ... }
}
```

#### 支持的地址格式

| 格式 | 示例 | 说明 |
|------|------|------|
| DB块位寻址 | `DB1.DBX0.0` | 第1块，第0字节，第0位 |
| DB块字寻址 | `DB1.DBW0` | 第1块，第0字节（UShort） |
| DB块双字寻址 | `DB1.DBD0` | 第1块，第0字节（Int） |
| M区位寻址 | `M0.0` | M区第0字节第0位 |
| M区字寻址 | `M0` | M区第0字节 |
| I区位寻址 | `I0.0` | I区第0字节第0位 |
| Q区位寻址 | `Q0.0` | Q区第0字节第0位 |

#### 使用示例

```csharp
try
{
    var address = SiemensAddressParser.Parse("DB1.DBD4");
    Console.WriteLine($"Area: {address.Area}");           // "DB"
    Console.WriteLine($"Block: {address.BlockNumber}");   // 1
    Console.WriteLine($"Offset: {address.Offset}");       // 4
    Console.WriteLine($"Type: {address.DataType}");       // PlcDataType.Int
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Invalid address: {ex.Message}");
}
```

---

## 客户端实现

### MemoryPlcClient

内存模拟 PLC 客户端，用于测试和离线开发。

```csharp
public class MemoryPlcClient : IPlcClient
{
    public MemoryPlcClient(PlcOptions options) { ... }

    // 所有接口方法实现
}
```

#### 特性

- 所有数据存储在内存中
- 无需真实 PLC 连接
- 适合单元测试
- 适合 UI 原型设计

#### 使用示例

```csharp
var options = new PlcOptions { Name = "TestPLC" };
var client = new MemoryPlcClient(options);

await client.ConnectAsync();
await client.WriteAsync("Addr1", 42);
var value = await client.ReadAsync<int>("Addr1");
await client.DisconnectAsync();
```

---

### SiemensPlcClient

Siemens PLC 客户端实现（当前为基础框架）。

```csharp
public class SiemensPlcClient : IPlcClient
{
    public SiemensPlcClient(SiemensPlcOptions options) { ... }

    // 所有接口方法实现
}
```

#### 特性

- 支持 Siemens S7 地址格式
- 地址自动解析和验证
- 线程安全的连接管理
- DB 块大小配置支持

---

## 依赖注入

### ServiceCollectionExtensions

DI 扩展方法。

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlc(
        this IServiceCollection services,
        Action<PlcBuilder>? configure = null);
}
```

#### 使用示例

```csharp
services.AddPlc(builder =>
{
    builder.UseSiemens(options =>
    {
        options.Ip = "192.168.1.10";
        options.Port = 102;
    });

    builder.UseMemory("TestPLC");
});
```

---

### PlcBuilder

PLC 框架配置构建器。

```csharp
public class PlcBuilder
{
    public PlcBuilder UseSiemens(string name, Action<SiemensPlcOptions> configure);
    public PlcBuilder UseSiemens(Action<SiemensPlcOptions> configure);
    public PlcBuilder UseMemory(string name = "Memory");
    public void Build();
}
```

#### 方法说明

| 方法 | 参数 | 说明 |
|------|------|------|
| `UseSiemens()` | `Action<SiemensPlcOptions>` | 配置 Siemens PLC（默认名称） |
| `UseSiemens()` | `string, Action<SiemensPlcOptions>` | 配置指定名称的 Siemens PLC |
| `UseMemory()` | `string?` | 配置内存模拟 PLC |
| `Build()` | - | 构建并注册所有 PLC 客户端 |

---

## 异常处理

### 异常层次结构

```
Exception
  └── PlcException (基础异常)
      ├── PlcConnectionException (连接异常)
      ├── PlcTimeoutException (超时异常)
      ├── PlcAddressException (地址异常)
      ├── PlcProtocolException (协议异常)
      └── PlcSerializationException (序列化异常)
```

### 异常详细说明

```csharp
// 基础异常
public class PlcException : Exception
{
    public PlcException(string message) { ... }
    public PlcException(string message, Exception innerException) { ... }
}

// 连接异常
public class PlcConnectionException : PlcException { ... }

// 超时异常
public class PlcTimeoutException : PlcException { ... }

// 地址异常
public class PlcAddressException : PlcException { ... }

// 协议异常
public class PlcProtocolException : PlcException { ... }

// 序列化异常
public class PlcSerializationException : PlcException { ... }
```

### 异常处理示例

```csharp
try
{
    await client.ReadAsync<int>("DB1.DBD0");
}
catch (PlcConnectionException ex)
{
    // 处理连接错误
    logger.LogError($"Connection error: {ex.Message}");
}
catch (PlcTimeoutException ex)
{
    // 处理超时错误
    logger.LogError($"Operation timeout: {ex.Message}");
}
catch (PlcAddressException ex)
{
    // 处理地址错误
    logger.LogError($"Invalid address: {ex.Message}");
}
catch (PlcException ex)
{
    // 处理其他 PLC 错误
    logger.LogError($"PLC error: {ex.Message}");
}
```

---

## 常见用法示例

### 连接和基本操作

```csharp
var provider = serviceProvider.GetRequiredService<IPlcProvider>();
var client = provider.GetClient("Siemens");

// 连接
await client.ConnectAsync();

try
{
    // 检查连接状态
    if (!client.IsConnected)
    {
        throw new InvalidOperationException("Not connected");
    }

    // 读取整型值
    int value = await client.ReadAsync<int>("DB1.DBD0");
    Console.WriteLine($"Read value: {value}");

    // 写入浮点值
    await client.WriteAsync("DB1.DBD4", 3.14f);

    // 读取字节数组
    var result = await client.ReadBytesAsync("DB1.DBX0.0", 10);
    if (result.IsSuccess && result.Value != null)
    {
        Console.WriteLine($"Bytes: {string.Join(",", result.Value)}");
    }
}
finally
{
    // 断开连接
    await client.DisconnectAsync();
}
```

### 批量读取

```csharp
var addresses = new[] 
{ 
    "DB1.DBD0",  // 整型
    "DB1.DBD4",  // 浮点型
    "M0.0"       // 布尔值
};

var results = await client.BatchReadAsync(addresses);

foreach (var result in results)
{
    if (result.IsSuccess)
    {
        Console.WriteLine($"{result.Address}: {result.Value}");
    }
    else
    {
        Console.WriteLine($"{result.Address}: Error - {result.ErrorMessage}");
    }
}
```

### 使用内存 PLC 进行测试

```csharp
[TestMethod]
public async Task TestMotorControl()
{
    var provider = serviceProvider.GetRequiredService<IPlcProvider>();
    var client = provider.GetClient("TestPLC");

    await client.ConnectAsync();

    // 设置初始值
    await client.WriteAsync("Speed", 0);

    // 模拟加速
    for (int i = 0; i <= 100; i += 10)
    {
        await client.WriteAsync("Speed", i);
        var speed = await client.ReadAsync<int>("Speed");
        Assert.AreEqual(i, speed);
    }

    await client.DisconnectAsync();
}
```

### 多 PLC 管理

```csharp
var plc1 = provider.GetClient("PLC1");
var plc2 = provider.GetClient("PLC2");

// 并行连接
await Task.WhenAll(
    plc1.ConnectAsync(),
    plc2.ConnectAsync()
);

// 并行读取
var (value1, value2) = await Task.WhenAll(
    plc1.ReadAsync<int>("DB1.DBD0"),
    plc2.ReadAsync<int>("DB1.DBD0")
).ContinueWith(t => (t.Result[0], (int)t.Result[1]));

// 并行断开
await Task.WhenAll(
    plc1.DisconnectAsync(),
    plc2.DisconnectAsync()
);
```

---

## 版本历史

### v1.0.0 (当前版本)
- ✅ 核心接口设计
- ✅ MemoryPlcClient 实现
- ✅ SiemensPlcClient 基础实现
- ✅ 依赖注入支持
- ✅ 24个单元测试

### v1.1.0 (计划中)
- [ ] 地址解析增强
- [ ] 复杂数据类型支持
- [ ] 对象映射框架

### v2.0.0 (计划中)
- [ ] 自动重连
- [ ] 值变化订阅
- [ ] Mitsubishi/Omron 支持
- [ ] 性能优化

---

## 相关文档

- [README.md](README.md) - 项目概览和快速开始
- [IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md) - 实现总结
- [示例代码](Examples/PlcFrameworkExample.cs) - 详细使用示例


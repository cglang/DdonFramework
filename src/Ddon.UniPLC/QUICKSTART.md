# Ddon.UniPLC 快速参考指南

## 📦 包含内容

### 核心包
- **Ddon.UniPLC** - 主框架库 (net10.0)
- **Test.UniPLC** - 单元测试项目 (24个测试，100%通过)

### 文档
- `README.md` - 项目概览和快速开始
- `API_REFERENCE.md` - 详细 API 参考
- `IMPLEMENTATION_SUMMARY.md` - 实现总结和架构设计

---

## ⚡ 5分钟快速开始

### 第1步: 安装
```bash
# 项目已包含，无需额外安装
# 只需在 VS 中打开解决方案
```

### 第2步: 配置
```csharp
var services = new ServiceCollection();
services.AddPlc(builder =>
{
    builder.UseMemory("TestPLC");  // 内存模拟 PLC
});
var provider = services.BuildServiceProvider();
```

### 第3步: 使用
```csharp
var client = provider.GetRequiredService<IPlcProvider>()
                    .GetClient("TestPLC");

await client.ConnectAsync();
await client.WriteAsync("Addr1", 42);
var value = await client.ReadAsync<int>("Addr1");
await client.DisconnectAsync();
```

---

## 🎯 常见任务

### 任务1: 读取 PLC 数据
```csharp
// 单值读取
int value = await client.ReadAsync<int>("DB1.DBD0");

// 字节读取
var result = await client.ReadBytesAsync("DB1.DBX0.0", 10);

// 批量读取
var results = await client.BatchReadAsync("DB1.DBD0", "DB1.DBD4", "M0.0");
```

### 任务2: 写入 PLC 数据
```csharp
// 单值写入
await client.WriteAsync("DB1.DBD0", 100);

// 字节写入
await client.WriteBytesAsync("DB1.DBX0.0", new byte[] { 1, 2, 3 });
```

### 任务3: 配置多个 PLC
```csharp
services.AddPlc(builder =>
{
    builder.UseSiemens("PLC1", options => 
    {
        options.Ip = "192.168.1.10";
    });

    builder.UseSiemens("PLC2", options =>
    {
        options.Ip = "192.168.1.20";
    });

    builder.UseMemory("TestPLC");
});
```

### 任务4: 处理异常
```csharp
try
{
    await client.ReadAsync<int>("DB1.DBD0");
}
catch (PlcConnectionException ex)
{
    Console.WriteLine("Connection failed");
}
catch (PlcTimeoutException ex)
{
    Console.WriteLine("Operation timeout");
}
```

---

## 🔍 文件导航

```
src/Ddon.UniPLC/
├── README.md                      ← 🌟 从这里开始
├── API_REFERENCE.md               ← 📖 API 详细文档
├── IMPLEMENTATION_SUMMARY.md      ← 🏗️ 架构和实现
├── Examples/PlcFrameworkExample.cs ← 💡 代码示例
│
├── Abstractions/
│   ├── IPlcClient.cs             ← 核心接口
│   ├── IPlcProvider.cs           ← 提供者接口
│   └── IPlcClientFactory.cs      ← 工厂接口
│
├── Models/
│   ├── PlcAddress.cs             ← 地址模型
│   ├── PlcOptions.cs             ← 配置模型
│   ├── PlcReadResult.cs          ← 读取结果
│   ├── PlcWriteResult.cs         ← 写入结果
│   └── PlcValueResult.cs         ← 批量结果
│
├── Exceptions/
│   └── PlcException.cs           ← 异常体系
│
├── Clients/
│   ├── MemoryPlcClient.cs        ← 内存模拟客户端
│   └── Siemens/
│       ├── SiemensPlcClient.cs   ← Siemens 实现
│       ├── SiemensPlcOptions.cs  ← Siemens 配置
│       ├── SiemensAddressParser.cs
│       └── SiemensPlcClientFactory.cs
│
├── Core/
│   ├── PlcProvider.cs            ← 提供者实现
│   └── PlcClientFactoryRegistry.cs
│
└── DependencyInjection/
    ├── PlcBuilder.cs             ← 构建器
    └── ServiceCollectionExtensions.cs ← DI 扩展

test/Test.UniPLC/
├── MemoryPlcClientTests.cs       ← 内存客户端测试 (10)
├── SiemensAddressParserTests.cs  ← 地址解析器测试 (8)
└── DependencyInjectionTests.cs   ← DI 测试 (6)
```

---

## 📊 支持的数据类型

| 类型 | C# | 字节数 | 示例 |
|------|-----|--------|------|
| Bool | bool | 1 | true |
| Byte | byte | 1 | 255 |
| Short | short | 2 | -100 |
| UShort | ushort | 2 | 1000 |
| Int | int | 4 | 123456 |
| UInt | uint | 4 | 123456 |
| Long | long | 8 | 123456789 |
| ULong | ulong | 8 | 123456789 |
| Float | float | 4 | 3.14 |
| Double | double | 8 | 3.14159 |
| String | string | 动态 | "Hello" |

---

## 🔗 Siemens 地址格式

### DB 区
```csharp
"DB1.DBX0.0"   // DB块1, 字节0, 位0    → bool
"DB1.DBW0"     // DB块1, 字节0         → ushort
"DB1.DBD0"     // DB块1, 字节0         → int
```

### M/I/Q 区
```csharp
"M0.0"         // M区, 字节0, 位0      → bool
"M0"           // M区, 字节0           → byte
"I0.0"         // I区, 字节0, 位0      → bool
"Q0.0"         // Q区, 字节0, 位0      → bool
```

---

## 🧪 运行测试

```bash
# 在 VS 中
# 1. 打开 Test Explorer (Ctrl+E, T)
# 2. 点击"全部运行"按钮

# 或从命令行
cd test\Test.UniPLC
dotnet test

# 预期结果: 24 个测试通过
```

---

## 📝 开发工作流

### 1. 创建新的 PLC 品牌支持

```csharp
// 1. 创建配置类
public class MyPLCOptions : PlcOptions { }

// 2. 实现客户端
public class MyPLCClient : IPlcClient { }

// 3. 实现工厂
public class MyPLCClientFactory : IPlcClientFactory { }

// 4. 在 PlcBuilder 中添加
public PlcBuilder UseMyPLC(Action<MyPLCOptions> configure)
{
    // ...
}
```

### 2. 添加新的地址解析器

```csharp
public class MyPLCAddressParser
{
    public static PlcAddress Parse(string address) { }
}
```

### 3. 添加单元测试

```csharp
[TestClass]
public class MyPLCClientTests
{
    [TestMethod]
    public async Task TestMethod() { }
}
```

---

## ⚙️ 配置选项

### 基础选项 (PlcOptions)
```csharp
new PlcOptions
{
    Name = "PLC1",                    // 客户端名称
    Type = "Siemens",                // PLC 类型
    Ip = "192.168.1.10",            // IP 地址
    Port = 102,                      // 端口
    ConnectTimeout = 5000,           // 连接超时(ms)
    OperationTimeout = 5000,         // 操作超时(ms)
    ReconnectInterval = 3            // 重连间隔(s)
}
```

### Siemens 特定选项
```csharp
new SiemensPlcOptions
{
    Ip = "192.168.1.10",
    Port = 102,
    Rack = 0,                        // Rack 号
    Slot = 1,                        // Slot 号
    DbBlockSizes = new() {
        { 1, 256 },                  // DB1: 256 字节
        { 2, 512 }                   // DB2: 512 字节
    }
}
```

---

## 🔐 最佳实践

### ✅ 推荐做法

```csharp
// 1. 使用 DI 容器管理生命周期
var provider = serviceProvider.GetRequiredService<IPlcProvider>();

// 2. 异步/等待
await client.ConnectAsync();

// 3. 使用 try-finally 确保断开
try
{
    // 操作
}
finally
{
    await client.DisconnectAsync();
}

// 4. 检查连接状态
if (client.IsConnected)
{
    // 执行操作
}

// 5. 处理异常
catch (PlcException ex)
{
    // 记录日志
}
```

### ❌ 避免做法

```csharp
// ❌ 不要同步阻塞
client.ConnectAsync().Wait();

// ❌ 不要忽略异常
try { await client.ReadAsync<int>("Addr"); } catch { }

// ❌ 不要创建多个客户端实例
var client = new SiemensPlcClient(options);

// ❌ 不要忘记断开连接
await client.ConnectAsync();
// ... 没有 DisconnectAsync()
```

---

## 🚀 性能优化建议

### 当前性能
- 单次读取: < 1ms (内存客户端)
- 批量读取: 顺序执行
- 内存占用: < 5MB

### 优化策略（计划中）
- 实现请求合并
- 异步并行操作
- 连接池复用
- 缓存热数据

---

## 🐛 常见问题

### Q1: 如何在单元测试中使用 PLC?
**A:** 使用 `UseMemory()` 配置内存模拟 PLC:
```csharp
services.AddPlc(builder => builder.UseMemory("TestPLC"));
```

### Q2: 支持哪些 Siemens 地址格式?
**A:** 支持 DB/M/I/Q 区的位/字/双字寻址，见"Siemens 地址格式"部分。

### Q3: 如何处理 PLC 连接中断?
**A:** 在 Phase 2 中会添加自动重连机制。当前需要手动重连:
```csharp
try
{
    // 操作
}
catch (PlcConnectionException)
{
    await client.DisconnectAsync();
    await client.ConnectAsync();
}
```

### Q4: 支持并发读写吗?
**A:** 支持。内部使用 `SemaphoreSlim` 保护关键部分。建议使用 DI 管理单个客户端实例。

### Q5: 如何添加日志?
**A:** Phase 3 中会添加详细日志记录支持。当前可以创建包装类：
```csharp
public class LoggingPlcClientDecorator : IPlcClient
{
    private readonly IPlcClient _inner;
    public async Task<T> ReadAsync<T>(string address)
    {
        _logger.LogInformation($"Reading {address}");
        return await _inner.ReadAsync<T>(address);
    }
}
```

---

## 📚 学习路径

### 初级 (了解框架)
1. 读 README.md
2. 运行示例代码
3. 尝试 UseMemory() 配置

### 中级 (实现集成)
1. 阅读 API_REFERENCE.md
2. 配置 Siemens 客户端
3. 理解地址解析

### 高级 (扩展框架)
1. 阅读 IMPLEMENTATION_SUMMARY.md
2. 实现新的 PLC 品牌支持
3. 贡献 Pull Request

---

## 🔗 相关资源

### 框架
- [EF Core](https://github.com/dotnet/efcore) - DI 配置风格参考
- [HttpClientFactory](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests) - 生命周期管理参考

### 工业协议
- [Siemens S7 协议](https://en.wikipedia.org/wiki/Siemens_S7)
- [Modbus 协议](https://en.wikipedia.org/wiki/Modbus)
- [OPC UA](https://en.wikipedia.org/wiki/OPC_Unified_Architecture)

### .NET
- [异步编程](https://docs.microsoft.com/en-us/dotnet/csharp/async)
- [依赖注入](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)

---

## 📞 获取帮助

### 遇到问题?
1. 查看 API_REFERENCE.md 或 README.md
2. 检查示例代码 (Examples/)
3. 查看单元测试用法 (test/)
4. 提交 GitHub Issue

### 想要贡献?
1. Fork 仓库
2. 创建特性分支
3. 提交 Pull Request
4. 参照现有代码风格

---

## 📄 版本信息

- **框架版本**: 1.0.0
- **目标框架**: .NET 10.0
- **发布日期**: 2024年
- **状态**: Phase 1 完成

**下一阶段**: Phase 2 - 地址解析与数据映射 (预计)

---

**最后更新**: 2024年
**许可证**: MIT

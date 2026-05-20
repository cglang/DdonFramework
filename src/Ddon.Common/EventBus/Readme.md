# GeneralEventBus

一个轻量、线程安全、零 UI 框架依赖的 .NET 发布/订阅事件总线。

支持从任意线程发布事件，订阅者可自由选择回调线程（UI 线程、线程池、发布者线程或任意自定义调度器）。

---

## 特性

- **零依赖**：仅依赖 .NET 标准库，不引用 WinForms / WPF / MAUI 等 UI 框架
- **任意线程发布**：`Publish<T>` 可在后台线程、UI 线程、线程池任意位置调用
- **灵活调度**：内置 4 种调度器，也可自实现 `IEventScheduler` 接口
- **自动线程切换**：UI 订阅者通过 `SyncContextScheduler` 自动切换回 UI 线程
- **类型安全**：事件以 C# 类型为频道，编译期即可发现类型不匹配问题
- **异常隔离**：单个订阅者抛出异常不影响其他订阅者
- **防内存泄漏**：`Subscribe` 返回 `IDisposable`，`Dispose` 即取消订阅，幂等安全
- **异步支持**：`PublishAsync<T>` 返回 `Task`，可 `await` 等待所有回调执行完毕

---

## 快速开始

### 1. 定义事件

事件是普通的 C# 类或 record，类型本身即为"频道"。

```csharp
public record SensorDataEvent(string SensorId, double Value, DateTime Timestamp);
public record AlertEvent(string Message, AlertLevel Level);

public enum AlertLevel { Info, Warning, Critical }
```

### 2. 订阅事件

在需要接收数据的地方注册订阅，选择合适的调度器。

```csharp
// 后台订阅者：在发布者线程直接回调（默认行为，scheduler 传 null 即可）
var sub = GeneralEventBus.Default.Subscribe<SensorDataEvent>(e =>
{
    Console.WriteLine($"[{e.SensorId}] {e.Value}°C @ {e.Timestamp}");
});

// UI 订阅者：必须在 UI 线程上捕获上下文，之后回调自动切换回 UI 线程
var uiScheduler = SyncContextScheduler.Capture(); // 在 UI 线程上调用

var sub = GeneralEventBus.Default.Subscribe<SensorDataEvent>(e =>
{
    myLabel.Text = $"{e.SensorId}: {e.Value}°C"; // 安全，已在 UI 线程
}, uiScheduler);
```

### 3. 发布事件

从任意线程调用 `Publish`，无需关心当前线程上下文。

```csharp
// 在后台任务中发布
Task.Run(async () =>
{
    while (true)
    {
        var reading = await sensor.ReadAsync();
        GeneralEventBus.Default.Publish(new SensorDataEvent("T-01", reading, DateTime.UtcNow));
        await Task.Delay(1000);
    }
});
```

### 4. 取消订阅

`Subscribe` 返回的 `EventSubscription` 实现 `IDisposable`，Dispose 即取消订阅。

```csharp
// 手动取消
sub.Dispose();

// 或用 using 语句管理生命周期
using var sub = GeneralEventBus.Default.Subscribe<AlertEvent>(e => { ... });

// 窗体/组件关闭时批量取消（推荐模式）
private readonly List<IDisposable> _subscriptions = new();

_subscriptions.Add(GeneralEventBus.Default.Subscribe<SensorDataEvent>(OnSensorData, uiScheduler));
_subscriptions.Add(GeneralEventBus.Default.Subscribe<AlertEvent>(OnAlert, uiScheduler));

// OnClose / Dispose 时
_subscriptions.ForEach(s => s.Dispose());
```

---

## 调度器

| 调度器 | 说明 | 适用场景 |
|---|---|---|
| `ImmediateScheduler.Instance` | 在发布者线程直接执行（默认） | 后台服务、日志、无 UI 操作 |
| `ThreadPoolScheduler.Instance` | 投递到 .NET 线程池 | 耗时操作、不阻塞发布者 |
| `SyncContextScheduler.Capture()` | 投递到当前 `SynchronizationContext` | WinForms / WPF / MAUI UI 线程 |
| `TaskSchedulerScheduler` | 投递到指定 `TaskScheduler` | 自定义并发控制、受限并发场景 |
| 自定义 `IEventScheduler` | 实现接口即可 | 任意调度需求 |

### SyncContextScheduler 使用须知

`SyncContextScheduler.Capture()` 必须在**目标线程**（通常是 UI 线程）上调用，才能正确捕获该线程的上下文。

```csharp
// ✅ 正确：在 UI 线程（窗体构造函数、Loaded 事件）中捕获
public MainForm()
{
    var uiScheduler = SyncContextScheduler.Capture();
    GeneralEventBus.Default.Subscribe<MyEvent>(OnMyEvent, uiScheduler);
}

// ❌ 错误：在后台线程捕获，拿到的不是 UI 上下文
Task.Run(() =>
{
    var uiScheduler = SyncContextScheduler.Capture(); // 抛出异常或行为不符预期
});
```

也可以手动传入已有的 `SynchronizationContext`：

```csharp
var ctx = SynchronizationContext.Current; // 在 UI 线程保存
var uiScheduler = new SyncContextScheduler(ctx); // 可在任意线程使用
```

---

## 异步发布

`PublishAsync<T>` 会等待所有订阅者的回调执行完毕后才返回。

```csharp
await GeneralEventBus.Default.PublishAsync(new SensorDataEvent("T-02", 37.0, DateTime.UtcNow));
// 所有订阅者均已处理完毕
```

> **注意**：若订阅者使用 `ImmediateScheduler`，回调在当前线程同步执行；若使用 `ThreadPoolScheduler`，`PublishAsync` 会通过 `TaskCompletionSource` 等待其完成。`SyncContextScheduler` 订阅者的完成同样会被等待，但需确保目标线程的消息循环处于运行状态，否则可能死锁。

---

## 异常处理

默认情况下，订阅者抛出的异常会被捕获并写入 `Trace`，不会影响其他订阅者。

可替换为自定义处理逻辑：

```csharp
GeneralEventBus.UnhandledExceptionHandler = (ex, eventType) =>
{
    myLogger.Error(ex, "处理事件 {EventType} 时发生异常", eventType.Name);
};
```

---

## 自定义调度器

实现 `IEventScheduler` 接口即可接入任何调度机制：

```csharp
public sealed class SingleThreadScheduler : IEventScheduler
{
    private readonly TaskFactory _factory;

    public SingleThreadScheduler()
    {
        var sts = new ConcurrentExclusiveSchedulerPair();
        _factory = new TaskFactory(sts.ExclusiveScheduler);
    }

    public void Schedule(Action action) => _factory.StartNew(action);
}

// 使用
var serialScheduler = new SingleThreadScheduler();
GeneralEventBus.Default.Subscribe<MyEvent>(e => { /* 串行执行，无并发问题 */ }, serialScheduler);
```

---

## 完整示例：传感器监控

```csharp
// 事件定义
public record TemperatureReadingEvent(string DeviceId, double Celsius);
public record DeviceOfflineEvent(string DeviceId, DateTime LastSeen);

// 后台采集服务（与 UI 框架无关）
public class SensorService
{
    public void Start()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    var value = await ReadSensorAsync();
                    GeneralEventBus.Default.Publish(new TemperatureReadingEvent("DEV-001", value));
                }
                catch (DeviceException)
                {
                    GeneralEventBus.Default.Publish(new DeviceOfflineEvent("DEV-001", DateTime.UtcNow));
                }
                await Task.Delay(500);
            }
        });
    }
}

// WinForms 窗体（或 WPF Window，逻辑完全相同）
public class MonitorForm : Form
{
    private readonly List<IDisposable> _subs = new();

    public MonitorForm()
    {
        var ui = SyncContextScheduler.Capture(); // UI 线程

        _subs.Add(GeneralEventBus.Default.Subscribe<TemperatureReadingEvent>(e =>
        {
            labelTemp.Text = $"{e.Celsius:F1}°C";
            chart.AddPoint(DateTime.Now, e.Celsius);
        }, ui));

        _subs.Add(GeneralEventBus.Default.Subscribe<DeviceOfflineEvent>(e =>
        {
            labelStatus.Text = $"设备离线 @ {e.LastSeen:HH:mm:ss}";
            labelStatus.ForeColor = Color.Red;
        }, ui));
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _subs.ForEach(s => s.Dispose());
        base.OnFormClosed(e);
    }
}
```

---

## 注意事项

**循环引用**：若订阅者持有对发布者的引用，而发布者又持有订阅者，可能形成内存泄漏。始终在组件销毁时调用 `Dispose`。

**发布顺序**：同一事件类型的多个订阅者按注册顺序分发，但跨线程调度后实际执行顺序不保证。

**值类型事件**：`Publish<T>` 支持 struct 事件，但装箱/拆箱会发生。高频场景建议使用 class 或 record。

**SyncContextScheduler 与死锁**：在 `await PublishAsync` 的同时，若 UI 消息循环被 `.Result` 或 `.Wait()` 阻塞，会发生死锁。始终用 `await`，不要用 `.Result`。

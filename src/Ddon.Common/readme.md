# Ddon.Common

零外部依赖的 .NET 通用工具库（仅依赖 .NET BCL）。

```
EventBus/       类型安全的事件总线
Utilities/
├── MainLoop/   可复用定时循环
├── WheelTimer/ 高性能哈希时间轮
└── DelayQueue  延迟队列
```

---

## EventBus — 事件总线

类型安全、线程安全的发布/订阅组件，支持任意线程发布和灵活的回调调度。

```csharp
// 订阅（返回 IDisposable，Dispose 即取消）
var sub = GeneralEventBus.Default.Subscribe<SensorDataEvent>(e => { });

// 发布（任意线程）
GeneralEventBus.Default.Publish(new SensorDataEvent("T-01", 37.5, DateTime.UtcNow));

// 异步发布（等待所有订阅者完成）
await GeneralEventBus.Default.PublishAsync(new SensorDataEvent("T-02", 38.0, DateTime.UtcNow));
```

完整文档见 [`EventBus/Readme.md`](EventBus/Readme.md)。

---

## MainLoop — 定时循环

周期性执行任务的抽象基类，支持同步/异步、UI 线程事件回调。

```csharp
var loop = new AsyncMainLoop(
    interval: TimeSpan.FromMilliseconds(100),
    loopAction: async ct =>
    {
        var data = await ReadSensorAsync(ct);
        Console.WriteLine($"Value: {data}");
    },
    highPrecision: true,
    captureUiContext: false
);

loop.Start();
await loop.StopAsync();
```

### 事件
| 事件 | 说明 |
|------|------|
| `Started` | 循环启动 |
| `Stopped` | 循环停止 |
| `BeforeLoop` | 每次迭代前触发 |
| `AfterLoop` | 每次迭代后触发 |
| `LoopException` | 异常通知 |

---

## WheelTimer — 哈希时间轮

高性能近似定时器，灵感来自 Netty 的 `HashedWheelTimer`。适用于大量超时任务。

```csharp
var timer = new HashedWheelTimer(tickDuration: TimeSpan.FromMilliseconds(100));

// 3 秒后执行
timer.NewTimeout(() => Console.WriteLine("Timeout!"), TimeSpan.FromSeconds(3));

// 可取消
var timeout = timer.NewTimeout(() => { }, TimeSpan.FromSeconds(5));
timeout.Cancel();
```

---

## DelayQueue — 延迟队列

元素按延迟时间出队的阻塞队列。

```csharp
var queue = new DelayQueue<string>();

queue.Enqueue("消息1", TimeSpan.FromSeconds(3));
queue.Enqueue("消息2", TimeSpan.FromSeconds(1));

var msg = await queue.DequeueAsync(); // 1 秒后 -> "消息2"
var msg = queue.Dequeue();            // 2 秒后 -> "消息1"
```

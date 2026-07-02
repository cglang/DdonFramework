using System.Reflection;
using Ddon.EventBus.Contracts;
using Ddon.EventBus.Memory;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("=== Ddon.EventBus.Memory Demo ===");
Console.WriteLine();

// 1. 通过 DI 注册 EventBus + Handler 扫描
Console.WriteLine("--- A. DI 方式, 扫描当前程序集注册 Handler ---");
var services = new ServiceCollection();
services.AddEventBus(Assembly.GetExecutingAssembly());
var sp = services.BuildServiceProvider();
var bus = sp.GetRequiredService<IEventBus>();

Console.WriteLine();
Console.WriteLine("发布 OrderCreatedEvent:");
await bus.PublishAsync(new OrderCreatedEvent("ORD-001", 299.99m));

Console.WriteLine();
Console.WriteLine("发布 PaymentReceivedEvent (2 个 Handler 同时响应):");
await bus.PublishAsync(new PaymentReceivedEvent("ORD-001", 299.99m));

Console.WriteLine();
Console.WriteLine("发布 OrderShippedEvent (IDomainEventData):");
await bus.PublishAsync((IDomainEventData)new OrderShippedEvent("ORD-001", "SF-Express"));

// 2. 手动订阅 GeneralEventBus.Default (绕过 DI)
Console.WriteLine();
Console.WriteLine("--- B. 手动订阅 GeneralEventBus.Default ---");
using var sub = Ddon.Common.EventBus.GeneralEventBus.Default.Subscribe<OrderCreatedEvent>(e =>
{
    Console.WriteLine($"  [Manual Sub] Got Order {e.OrderId}, Amount: {e.Amount:C}");
});

Console.WriteLine();
await bus.PublishAsync(new OrderCreatedEvent("ORD-002", 199.99m));

sub.Dispose();
Console.WriteLine("  [Manual Sub] 已取消订阅");

Console.WriteLine();
await bus.PublishAsync(new OrderCreatedEvent("ORD-003", 99.99m));
Console.WriteLine("  (手动订阅已取消, 不会再收到回调)");

Console.WriteLine();
Console.WriteLine("=== Demo Complete ===");

// ── 事件类型 ─────────────────────────────────────────────────

public sealed record OrderCreatedEvent(string OrderId, decimal Amount) : IEventData;
public sealed record OrderShippedEvent(string OrderId, string Carrier) : IDomainEventData;
public sealed record PaymentReceivedEvent(string OrderId, decimal Amount) : IEventData;

// ── Handler ──────────────────────────────────────────────────

public sealed class OrderCreatedHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly int _instanceId;
    private static int _nextId;

    public OrderCreatedHandler()
    {
        _instanceId = Interlocked.Increment(ref _nextId);
    }

    public Task HandleAsync(OrderCreatedEvent eventData, CancellationToken cancellationToken)
    {
        Console.WriteLine($"  [Handler #{_instanceId}] Order {eventData.OrderId} created, Amount: {eventData.Amount:C}");
        return Task.CompletedTask;
    }
}

public sealed class OrderShippedHandler : IDomainEventHandler<OrderShippedEvent>
{
    public Task HandleAsync(OrderShippedEvent eventData, CancellationToken cancellationToken)
    {
        Console.WriteLine($"  [DomainHandler] Order {eventData.OrderId} shipped via {eventData.Carrier}");
        return Task.CompletedTask;
    }
}

public sealed class PaymentLogger : IEventHandler<PaymentReceivedEvent>
{
    public Task HandleAsync(PaymentReceivedEvent eventData, CancellationToken cancellationToken)
    {
        Console.WriteLine($"  [Logger] Payment received for Order {eventData.OrderId}: {eventData.Amount:C}");
        return Task.CompletedTask;
    }
}

public sealed class PaymentAuditor : IEventHandler<PaymentReceivedEvent>
{
    public Task HandleAsync(PaymentReceivedEvent eventData, CancellationToken cancellationToken)
    {
        Console.WriteLine($"  [Auditor] Auditing payment for Order {eventData.OrderId}...");
        return Task.CompletedTask;
    }
}

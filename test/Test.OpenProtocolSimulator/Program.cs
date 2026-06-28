using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using OpenProtocolInterpreter;
using OpenProtocolInterpreter.Communication;
using OpenProtocolInterpreter.IOInterface;
using OpenProtocolInterpreter.KeepAlive;
using OpenProtocolInterpreter.ParameterSet;
using OpenProtocolInterpreter.Tightening;
using OpenProtocolInterpreter.Tool;

var port = args.Length > 0 ? int.Parse(args[0]) : 4545;
var intervalMs = args.Length > 1 ? int.Parse(args[1]) : 10_000;

var listener = new TcpListener(IPAddress.Any, port);
listener.Start();

Console.WriteLine($"扭紧机模拟器已启动, 端口: {port}");
Console.WriteLine($"每 {intervalMs}ms 模拟一次拧紧结果");
Console.WriteLine("按 Ctrl+C 停止");
Console.WriteLine();

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

var clientCount = 0;

try
{
    while (!cts.Token.IsCancellationRequested)
    {
        var tcpClient = await listener.AcceptTcpClientAsync(cts.Token);
        var id = Interlocked.Increment(ref clientCount);
        _ = HandleClientAsync(tcpClient, id, intervalMs, cts.Token);
    }
}
catch (OperationCanceledException) { }
finally
{
    listener.Stop();
    Console.WriteLine("模拟器已停止.");
}

static async Task HandleClientAsync(TcpClient client, int clientId, int intervalMs, CancellationToken ct)
{
    var interpreter = new MidInterpreter().UseAllMessages();
    var stream = client.GetStream();
    var buffer = new byte[4096];
    var acc = new List<byte>();
    var rand = new Random(clientId * 1000 + Environment.TickCount);

    var state = new ClientState
    {
        Name = $"客户端#{clientId}",
        TighteningId = 1000 + clientId,
        Subscribed = 0,
        Running = 1,
    };

    Console.WriteLine($"[{state.Name}] 已连接 ({client.Client.RemoteEndPoint})");

    var pushTask = Task.Run(async () =>
    {
        while (Volatile.Read(ref state.Running) != 0 && !ct.IsCancellationRequested)
        {
            await Task.Delay(intervalMs, ct);

            if (Volatile.Read(ref state.Subscribed) == 0) continue;

            var id = Interlocked.Increment(ref state.TighteningId);
            var result = SimulateTightening(rand, id);

            try
            {
                var packet = result.Pack();
                var data = Encoding.ASCII.GetBytes(packet + "\0");
                await stream.WriteAsync(data, ct);
                await stream.FlushAsync(ct);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(
                    $"[{state.Name}] → MID0061 推送拧紧结果 | " +
                    $"扭矩={result.Torque:F2}Nm " +
                    $"角度={result.Angle:F0}度 " +
                       $"状态={(result.TighteningStatus ? "Ok" : "Low")}");
                Console.ResetColor();
            }
            catch { break; }
        }
    }, ct);

    try
    {
        while (Volatile.Read(ref state.Running) != 0 && !ct.IsCancellationRequested)
        {
            var read = await stream.ReadAsync(buffer, ct);
            if (read == 0) break;

            acc.AddRange(buffer.AsSpan(0, read));

            while (TryExtractFrame(acc, out var frame))
            {
                var mid = interpreter.Parse(frame);
                if (mid is null)
                {
                    Console.WriteLine($"[{state.Name}] ✗ 无法解析帧");
                    continue;
                }

                await HandleMidAsync(mid, stream, state, rand, ct);
            }
        }
    }
    catch (OperationCanceledException) { }
    catch (Exception ex)
    {
        Console.WriteLine($"[{state.Name}] 异常: {ex.Message}");
    }
    finally
    {
        Volatile.Write(ref state.Running, 0);
        client.Dispose();
        Console.WriteLine($"[{state.Name}] 已断开.");
    }
}

static Mid0061 SimulateTightening(Random rand, int tighteningId)
{
    var mid = new Mid0061();
    mid.TighteningId = tighteningId;
    mid.VinNumber = $"SIM{rand.Next(100000, 999999)}";
    mid.Torque = (decimal)Math.Round(10.0 + rand.NextDouble() * 90, 2);
    mid.TorqueMinLimit = 5;
    mid.TorqueMaxLimit = 100;
    mid.Angle = rand.Next(0, 3600);
    mid.AngleMinLimit = 0;
    mid.AngleMaxLimit = 3600;
    mid.TighteningStatus = rand.Next(0, 10) > 1;
    mid.TorqueStatus = TighteningValueStatus.Ok;
    mid.AngleStatus = TighteningValueStatus.Ok;
    return mid;
}

static bool TryExtractFrame(List<byte> buffer, out byte[]? frame)
{
    frame = null;

    while (buffer.Count > 0 && (buffer[0] < '0' || buffer[0] > '9'))
        buffer.RemoveAt(0);

    if (buffer.Count < 4) return false;

    var lenStr = Encoding.ASCII.GetString(buffer.GetRange(0, 4).ToArray());
    if (!int.TryParse(lenStr, out var frameLen) || frameLen <= 0 || frameLen > 65535)
    {
        buffer.RemoveAt(0);
        return false;
    }

    if (buffer.Count < frameLen) return false;

    frame = buffer.GetRange(0, frameLen).ToArray();
    buffer.RemoveRange(0, frameLen);
    return true;
}

static async Task HandleMidAsync(
    Mid mid, NetworkStream stream, ClientState state, Random rand, CancellationToken ct)
{
    switch (mid)
    {
        case Mid0001:
            Console.WriteLine($"[{state.Name}] ← MID0001 连接请求");
            await SendMidAsync(stream, new Mid0002());
            Console.WriteLine($"[{state.Name}] → MID0002 握手成功");
            break;

        case Mid0003:
            Console.WriteLine($"[{state.Name}] ← MID0003 断开请求");
            await SendMidAsync(stream, new Mid0005(mid.Header.Mid));
            Console.WriteLine($"[{state.Name}] → MID0005 确认断开");
            break;

        case Mid0060:
            Volatile.Write(ref state.Subscribed, 1);
            Console.WriteLine($"[{state.Name}] ← MID0060 订阅拧紧结果");
            await SendMidAsync(stream, new Mid0005(mid.Header.Mid));

            var id = Interlocked.Increment(ref state.TighteningId);
            var first = SimulateTightening(rand, id);
            var packet = first.Pack();
            var data = Encoding.ASCII.GetBytes(packet);
            await stream.WriteAsync(data, ct);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(
                $"[{state.Name}] → MID0061 推送首个结果 | " +
                $"扭矩={first.Torque:F2}Nm " +
                $"角度={first.Angle:F0}度 " +
                $"状态={(first.TighteningStatus ? "Ok" : "Low")}");
            Console.ResetColor();
            break;

        case Mid0062:
            Volatile.Write(ref state.Subscribed, 0);
            Console.WriteLine($"[{state.Name}] ← MID0062 取消订阅");
            await SendMidAsync(stream, new Mid0005(mid.Header.Mid));
            Console.WriteLine($"[{state.Name}] → MID0005 已取消订阅");
            break;

        case Mid0018 m18:
            Console.WriteLine($"[{state.Name}] ← MID0018 选择参数组 #{m18.ParameterSetId}");
            await SendMidAsync(stream, new Mid0005(mid.Header.Mid));
            Console.WriteLine($"[{state.Name}] → MID0005 参数组已切换");
            break;

        case Mid0042:
            Console.WriteLine($"[{state.Name}] ← MID0042 禁用工具");
            await SendMidAsync(stream, new Mid0005(mid.Header.Mid));
            Console.WriteLine($"[{state.Name}] → MID0005 工具已禁用");
            break;

        case Mid0043:
            Console.WriteLine($"[{state.Name}] ← MID0043 启用工具");
            await SendMidAsync(stream, new Mid0005(mid.Header.Mid));
            Console.WriteLine($"[{state.Name}] → MID0005 工具已启用");
            break;

        case Mid9999:
            Console.WriteLine($"[{state.Name}] ← MID9999 心跳");
            await SendMidAsync(stream, new Mid9999());
            break;

        default:
            Console.WriteLine($"[{state.Name}] ← MID{mid.Header.Mid:D4} (未处理)");
            await SendMidAsync(stream, new Mid0005(mid.Header.Mid));
            break;
    }
}

static async Task SendMidAsync(NetworkStream stream, Mid mid)
{
    var packed = mid.Pack();
    var data = Encoding.ASCII.GetBytes(packed);
    await stream.WriteAsync(data);
    await stream.FlushAsync();
}

sealed class ClientState
{
    public string Name = "";
    public int TighteningId;
    public int Subscribed;
    public int Running;
}

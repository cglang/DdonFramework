using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC.SyncEngine
{
    /// <summary>
    /// 写命令服务。
    /// 执行原则：直接写 PLC，不修改本地镜像，返回结果注明"需扫描确认"。
    /// </summary>
    public sealed class WriteCommandService : IWriteCommandService
    {
        private readonly IPlcClient _client;
        private readonly ITagRegistry _registry;
        private readonly EndianFormat _endian;
        private readonly ILogger<WriteCommandService> _logger;

        public WriteCommandService(
            IPlcClient client,
            ITagRegistry registry,
            EndianFormat endian,
            ILogger<WriteCommandService> logger)
        {
            _client = client;
            _registry = registry;
            _endian = endian;
            _logger = logger;
        }

        public async Task<WriteResult> ExecuteAsync<T>(TagDefinition tag, T value, CancellationToken ct = default)
        {
            try
            {
                var addr = AddressParser.Parse(tag.Address, tag.Type);
                var bytes = PlcCodec.Encode(value, tag.Type, _endian, addr.ByteOffset, addr.BitIndex, tag.StringLength);

                _logger.LogDebug("写入 PLC: {Tag}={Value} @ {Address} ({Bytes} bytes)",
                    tag.Name, value, tag.Address, bytes.Length);

                await _client.WriteBytesAsync(tag.Address, bytes, ct);

                _logger.LogInformation("写入成功: {Tag}={Value} (等待扫描确认)", tag.Name, value);
                return WriteResult.Ok(tag.Name, value);
            }
            catch (OperationCanceledException)
            {
                return WriteResult.Fail(tag.Name, "操作已取消");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "写入失败: {Tag}", tag.Name);
                return WriteResult.Fail(tag.Name, ex.Message, ex);
            }
        }
    }
}

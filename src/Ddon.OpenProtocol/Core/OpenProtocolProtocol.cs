using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Ddon.OpenProtocol.Abstractions;
using Ddon.OpenProtocol.Configuration;
using Microsoft.Extensions.Logging;
using OpenProtocolInterpreter;

namespace Ddon.OpenProtocol.Core
{
    public class OpenProtocolProtocol : IOpenProtocolProtocol
    {
        private readonly MidInterpreter _interpreter;
        private readonly OpenProtocolEndpointOptions _options;
        private readonly ILogger<OpenProtocolProtocol>? _logger;

        private readonly ConcurrentDictionary<int, int> _responseMap = new();

        public IReadOnlyDictionary<int, int> ResponseMap =>
            new Dictionary<int, int>(_responseMap);

        public OpenProtocolProtocol(
            OpenProtocolEndpointOptions options,
            MidInterpreter interpreter,
            ILogger<OpenProtocolProtocol>? logger = null)
        {
            _options = options;
            _interpreter = interpreter;
            _logger = logger;
        }

        public byte[] Serialize(Mid mid)
        {
            string packed = mid.Pack();

            byte[] terminator = _options.Terminator switch
            {
                MessageTerminator.None => [],
                MessageTerminator.Nul => [0],
                MessageTerminator.CrLf => [0x0D, 0x0A],
                MessageTerminator.Custom => _options.CustomTerminator ?? [],
                _ => [0],
            };

            var data = Encoding.ASCII.GetBytes(packed);
            if (terminator.Length == 0)
                return data;

            var result = new byte[data.Length + terminator.Length];
            Buffer.BlockCopy(data, 0, result, 0, data.Length);
            Buffer.BlockCopy(terminator, 0, result, data.Length, terminator.Length);
            return result;
        }

        public Mid? Deserialize(byte[] packet)
        {
            var mid = SafeParse(packet);
            return mid;
        }

        private Mid? SafeParse(byte[] packet)
        {
            try
            {
                var mid = _interpreter.Parse(packet);
                if (mid is not null) return mid;
            }
            catch (Exception ex)
            {
                string revInfo = "?";
                if (packet.Length > 11)
                {
                    revInfo = Encoding.ASCII.GetString(
                        new[] { packet[8], packet[9], packet[10] });
                }
                _logger?.LogDebug(
                    "Parse failed (rev={Rev}), will retry with lower revision. {Msg}",
                    revInfo, ex.Message);
            }

            byte[] patched = (byte[])packet.Clone();
            int[] revisions = new int[] { 6, 5, 4, 3, 2, 1 };
            foreach (int rev in revisions)
            {
                try
                {
                    string revStr = rev.ToString("D3");
                    patched[8] = (byte)revStr[0];
                    patched[9] = (byte)revStr[1];
                    patched[10] = (byte)revStr[2];

                    var mid = _interpreter.Parse(patched);
                    if (mid is not null)
                    {
                        string rawMid = packet.Length > 8
                        ? Encoding.ASCII.GetString(
                            new[] { packet[4], packet[5], packet[6], packet[7] })
                        : "????";
                    _logger?.LogDebug(
                        "MID{Raw} parsed with downgraded revision {Rev}.",
                        rawMid, rev);
                        return mid;
                    }
                }
                catch { }
            }

            return null;
        }

        public void RegisterCustomMid<T>(int? midNumber = null) where T : Mid
        {
            int mid = midNumber ?? GetMidFromType<T>();

            _interpreter.UseCustomMessage(new Dictionary<int, Type>
            {
                { mid, typeof(T) }
            });
        }

        public bool TryMapResponse(int requestMid, int responseMid)
        {
            return _responseMap.TryAdd(requestMid, responseMid);
        }

        public bool TryGetResponseMid(int requestMid, out int responseMid)
        {
            return _responseMap.TryGetValue(requestMid, out responseMid);
        }

        internal static int GetMidFromType<T>() where T : Mid
        {
            var field = typeof(T).GetField("MID",
                BindingFlags.Public | BindingFlags.Static);

            if (field?.FieldType == typeof(int))
                return (int)field.GetValue(null)!;

            var instance = Activator.CreateInstance<T>();
            return instance.Header.Mid;
        }
    }
}

﻿using Ddon.Core.Exceptions;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Core.Use.Socket
{
    public class DdonSocketCore : DdonSocketBase
    {
        private Func<DdonSocketCore, byte[], Task>? _byteHandler;
        private Func<DdonSocketCore, string, Task>? _stringHandler;
        private Func<DdonSocketCore, DdonSocketException, Task>? _exceptionHandler;

        public DdonSocketCore(
            TcpClient tcpClient,
            Func<DdonSocketCore, byte[], Task>? byteHandler,
            Func<DdonSocketCore, DdonSocketException, Task>? exceptionHandler = null) : base(tcpClient)
        {
            _byteHandler += byteHandler;
            _exceptionHandler += exceptionHandler;
            ConsecutiveReadStream();
        }

        public DdonSocketCore(TcpClient tcpClient) : base(tcpClient)
        {
            ConsecutiveReadStream();
        }

        public DdonSocketCore(string host, int port) : base(new TcpClient(host, port))
        {
            ConsecutiveReadStream();
        }

        private void ConsecutiveReadStream()
        {
            var newThread = new Thread(Start);
            newThread.Start();
        }

        private async void Start()
        {
            try
            {
                while (true)
                {
                    if (!TcpClient.Connected) throw new Exception("客户端已正常断开");

                    var dataSize = await Stream.ReadLengthBytesAsync(sizeof(int));
                    var length = BitConverter.ToInt32(dataSize);
                    var initialBytes = await Stream.ReadLengthBytesAsync(length);

                    if (_byteHandler == null) continue;
                    await _byteHandler(this, initialBytes);

                    if (_stringHandler == null) continue;
                    var data = Encoding.UTF8.GetString(initialBytes);
                    await _stringHandler(this, data);
                }
            }
            catch (Exception ex)
            {
                if (_exceptionHandler != null)
                {
                    var socketEx = new DdonSocketException(ex, SocketId);
                    await _exceptionHandler(this, socketEx);
                }
            }
        }

        /// <summary>
        /// 发送Byte数组
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>发送的数据字节长度</returns>
        public async Task<int> SendBytesAsync(byte[] data)
        {
            var lengthByte = BitConverter.GetBytes(data.Length);
            byte[] contentBytes = DdonArray.MergeArrays(lengthByte, data);

            await Stream.WriteAsync(contentBytes);
            return data.Length;
        }

        /// <summary>
        /// 发送字符串
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>发送的数据字节长度</returns>
        public async Task<int> SendStringAsync(string data)
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);
            return await SendBytesAsync(dataBytes);
        }

        /// <summary>
        /// 发送Json字符串
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>发送的数据字节长度</returns>
        public async Task<int> SendJsonAsync<TData>(TData data)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };
            return await SendStringAsync(JsonSerializer.Serialize(data, options));
        }

        public DdonSocketCore ByteHandler(Func<DdonSocketCore, byte[], Task>? byteHandler)
        {
            this._byteHandler += byteHandler;
            return this;
        }

        public DdonSocketCore StringHandler(Func<DdonSocketCore, string, Task>? stringHandler)
        {
            _stringHandler += stringHandler;
            return this;
        }

        public DdonSocketCore ExceptionHandler(Func<DdonSocketCore, DdonSocketException, Task>? exceptionHandler)
        {
            _exceptionHandler += exceptionHandler;
            return this;
        }
    }
}
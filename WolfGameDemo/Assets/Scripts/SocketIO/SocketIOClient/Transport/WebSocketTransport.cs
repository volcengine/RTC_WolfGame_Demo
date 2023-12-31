﻿/* MIT License
 *
 * Copyright (c) 2019 HeroWong
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Text;
using SocketIOClient.JsonSerializer;

namespace SocketIOClient.Transport
{
    public class WebSocketTransport : BaseTransport
    {
        public WebSocketTransport(IClientWebSocket ws, SocketIOOptions options, IJsonSerializer jsonSerializer) : base(options, jsonSerializer)
        {
            _ws = ws;

            _ws.TextSubject.Subscribe(this);
        }

        readonly IClientWebSocket _ws;

        void SendAsync(byte[] bytes)
        {
            _ws.SendAsync(bytes);

        }

        public override void ConnectAsync(Uri uri)
        {
            _ws.ConnectAsync(uri);
        }

        public override void DisconnectAsync()
        {
            _ws.DisconnectAsync();
        }

        public override void SendAsync(Payload payload)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(payload.Text);

            SendAsync(bytes);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}

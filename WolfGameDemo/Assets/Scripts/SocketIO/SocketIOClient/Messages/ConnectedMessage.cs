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
using SocketIOClient.Transport;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace SocketIOClient.Messages
{
    public class ConnectedMessage : IMessage
    {
        public MessageType Type => MessageType.Connected;

        public string Namespace { get; set; }

        public string Sid { get; set; }

        public List<byte[]> OutgoingBytes { get; set; }

        public List<byte[]> IncomingBytes { get; set; }

        public int BinaryCount { get; }

        public int Eio { get; set; }

        public TransportProtocol Protocol { get; set; }

        public IEnumerable<KeyValuePair<string, string>> Query { get; set; }
        public string AuthJsonStr { get; set; }

        public void Read(string msg)
        {
            if (Eio == 3)
            {
                Eio3Read(msg);
            }
            else
            {
                Eio4Read(msg);
            }
        }

        public string Write()
        {
            if (Eio == 3)
            {
                return Eio3Write();
            }
            return Eio4Write();
        }

        public void Eio4Read(string msg)
        {
            int index = msg.IndexOf('{');
            if (index > 0)
            {
                Namespace = msg.Substring(0, index - 1);
                msg = msg.Substring(index);
            }
            else
            {
                Namespace = string.Empty;
            }
            Sid = JsonDocument.Parse(msg).RootElement.GetProperty("sid").GetString();
        }

        public string Eio4Write()
        {
            var builder = new StringBuilder("40");
            if (!string.IsNullOrEmpty(Namespace))
            {
                builder.Append(Namespace).Append(',');
            }
            builder.Append(AuthJsonStr);
            return builder.ToString();
        }

        public void Eio3Read(string msg)
        {
            if (msg.Length >= 2)
            {
                int startIndex = msg.IndexOf('/');
                if (startIndex == -1)
                {
                    return;
                }
                int endIndex = msg.IndexOf('?', startIndex);
                if (endIndex == -1)
                {
                    endIndex = msg.IndexOf(',', startIndex);
                }
                if (endIndex == -1)
                {
                    endIndex = msg.Length;
                }
                Namespace = msg.Substring(startIndex, endIndex);
            }
        }

        public string Eio3Write()
        {
            if (string.IsNullOrEmpty(Namespace))
            {
                return string.Empty;
            }
            var builder = new StringBuilder("40");
            builder.Append(Namespace);
            if (Query != null)
            {
                int i = -1;
                foreach (var item in Query)
                {
                    i++;
                    if (i == 0)
                    {
                        builder.Append('?');
                    }
                    else
                    {
                        builder.Append('&');
                    }
                    builder.Append(item.Key).Append('=').Append(item.Value);
                }
            }
            builder.Append(',');
            return builder.ToString();
        }
    }
}

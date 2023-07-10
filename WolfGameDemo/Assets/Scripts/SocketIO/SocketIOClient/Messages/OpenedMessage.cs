/* MIT License
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
using System.Text.Json;
using System.Collections.Generic;
using SocketIOClient.Transport;

namespace SocketIOClient.Messages
{
    public class OpenedMessage : IMessage
    {
        public MessageType Type => MessageType.Opened;

        public string Sid { get; set; }

        public string Namespace { get; set; }

        public List<string> Upgrades { get; private set; }

        public int PingInterval { get; private set; }

        public int PingTimeout { get; private set; }

        public List<byte[]> OutgoingBytes { get; set; }

        public List<byte[]> IncomingBytes { get; set; }

        public int BinaryCount { get; }

        public int Eio { get; set; }

        public TransportProtocol Protocol { get; set; }

        private int GetInt32FromJsonElement(JsonElement element, string msg, string name)
        {
            var p = element.GetProperty(name);
            int val;
            switch (p.ValueKind)
            {
                case JsonValueKind.String:
                    val = int.Parse(p.GetString());
                    break;
                case JsonValueKind.Number:
                    val = p.GetInt32();
                    break;
                default:
                    throw new ArgumentException($"Invalid message: '{msg}'");
            }
            return val;
        }

        public void Read(string msg)
        {
            var doc = JsonDocument.Parse(msg);
            var root = doc.RootElement;
            Sid = root.GetProperty("sid").GetString();

            PingInterval = GetInt32FromJsonElement(root, msg, "pingInterval");
            PingTimeout = GetInt32FromJsonElement(root, msg, "pingTimeout");

            Upgrades = new List<string>();
            var upgrades = root.GetProperty("upgrades").EnumerateArray();
            foreach (var item in upgrades)
            {
                Upgrades.Add(item.GetString());
            }
        }

        public string Write()
        {
            //var builder = new StringBuilder();
            //builder.Append("40");
            //if (!string.IsNullOrEmpty(Namespace))
            //{
            //    builder.Append(Namespace).Append(',');
            //}
            //return builder.ToString();
            throw new NotImplementedException();
        }
    }
}

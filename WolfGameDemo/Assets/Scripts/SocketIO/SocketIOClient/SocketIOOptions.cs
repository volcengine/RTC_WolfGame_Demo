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

using SocketIOClient.Transport;
using System;
using System.Collections.Generic;

namespace SocketIOClient
{
    public sealed class SocketIOOptions
    {
        public SocketIOOptions()
        {
            RandomizationFactor = 0.5;
            ReconnectionDelay = 1000;
            ReconnectionDelayMax = 5000;
            ReconnectionAttempts = int.MaxValue;
            Path = "/socket.io";
            ConnectionTimeout = TimeSpan.FromSeconds(20);
            Reconnection = true;
            Transport = TransportProtocol.WebSocket;
            EIO = 4;
            AutoUpgrade = true;
        }

        public string Path { get; set; }

        public TimeSpan ConnectionTimeout { get; set; }

        public IEnumerable<KeyValuePair<string, string>> Query { get; set; }

        /// <summary>
        /// Whether to allow reconnection if accidentally disconnected
        /// </summary>
        public bool Reconnection { get; set; }

        public double ReconnectionDelay { get; set; }
        public int ReconnectionDelayMax { get; set; }
        public int ReconnectionAttempts { get; set; }

        double _randomizationFactor;
        public double RandomizationFactor
        {
            get => _randomizationFactor;
            set
            {
                if (value >= 0 && value <= 1)
                {
                    _randomizationFactor = value;
                }
                else
                {
                    throw new ArgumentException($"{nameof(RandomizationFactor)} should be greater than or equal to 0.0, and less than 1.0.");
                }
            }
        }

        public TransportProtocol Transport { get; set; }

        public int EIO { get; set; }

        public bool AutoUpgrade { get; set; }

        public object Auth { get; set; }
    }
}

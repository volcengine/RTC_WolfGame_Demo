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
 * 
 * This file may have been modified by Beijing Volcano Engine Technology Ltd.
 * (“ Volcano Engine's Modifications”). All Volcano Engine's Modifications are
 * Copyright (2023) Beijing Volcano Engine Technology Ltd..
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using SocketIOClient.JsonSerializer;
using SocketIOClient.Messages;
using SocketIOClient.UriConverters;
using UnityEngine;

namespace SocketIOClient.Transport
{
    public abstract class BaseTransport : IObserver<string>, IObservable<IMessage>, IDisposable
    {
        public BaseTransport(SocketIOOptions options, IJsonSerializer jsonSerializer)
        {
            Options = options;
            MessageSubject = new Subject<IMessage>();
            JsonSerializer = jsonSerializer;
            UriConverter = new UriConverter();
            _messageQueue = new Queue<IMessage>();
        }

        DateTime _pingTime;
        readonly Queue<IMessage> _messageQueue;

        protected SocketIOOptions Options { get; }
        protected Subject<IMessage> MessageSubject { get; }

        protected IJsonSerializer JsonSerializer { get; }
        protected OpenedMessage OpenedMessage { get; private set; }

        public string Namespace { get; set; }
        public IUriConverter UriConverter { get; set; }

        public void SendAsync(IMessage msg)
        {
            msg.Eio = Options.EIO;
            msg.Protocol = Options.Transport;
            var payload = new Payload
            {
                Text = msg.Write()
            };
            SendAsync(payload);
        }

        protected virtual void OpenAsync(OpenedMessage msg)
        {
            OpenedMessage = msg;
            if (Options.EIO == 3 && string.IsNullOrEmpty(Namespace))
            {
                return;
            }
            var connectMsg = new ConnectedMessage
            {
                Namespace = Namespace,
                Eio = Options.EIO,
                Query = Options.Query,
            };
            if (Options.EIO == 4)
            {
                if (Options.Auth != null)
                {
                    connectMsg.AuthJsonStr = JsonSerializer.Serialize(new[] { Options.Auth }).Json.TrimStart('[').TrimEnd(']');
                }
            }


            try
            {
                SendAsync(connectMsg);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        /// <summary>
        /// <para>Eio3 ping is sent by the client</para>
        /// <para>Eio4 ping is sent by the server</para>
        /// </summary>
        ///

        private async Task StartPing()
        {
            System.Diagnostics.Debug.WriteLine($"[Ping] Interval: {OpenedMessage.PingInterval}");
            var lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            while (true)
            {
                await Task.Yield();
                System.Threading.Thread.Sleep(200);

                var now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                if (now - lastTime >= OpenedMessage.PingInterval)
                {
                    lastTime = now;

                    try
                    {
                        var ping = new PingMessage();
                        System.Diagnostics.Debug.WriteLine($"[Ping] Sending");
                        SendAsync(ping);
                        System.Diagnostics.Debug.WriteLine($"[Ping] Has been sent");
                        _pingTime = DateTime.Now;
                        MessageSubject.OnNext(ping);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Ping] Failed to send, {e.Message}");
                        MessageSubject.OnError(e);
                        break;
                    }
                }
            }
        }

        public abstract void ConnectAsync(Uri uri);

        public abstract void DisconnectAsync();


        public virtual void Dispose()
        {
            MessageSubject.Dispose();
            _messageQueue.Clear();

        }

        public abstract void SendAsync(Payload payload);

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            MessageSubject.OnError(error);
        }

        public void OnNext(string text)
        {
            System.Diagnostics.Debug.WriteLine($"[Receive] {text}");
            var msg = MessageFactory.CreateMessage(Options.EIO, text);
            if (msg == null)
            {
                return;
            }
            if (msg.BinaryCount > 0)
            {
                msg.IncomingBytes = new List<byte[]>(msg.BinaryCount);
                _messageQueue.Enqueue(msg);
                return;
            }
            if (msg.Type == MessageType.Opened)
            {
                OpenAsync(msg as OpenedMessage);
            }

            if (Options.EIO == 3)
            {
                if (msg.Type == MessageType.Connected)
                {
                    var connectMsg = msg as ConnectedMessage;
                    connectMsg.Sid = OpenedMessage.Sid;
                    if ((string.IsNullOrEmpty(Namespace) && string.IsNullOrEmpty(connectMsg.Namespace)) || connectMsg.Namespace == Namespace)
                    {

                        _ = StartPing();
                    }
                    else
                    {
                        return;
                    }
                }
                else if (msg.Type == MessageType.Pong)
                {
                    var pong = msg as PongMessage;
                    pong.Duration = DateTime.Now - _pingTime;
                }
            }

            MessageSubject.OnNext(msg);

            if (msg.Type == MessageType.Ping)
            {
                _pingTime = DateTime.Now;
                try
                {
                    SendAsync(new PongMessage());
                    MessageSubject.OnNext(new PongMessage
                    {
                        Eio = Options.EIO,
                        Protocol = Options.Transport,
                        Duration = DateTime.Now - _pingTime
                    });
                }
                catch (Exception e)
                {
                    OnError(e);
                }
            }
        }

        public IDisposable Subscribe(IObserver<IMessage> observer)
        {
            return MessageSubject.Subscribe(observer);
        }
    }
}

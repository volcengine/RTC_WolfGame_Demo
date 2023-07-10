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
using System.Threading.Tasks;
using HybridWebSocket;
using SocketIOClient.JsonSerializer;
using SocketIOClient.Messages;
using SocketIOClient.Transport;
using SocketIOClient.UriConverters;

namespace SocketIOClient
{
    /// <summary>
    /// socket.io client class
    /// </summary>
    public class SocketIO : IDisposable
    {
        /// <summary>
        /// Create SocketIO object with default options
        /// </summary>
        /// <param name="uri"></param>
        public SocketIO(string uri) : this(new Uri(uri)) { }

        /// <summary>
        /// Create SocketIO object with options
        /// </summary>
        /// <param name="uri"></param>
        public SocketIO(Uri uri) : this(uri, new SocketIOOptions()) { }

        /// <summary>
        /// Create SocketIO object with options
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="options"></param>
        public SocketIO(string uri, SocketIOOptions options) : this(new Uri(uri), options) { }

        /// <summary>
        /// Create SocketIO object with options
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="options"></param>
        public SocketIO(Uri uri, SocketIOOptions options)
        {
            ServerUri = uri ?? throw new ArgumentNullException("uri");
            Options = options ?? throw new ArgumentNullException("options");
            Initialize();
        }

        Uri _serverUri;
        public Uri ServerUri
        {
            get => _serverUri;
            set
            {
                if (_serverUri != value)
                {
                    _serverUri = value;
                    if (value != null && value.AbsolutePath != "/")
                    {
                        Namespace = value.AbsolutePath;
                    }
                }
            }
        }

        /// <summary>
        /// An unique identifier for the socket session. Set after the connect event is triggered, and updated after the reconnect event.
        /// </summary>
        public string Id { get; set; }

        public string Namespace { get; private set; }

        /// <summary>
        /// Whether or not the socket is connected to the server.
        /// </summary>
        public bool Connected { get; private set; }

        /// <summary>
        /// Whether or not the socket is Connecting.
        /// </summary>
        public bool Connecting { get; private set; }

        /// <summary>
        /// Gets current attempt of reconnection.
        /// </summary>
        public int Attempts { get; private set; }

        /// <summary>
        /// Whether or not the socket is disconnected from the server.
        /// </summary>
        public bool Disconnected => !Connected;

        public SocketIOOptions Options { get; }

        public IJsonSerializer JsonSerializer { get; set; }

        public IUriConverter UriConverter { get; set; }

        public Func<IClientWebSocket> ClientWebSocketProvider { get; set; }
        private IClientWebSocket _clientWebsocket;

        public BaseTransport _transport;

        List<Type> _expectedExceptions;

        int _packetId;
        bool _isConnectCoreRunning;
        Uri _realServerUri;
        Exception _connectCoreException;
        Dictionary<int, Action<SocketIOResponse>> _ackHandlers;
        List<OnAnyHandler> _onAnyHandlers;
        Dictionary<string, Action<SocketIOResponse>> _eventHandlers;
        double _reconnectionDelay;

        #region Socket.IO event
        public event EventHandler OnConnected;
        //public event EventHandler<string> OnConnectError;
        //public event EventHandler<string> OnConnectTimeout;
        public event EventHandler<string> OnError;
        public event EventHandler<string> OnDisconnected;

        /// <summary>
        /// Fired upon a successful reconnection.
        /// </summary>
        public event EventHandler<int> OnReconnected;

        /// <summary>
        /// Fired upon an attempt to reconnect.
        /// </summary>
        public event EventHandler<int> OnReconnectAttempt;

        /// <summary>
        /// Fired upon a reconnection attempt error.
        /// </summary>
        public event EventHandler<Exception> OnReconnectError;

        /// <summary>
        /// Fired when couldn’t reconnect within reconnectionAttempts
        /// </summary>
        public event EventHandler OnReconnectFailed;
        public event EventHandler OnPing;
        public event EventHandler<TimeSpan> OnPong;

        #endregion

        #region Observable Event
        //Subject<Unit> _onConnected;
        //public IObservable<Unit> ConnectedObservable { get; private set; }
        #endregion

        private void Initialize()
        {
            _packetId = -1;
            _ackHandlers = new Dictionary<int, Action<SocketIOResponse>>();
            _eventHandlers = new Dictionary<string, Action<SocketIOResponse>>();
            _onAnyHandlers = new List<OnAnyHandler>();

            JsonSerializer = new SystemTextJsonSerializer();
            UriConverter = new UriConverter();

            ClientWebSocketProvider = () => new WebGLClientWebSocket(Options.EIO);
            _expectedExceptions = new List<Type>
            {
                typeof(TimeoutException),
                typeof(WebSocketException),
                typeof(OperationCanceledException),
            };
        }

        private void CreateTransportAsync()
        {

            _clientWebsocket = ClientWebSocketProvider();
            _transport = new WebSocketTransport(_clientWebsocket, Options, JsonSerializer);
            _transport.Namespace = Namespace;
        }

        private void SyncExceptionToMain(Exception e)
        {
            _connectCoreException = e;
            _isConnectCoreRunning = false;
        }

        private async Task ConnectCoreAsync()
        {
            DisposeForReconnect();
            var lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            _isConnectCoreRunning = true;
            _connectCoreException = null;

            Connecting = true;
            while (true)
            {
                if (Attempts > 0)
                {
                    await Task.Yield();
                    System.Threading.Thread.Sleep(200);

                    var now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    if (now - lastTime < _reconnectionDelay)
                    {
                        continue;
                    }else
                    {
                        lastTime = now;
                    }
                }

                _clientWebsocket?.Dispose();
                _transport?.Dispose();
                CreateTransportAsync();
                _realServerUri = UriConverter.GetServerUri(Options.Transport == TransportProtocol.WebSocket, ServerUri, Options.EIO, Options.Path, Options.Query);
                try
                {
                    if (Attempts > 0)
                        OnReconnectAttempt?.Invoke(this, Attempts);
                    _transport.Subscribe(OnMessageReceived, OnErrorReceived);

                    _transport.ConnectAsync(_realServerUri);
                    break;
                }
                catch (Exception e)
                {
                    if (_expectedExceptions.Contains(e.GetType()))
                    {
                        if (!Options.Reconnection)
                        {
                            SyncExceptionToMain(e);
                            throw;
                        }
                        if (Attempts > 0)
                        {
                            OnReconnectError?.Invoke(this, e);
                        }
                        Attempts++;
                        if (Attempts <= Options.ReconnectionAttempts)
                        {
                            if (_reconnectionDelay < Options.ReconnectionDelayMax)
                            {
                                _reconnectionDelay += 2000 * Options.RandomizationFactor;
                            }
                            if (_reconnectionDelay > Options.ReconnectionDelayMax)
                            {
                                _reconnectionDelay = Options.ReconnectionDelayMax;
                            }
                        }
                        else
                        {
                            OnReconnectFailed?.Invoke(this, EventArgs.Empty);
                            break;
                        }
                    }
                    else
                    {
                        SyncExceptionToMain(e);
                        throw;
                    }
                }
            }
            _isConnectCoreRunning = false;
        }

        public void ConnectAsync()
        {
            _reconnectionDelay = Options.ReconnectionDelay;
            _ = ConnectCoreAsync();
        }

        private void PingHandler()
        {
            OnPing?.Invoke(this, EventArgs.Empty);
        }

        private void PongHandler(PongMessage msg)
        {
            OnPong?.Invoke(this, msg.Duration);
        }

        private void ConnectedHandler(ConnectedMessage msg)
        {
            Id = msg.Sid;
            Connected = true;
            OnConnected?.Invoke(this, EventArgs.Empty);
            if (Attempts > 0)
            {
                OnReconnected?.Invoke(this, Attempts);
            }
            Attempts = 0;
        }

        private void DisconnectedHandler()
        {
            InvokeDisconnect(DisconnectReason.IOServerDisconnect);
        }

        private void EventMessageHandler(EventMessage m)
        {
            var res = new SocketIOResponse(m.JsonElements, this)
            {
                PacketId = m.Id
            };
            foreach (var item in _onAnyHandlers)
            {
                try
                {
                    item(m.Event, res);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            if (_eventHandlers.ContainsKey(m.Event))
            {
                try
                {
                    _eventHandlers[m.Event](res);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        private void AckMessageHandler(ClientAckMessage m)
        {
            if (_ackHandlers.ContainsKey(m.Id))
            {
                var res = new SocketIOResponse(m.JsonElements, this);
                try
                {
                    _ackHandlers[m.Id](res);
                }
                finally
                {
                    _ackHandlers.Remove(m.Id);
                }
            }
        }

        private void ErrorMessageHandler(ErrorMessage msg)
        {
            OnError?.Invoke(this, msg.Message);
        }

        private void BinaryMessageHandler(BinaryMessage msg)
        {
            if (_eventHandlers.ContainsKey(msg.Event))
            {
                try
                {
                    var response = new SocketIOResponse(msg.JsonElements, this)
                    {
                        PacketId = msg.Id
                    };
                    response.InComingBytes.AddRange(msg.IncomingBytes);
                    _eventHandlers[msg.Event](response);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        private void BinaryAckMessageHandler(ClientBinaryAckMessage msg)
        {
            if (_ackHandlers.ContainsKey(msg.Id))
            {
                try
                {
                    var response = new SocketIOResponse(msg.JsonElements, this)
                    {
                        PacketId = msg.Id,
                    };
                    response.InComingBytes.AddRange(msg.IncomingBytes);
                    _ackHandlers[msg.Id](response);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        private void OnErrorReceived(Exception ex)
        {
            InvokeDisconnect(DisconnectReason.TransportClose);
        }

        private void OnMessageReceived(IMessage msg)
        {
            try
            {
                switch (msg.Type)
                {
                    case MessageType.Ping:
                        PingHandler();
                        break;
                    case MessageType.Pong:
                        PongHandler(msg as PongMessage);
                        break;
                    case MessageType.Connected:
                        ConnectedHandler(msg as ConnectedMessage);
                        break;
                    case MessageType.Disconnected:
                        DisconnectedHandler();
                        break;
                    case MessageType.EventMessage:
                        EventMessageHandler(msg as EventMessage);
                        break;
                    case MessageType.AckMessage:
                        AckMessageHandler(msg as ClientAckMessage);
                        break;
                    case MessageType.ErrorMessage:
                        ErrorMessageHandler(msg as ErrorMessage);
                        break;
                    case MessageType.BinaryMessage:
                        BinaryMessageHandler(msg as BinaryMessage);
                        break;
                    case MessageType.BinaryAckMessage:
                        BinaryAckMessageHandler(msg as ClientBinaryAckMessage);
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public void DisconnectAsync()
        {
            if (Connected)
            {
                var msg = new DisconnectedMessage
                {
                    Namespace = Namespace
                };
                try
                {
                    _transport.SendAsync(msg);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
                InvokeDisconnect(DisconnectReason.IOClientDisconnect);
            }
        }

        /// <summary>
        /// Register a new handler for the given event.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="callback"></param>
        public void On(string eventName, Action<SocketIOResponse> callback)
        {
            if (_eventHandlers.ContainsKey(eventName))
            {
                _eventHandlers.Remove(eventName);
            }
            _eventHandlers.Add(eventName, callback);
        }



        /// <summary>
        /// Unregister a new handler for the given event.
        /// </summary>
        /// <param name="eventName"></param>
        public void Off(string eventName)
        {
            if (_eventHandlers.ContainsKey(eventName))
            {
                _eventHandlers.Remove(eventName);
            }
        }

        public void OffAllEvent()
        {
            _eventHandlers.Clear();
        }

        public void OnAny(OnAnyHandler handler)
        {
            if (handler != null)
            {
                _onAnyHandlers.Add(handler);
            }
        }

        public void PrependAny(OnAnyHandler handler)
        {
            if (handler != null)
            {
                _onAnyHandlers.Insert(0, handler);
            }
        }

        public void OffAny(OnAnyHandler handler)
        {
            if (handler != null)
            {
                _onAnyHandlers.Remove(handler);
            }
        }

        public OnAnyHandler[] ListenersAny() => _onAnyHandlers.ToArray();

        internal void ClientAckAsync(int packetId, params object[] data)
        {
            IMessage msg;
            if (data != null && data.Length > 0)
            {
                var result = JsonSerializer.Serialize(data);
                if (result.Bytes.Count > 0)
                {
                    msg = new ServerBinaryAckMessage
                    {
                        Id = packetId,
                        Namespace = Namespace,
                        Json = result.Json
                    };
                    msg.OutgoingBytes = new List<byte[]>(result.Bytes);
                }
                else
                {
                    msg = new ServerAckMessage
                    {
                        Namespace = Namespace,
                        Id = packetId,
                        Json = result.Json
                    };
                }
            }
            else
            {
                msg = new ServerAckMessage
                {
                    Namespace = Namespace,
                    Id = packetId
                };
            }
            _transport.SendAsync(msg);
        }

        /// <summary>
        /// Emits an event to the socket
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="data">Any other parameters can be included. All serializable datastructures are supported, including byte[]</param>
        /// <returns></returns>
        public void EmitAsync(string eventName, params object[] data)
        {
            if (data != null && data.Length > 0)
            {
                var result = JsonSerializer.Serialize(data);
                if (result.Bytes.Count > 0)
                {
                    var msg = new BinaryMessage
                    {
                        Namespace = Namespace,
                        OutgoingBytes = new List<byte[]>(result.Bytes),
                        Event = eventName,
                        Json = result.Json
                    };
                    _transport.SendAsync(msg);
                }
                else
                {
                    var msg = new EventMessage
                    {
                        Namespace = Namespace,
                        Event = eventName,
                        Json = result.Json
                    };
                    _transport.SendAsync(msg);
                }
            }
            else
            {
                var msg = new EventMessage
                {
                    Namespace = Namespace,
                    Event = eventName
                };
                _transport.SendAsync(msg);
            }
        }

        /// <summary>
        /// Emits an event to the socket
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="ack">will be called with the server answer.</param>
        /// <param name="data">Any other parameters can be included. All serializable datastructures are supported, including byte[]</param>
        /// <returns></returns>
        public void EmitAsync(string eventName, Action<SocketIOResponse> ack, params object[] data)
        {
            _ackHandlers.Add(++_packetId, ack);
            if (data != null && data.Length > 0)
            {
                var result = JsonSerializer.Serialize(data);
                if (result.Bytes.Count > 0)
                {
                    var msg = new ClientBinaryAckMessage
                    {
                        Event = eventName,
                        Namespace = Namespace,
                        Json = result.Json,
                        Id = _packetId,
                        OutgoingBytes = new List<byte[]>(result.Bytes)
                    };
                    _transport.SendAsync(msg);
                }
                else
                {
                    var msg = new ClientAckMessage
                    {
                        Event = eventName,
                        Namespace = Namespace,
                        Id = _packetId,
                        Json = result.Json
                    };
                    _transport.SendAsync(msg);
                }
            }
            else
            {
                var msg = new ClientAckMessage
                {
                    Event = eventName,
                    Namespace = Namespace,
                    Id = _packetId
                };
                _transport.SendAsync(msg);
            }
        }

        private void InvokeDisconnect(string reason)
        {
            if (Connected || Connecting)
            {
                Connected = false;
                Connecting = false;
                OnDisconnected?.Invoke(this, reason);
                try
                {
                    _transport.DisconnectAsync();
                }
                catch { }
                if (reason != DisconnectReason.IOServerDisconnect && reason != DisconnectReason.IOClientDisconnect)
                {
                    //In the this cases (explicit disconnection), the client will not try to reconnect and you need to manually call socket.connect().
                    if (Options.Reconnection)
                    {
                        Attempts++;
                        if (Attempts <= Options.ReconnectionAttempts)
                        {
                            if (_reconnectionDelay < Options.ReconnectionDelayMax)
                            {
                                _reconnectionDelay += 2000 * Options.RandomizationFactor;
                            }
                            if (_reconnectionDelay > Options.ReconnectionDelayMax)
                            {
                                _reconnectionDelay = Options.ReconnectionDelayMax;
                            }
                        }
                        _ = ConnectCoreAsync();
                    }
                }
            }
        }

        public void AddExpectedException(Type type)
        {
            if (!_expectedExceptions.Contains(type))
            {
                _expectedExceptions.Add(type);
            }
        }

        private void DisposeForReconnect()
        {
            _packetId = -1;
            _ackHandlers.Clear();
        }

        public void Dispose()
        {
            _transport.Dispose();
            _ackHandlers.Clear();
            _onAnyHandlers.Clear();
            _eventHandlers.Clear();
        }
    }
}
/* 
 * Copyright(2023) Beijing Volcano Engine Technology Ltd.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 *
 *   http://www.apache.org/licenses/LICENSE-2.0 
 *
 * Unless required by applicable law or agreed to in writing, software 
 * distributed under the License is distributed on an "AS IS" BASIS, 
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
 * See the License for the specific language governing permissions and 
 * limitations under the License.
 */

using System;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Reactive.Subjects;

// Use plugin namespace
using HybridWebSocket;
using UnityEngine.UI;
using WebSocket = HybridWebSocket.WebSocket;
using WebSocketState = HybridWebSocket.WebSocketState;
using StarkSDKSpace;

namespace SocketIOClient.Transport
{
    public class WebGLClientWebSocket : IClientWebSocket
    {
        public WebGLClientWebSocket(int eio)
        {
            _eio = eio;
            _textSubject = new Subject<string>();
            TextSubject = _textSubject;

            StarkSDK.API.GetStarkAppLifeCycle().onAppShow += () =>
            {
                _isAppShow = true;
            };

            StarkSDK.API.GetStarkAppLifeCycle().onAppHide += () =>
            {
                _isAppShow = false;
            };
        }


        const int ReceiveChunkSize = 1024 * 8;

        WebSocket _ws;
        readonly int _eio;
        readonly Subject<string> _textSubject;
        static private bool _isAppShow = true;

        public Subject<string> TextSubject { get; }

        public void ConnectAsync(Uri uri)
        {
            if (!_isAppShow)
            {
                throw new HybridWebSocket.WebSocketException("can not use webSocket on background");
            }else
            {
                _ws = WebSocketFactory.CreateInstance(uri.ToString());

                _ws.OnOpen += () =>
                {
                    Debug.Log("WS connected!");
                };

                _ws.Connect();

                _ws.OnMessage += (byte[] msg) =>
                {
                    string text = Encoding.UTF8.GetString(msg);
                    Debug.Log("WS OnMessage: " + text);
                    _textSubject.OnNext(text);
                };

                // Add OnError event listener
                _ws.OnError += (string errMsg) =>
                {
                    Debug.Log("WS error: " + errMsg);
                    _textSubject.OnError(new HybridWebSocket.WebSocketException(errMsg));
                    _ws = null;
                };

                // Add OnClose event listener
                _ws.OnClose += (WebSocketCloseCode code) =>
                {
                    Debug.Log("WS closed with code: " + code.ToString());
                    _textSubject.OnError(new HybridWebSocket.WebSocketException("Received a Close message"));
                    _ws = null;
                };
            }
        }

        public void DisconnectAsync()
        {
            _ws.Close();
            _ws = null;
        }

        public void SendAsync(byte[] bytes)
        {
            if (_ws == null)
            {
                throw new Exception("WebSocket is null");
            }
            else if (!_isAppShow)
            {
                throw new HybridWebSocket.WebSocketException("can not use webSocket on background");
            }
            else
            {
                Debug.Log("WebSocket SendAsync:" + Encoding.UTF8.GetString(bytes));
                _ws.Send(bytes);
            }
        }

        public void Dispose()
        {
            _textSubject.Dispose();
            _ws = null;
        }
    }
}
/* MIT License
 *
 * Copyright (c) 2021 itisnajim
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
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;
using System.Text.Json;

using Debug = System.Diagnostics.Debug;

public class SocketManager
{
    public SocketIOUnity Socket;

    //初始化Socket，建立连接
    public void Connect(Action callback)
    {
        var uri = new Uri(Constants.LOGIN_URL);
        Socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Path = "/vc_control",
            Query = new Dictionary<string, string>
                {
                    {"appid", "veRTCDemo" },
                    {"ua", "web-3.17.7" },
                    {"did", DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()}
                },
            EIO = 3
            ,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        Socket.JsonSerializer = new NewtonsoftJsonSerializer();

        ///// reserved socketio events
        Socket.OnConnected += (sender, e) =>
        {
            UnityEngine.Debug.Log("socket.OnConnected");

            if (callback != null)
            {
                Socket.ExecuteInUnityThreadIfNeeded(callback);
                callback = null;
            }
        };

        Socket.OnPing += (sender, e) =>
        {
            UnityEngine.Debug.Log("Ping");
        };
        Socket.OnPong += (sender, e) =>
        {
            UnityEngine.Debug.Log("Pong: " + e.TotalMilliseconds);
        };
        Socket.OnDisconnected += (sender, e) =>
        {
            UnityEngine.Debug.Log("disconnect: " + e);
        };
        Socket.OnReconnectAttempt += (sender, e) =>
        {
            UnityEngine.Debug.Log($"{DateTime.Now} Reconnecting: attempt = {e}");
        };
        Socket.OnError += (sender, e) =>
        {
            UnityEngine.Debug.Log("socket error: " + e);
        };

        UnityEngine.Debug.Log("Connecting...");
        Socket.ConnectAsync();
    }
}
//
//  SocketIOUnity.cs
//  SocketIOUnity
//
//  Created by itisnajim on 10/30/2021.
//  Copyright (c) 2021 itisnajim. All rights reserved.
//
//  This file may have been modified by Beijing Volcano Engine Technology Ltd.
//  (“ Volcano Engine's Modifications”). All Volcano Engine's Modifications are
//  Copyright (2023) Beijing Volcano Engine Technology Ltd..


using System;
using System.Threading;
using System.Threading.Tasks;
using SocketIOClient;
using SocketIOClient.Messages;

public class SocketIOUnity : SocketIO
{
    public enum UnityThreadScope
    {
        Update,
        LateUpdate,
        FixedUpdate
    }
    public UnityThreadScope unityThreadScope = UnityThreadScope.Update;

    public SocketIOUnity(string uri, UnityThreadScope unityThreadScope = UnityThreadScope.Update) : base(uri)
    {
        CommonInit(unityThreadScope);
    }

    public SocketIOUnity(Uri uri, UnityThreadScope unityThreadScope = UnityThreadScope.Update) : base(uri)
    {
        CommonInit(unityThreadScope);
    }

    public SocketIOUnity(string uri, SocketIOOptions options, UnityThreadScope unityThreadScope = UnityThreadScope.Update) : base(uri, options)
    {
        CommonInit(unityThreadScope);
    }

    public SocketIOUnity(Uri uri, SocketIOOptions options, UnityThreadScope unityThreadScope = UnityThreadScope.Update) : base(uri, options)
    {
        CommonInit(unityThreadScope);
    }

    private void CommonInit(UnityThreadScope unityThreadScope)
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        UnityThread.initUnityThread();
        this.unityThreadScope = unityThreadScope;
#endif
    }

#if !UNITY_WEBGL || UNITY_EDITOR
    /// <summary>
    /// Register a new handler for the given event.
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="callback"></param>
    public void OnUnityThread(string eventName, Action<SocketIOResponse> callback)
    {
        On(eventName, res =>
        {
            ExecuteInUnityThread(() => callback(res));
        });

    }

    public void OnAnyInUnityThread(OnAnyHandler handler)
    {
        OnAny((name, response) =>
        {
            ExecuteInUnityThread(() => handler(name, response));
        });
    }
#endif

    /// <summary>
    /// Emits an event to the socket
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="data">Any other parameters can be included. All serializable datastructures are supported, including byte[]</param>
    /// <returns></returns>
    public void Emit(string eventName, params object[] data)
    {
        EmitAsync(eventName, data);
    }


    public void EmitStringAsJSONAsync(string eventName, string json)
    {

        var msg = new EventMessage
        {
            Namespace = Namespace,
            Event = eventName,
        };
        if (!string.IsNullOrEmpty(json))
        {
            msg.Json = "["+json+"]";
        }
        _transport.SendAsync(msg);
    }

    public void EmitStringAsJSON(string eventName, string json)
    {
        EmitStringAsJSONAsync(eventName, json);
    }

    public void Connect()
    {
        ConnectAsync();
    }

    public void Disconnect()
    {
        DisconnectAsync();
    }

    public void ExecuteInUnityThreadIfNeeded(Action action)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        action();
#else
        ExecuteInUnityThread(action);
#endif
    }

#if !UNITY_WEBGL || UNITY_EDITOR
    private void ExecuteInUnityThread(Action action)
    {
        switch (unityThreadScope)
        {
            case UnityThreadScope.LateUpdate :
                UnityThread.executeInLateUpdate(action);
                break;
            case UnityThreadScope.FixedUpdate :
                UnityThread.executeInFixedUpdate(action);
                break;
            default :
                UnityThread.executeInUpdate(action);
                break;
        }
    }
#endif
}

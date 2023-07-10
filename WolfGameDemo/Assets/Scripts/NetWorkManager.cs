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
using System.Collections;
using System.Collections.Generic;
using SocketIOClient;
using UnityEngine;

public class NetWorkManager
{
    private SocketManager _socketManager = new SocketManager();

    private static volatile NetWorkManager _instance;
    private static object _lock = new object();


    public static NetWorkManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new NetWorkManager();
                }
            }
            return _instance;
        }
    }

    private NetWorkManager()
    {
    }

    //发起请求
    //eventName 事件名
    //ack T返回数据对应解析类型，bool是否请求成功，int错误码
    //data 请求数据(class或者Dictionary)
    private void _request<T>(string eventName, Action<ResponseRoot<T>, bool, int> ack, params object[] data)
    {
        _socketManager.Socket.EmitAsync(eventName, (response) =>
        {
            ResponseRoot<T> responseRoot = response.GetValue<ResponseRoot<T>>();

            _socketManager.Socket.ExecuteInUnityThreadIfNeeded(() =>
            {
                ack(responseRoot, responseRoot.code == 200 && (responseRoot.response != null), responseRoot.code);
            });
        }, data);
    }

    //接收服务端下发通知
    //eventName 事件名
    //ack T返回数据对应解析类型，bool是否成功
    private void _on<T>(string eventName, Action<NoticeRoot<T>, bool> ack)
    {
        _socketManager.Socket.On(eventName, (notice) =>
        {
            NoticeRoot<T> noticeRoot = notice.GetValue<NoticeRoot<T>>();

            _socketManager.Socket.ExecuteInUnityThreadIfNeeded(() =>
            {
                ack(noticeRoot, noticeRoot.data != null);
            });
        });
    }

    //当前是否连接中
    public bool AlreadyConnected
    {
        get
        {
            if (_socketManager.Socket != null)
            {
                return _socketManager.Socket.Connecting;
            }else
            {
                return false;
            }
        }
    }

    //发起连接
    //callback 成功回调
    public void Connect(Action callback)
    {
        _socketManager.Connect(callback);
    }

    //添加连接中断监听
    //handler 连接中断回调
    public void OnDisconnected(EventHandler<string> handler)
    {
        _socketManager.Socket.OnDisconnected += handler;
    }

    //移除连接中断监听
    public void OffDisconnected(EventHandler<string> handler)
    {
        _socketManager.Socket.OnDisconnected -= handler;
    }

    //接口请求返回Response根数据对应的class
    [System.Serializable]
    private class ResponseRoot<T>
    {
        public int code;
        public string message;
        public long timestamp;

        public T response;

        public ResponseRoot(T response)
        {
            this.response = response;
        }
    }

    //接口下发通知Notice根数据对应的class
    [System.Serializable]
    private class NoticeRoot<T>
    {
        public long timestamp;

        public T data;

        public NoticeRoot(T data)
        {
            this.data = data;
        }
    }

    //房间数据对应的class
    [System.Serializable]
    public class RoomObject
    {
        public long id;
        public string app_id;
        public string room_id;
        public string room_name;
        public string host_user_id;
        public string host_user_name;

        public int status;
        public int game_status; //1：等待准备 2：正在开始 3：天黑 4：天亮 5：结束
        public int user_count;
        public string create_time;
        public string update_time;
    }

    //User数据对应的class
    [System.Serializable]
    public class UserObject
    {
        public string app_id;
        public string room_id;
        public string user_id;
        public string user_name;

        public int net_status;
        public int game_status; //1：未准备 2：已准备 3：游戏中
        public int room_role;   //1：房主 2：普通用户
        public int game_role;   //1：平民 2：狼人
        public string conn_id;
        public string create_time;
        public string update_time;
    }

    //请求数据添加app_id字段
    private void requestDataAddAppID(Dictionary<string, string> requestData)
    {
        if (Constants.APP_ID.Length > 0)
        {
            requestData.Add("app_id", Constants.APP_ID);
        }
    }

    [System.Serializable]
    public class SetAppInfoResponse
    {
    }

    //请求wolfSetAppInfo接口
    //ack 请求回调
    public void SetAppInfo(Action<SetAppInfoResponse, bool> ack)
    {
        var setAppInfoRequestData = new Dictionary<string, string>
        {
            {"app_id", Constants.APP_ID },
            {"app_key", Constants.APP_KEY }
        };

        _request<SetAppInfoResponse>("wolfSetAppInfo", (response, isSuccess, code) =>
        {
            ack(response.response, isSuccess);
        }, setAppInfoRequestData);
    }

    //获取房间列表返回数据对应class
    [System.Serializable]
    public class GetRoomListRoomResponse
    {
        public List<RoomObject> room_list;
    }

    //获取房间列表
    public void GetRoomList(Action<GetRoomListRoomResponse, bool> ack)
    {
        var getRoomListRequestData = new Dictionary<string, string>();
        requestDataAddAppID(getRoomListRequestData);

        _request<GetRoomListRoomResponse>("wolfGetRoomList", (response, isSuccess, code) =>
        {
            ack(response.response, isSuccess);
        }, getRoomListRequestData);
    }

    //创建房间返回数据对应class
    [System.Serializable]
    public class CreateRoomResponse
    {
        public RoomObject room;
        public UserObject user;
        public string rtc_token;
    }

    //创建房间
    public void CreateRoom(string roomName, string userName, Action<CreateRoomResponse, bool, int> ack)
    {
        var createRoomRequestData = new Dictionary<string, string>
        {
            {"room_name", roomName },
            {"user_name", userName }
        };
        requestDataAddAppID(createRoomRequestData);

        _request<CreateRoomResponse>("wolfCreateRoom", (response, isSuccess, code) =>
        {
            ack(response.response, isSuccess, code);
        }, createRoomRequestData);
    }

    //加入房间返回数据对应class
    [System.Serializable]
    public class JoinRoomResponse
    {
        public List<UserObject> user_list;

        public RoomObject room;
        public UserObject user;
        public string rtc_token;
    }

    //加入房间
    public void JoinRoom(string roomid, string userName, Action<JoinRoomResponse, bool, int> ack)
    {
        var joinRoomRequestData = new Dictionary<string, string>
        {
            {"room_id", roomid },
            {"user_name", userName }
        };
        requestDataAddAppID(joinRoomRequestData);

        _request<JoinRoomResponse>("wolfJoinRoom", (response, isSuccess, code) =>
        {
            ack(response.response, isSuccess, code);
        }, joinRoomRequestData);
    }

    [System.Serializable]
    public class LeaveRoomResponse
    {
    }

    //离开房间
    public void LeaveRoom(string roomid, string userid, Action<LeaveRoomResponse, bool> ack)
    {
        var leaveRoomRequestData = new Dictionary<string, string>
        {
            {"room_id", roomid },
            {"user_id", userid }
        };
        requestDataAddAppID(leaveRoomRequestData);

        _request<LeaveRoomResponse>("wolfLeaveRoom", (response, isSuccess, code) =>
        {
            ack(response.response, isSuccess);
        }, leaveRoomRequestData);
    }

    //请求数据class基类
    [System.Serializable]
    public class BaseRequestData
    {
        public string app_id;
    }

    //请求数据准备游戏class
    [System.Serializable]
    public class PrepareGameRequest : BaseRequestData
    {
        public string room_id;
        public string user_id;
        public int game_status;
    }


    [System.Serializable]
    public class PrepareGameResponse
    {
    }

    //准备游戏
    public void PrepareGame(string roomid, string userid, bool isPrepare , Action<PrepareGameResponse, bool> ack)
    {
        var prepareGameRequestData = new PrepareGameRequest
        {
            app_id = Constants.APP_ID,
            room_id = roomid,
            user_id = userid,
            game_status = isPrepare ? 2 : 1
        };

        _request<PrepareGameResponse>("wolfChangeUserGameStatus", (response, isSuccess, code) =>
        {
            ack(response.response, isSuccess);
        }, prepareGameRequestData);
    }

    //请求数据开始游戏class
    [System.Serializable]
    public class StartGameResponse
    {
        public List<UserObject> user_list;
    }

    //开始游戏
    public void StartGame(string roomid, string userid, Action<StartGameResponse, bool> ack)
    {
        var startGameRequestData = new Dictionary<string, string>
        {
            {"room_id", roomid },
            {"user_id", userid }
        };
        requestDataAddAppID(startGameRequestData);

        _request<StartGameResponse>("wolfStartGame", (response, isSuccess, code) =>
        {
            ack(response.response, isSuccess);
        }, startGameRequestData);
    }

    //通知数据加入房间class
    [System.Serializable]
    public class OnJoinRoomResponse
    {
        public string room_id;
        public UserObject user;
    }

    //用户进房通知
    public void OnJoinRoomNotification(Action<OnJoinRoomResponse, bool> ack)
    {
        _on<OnJoinRoomResponse>("wolfOnJoinRoom", (response, isSuccess) =>
        {
            ack(response.data, isSuccess);
        });
    }

    //通知数据离开房间class
    [System.Serializable]
    public class OnLeaveRoomResponse
    {
        public string room_id;
        public UserObject user;
    }

    //用户退房通知
    public void OnLeaveRoomNotification(Action<OnLeaveRoomResponse, bool> ack)
    {
        _on<OnLeaveRoomResponse>("wolfOnLeaveRoom", (response, isSuccess) =>
        {
            ack(response.data, isSuccess);
        });
    }

    //通知数据关闭房间class
    [System.Serializable]
    public class OnCloseRoomResponse
    {
        public string room_id;
        public int type;
    }

    //用户关闭房间通知
    public void OnCloseRoomNotification(Action<OnCloseRoomResponse, bool> ack)
    {
        _on<OnCloseRoomResponse>("wolfOnCloseRoom", (response, isSuccess) =>
        {
            ack(response.data, isSuccess);
        });
    }

    //通知数据准备游戏class
    [System.Serializable]
    public class OnPrepareGameResponse
    {
        public string room_id;
        public string user_id;
        public int game_status; //1：未准备 2：已准备
        public bool can_start;
    }

    //用户准备游戏通知
    public void OnPrepareGameNotification(Action<OnPrepareGameResponse, bool> ack)
    {
        _on<OnPrepareGameResponse>("wolfOnChangeUserGameStatus", (response, isSuccess) =>
        {
            ack(response.data, isSuccess);
        });
    }

    //通知数据开始游戏class
    [System.Serializable]
    public class OnStartGameResponse
    {
        public string room_id;
        public List<UserObject> user_list;
    }

    //开始游戏通知
    public void OnStartGameNotification(Action<OnStartGameResponse, bool> ack)
    {
        _on<OnStartGameResponse>("wolfOnStartGame", (response, isSuccess) =>
        {
            ack(response.data, isSuccess);
        });
    }

    //通知数据游戏状态改变class
    [System.Serializable]
    public class OnChangeGameStatusResponse
    {
        public string room_id;
        public int game_status;
    }

    //移除所有通知监听
    public void RemoveAllNotification()
    {
        _socketManager.Socket.OffAllEvent();
    }

    //游戏阶段改变通知
    public void OnChangeGameStatusNotification(Action<OnChangeGameStatusResponse, bool> ack)
    {
        _on<OnChangeGameStatusResponse>("wolfOnChangeGameStatus", (response, isSuccess) =>
        {
            ack(response.data, isSuccess);
        });
    }

    //通知数据玩家发言class
    [System.Serializable]
    public class OnUserSpeakResponse
    {
        public string room_id;
        public string user_id;
    }

    //玩家发言通知
    public void OnUserSpeakNotification(Action<OnUserSpeakResponse, bool> ack)
    {
        _on<OnUserSpeakResponse>("wolfOnUserSpeak", (response, isSuccess) =>
        {
            ack(response.data, isSuccess);
        });
    }

}

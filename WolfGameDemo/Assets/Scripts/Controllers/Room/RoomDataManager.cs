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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameStatus
{
    Ready,
    Start,
    ShowCard,
    Night,
    WolfSpeaking,
    DayTime,
    SpeakingTurn
}

public enum RoleType
{
    Person = 1,
    Wolf = 2
}

public class RoomDataManager
{
    private static volatile RoomDataManager _instance;
    private static object _lock = new object();

    public NetWorkManager.RoomObject RoomData;

    public string MyUserid;

    public List<NetWorkManager.UserObject> UserList = new List<NetWorkManager.UserObject>();

    public string RtcToken;

    public bool IsHost = false;

    public bool IsReady = false;

    public bool CanStart = false;

    public GameStatus GameStatus = GameStatus.Ready;

    public RoleType RoleType;

    public string SpeakingTurnsUserid;   //天亮后轮到发言的用户id

    public System.Action OnUserJoinRoom;

    public static RoomDataManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new RoomDataManager();
                }
            }
            return _instance;
        }
    }

    private RoomDataManager()
    {
    }

    //清除数据
    public void ClearData()
    {
        RoomData = null;
        MyUserid = null;
        UserList.Clear();
        RtcToken = null;
        IsHost = false;
        IsReady = false;
        CanStart = false;
        GameStatus = GameStatus.Ready;
        SpeakingTurnsUserid = null;
    }

    //设置创建房间数据
    public void SetCreateRoomData(NetWorkManager.RoomObject roomData, NetWorkManager.UserObject userData, string rtcToken)
    {
        RoomData = roomData;
        MyUserid = userData.user_id;
        RtcToken = rtcToken;
        IsHost = true;

        UserList.Clear();
        UserList.Add(userData);
    }

    //设置加入房间数据
    public void SetJoinRoomData(NetWorkManager.RoomObject roomData, string userid, List<NetWorkManager.UserObject> userList, string rtcToken)
    {
        RoomData = roomData;
        MyUserid = userid;
        RtcToken = rtcToken;
        IsHost = false;

        UserList = userList;
    }

    //注册加入房间事件
    public void RegistJoinRoomEvent()
    {
        NetWorkManager.Instance.OnJoinRoomNotification((notice, isSuccess) =>
        {
            if (!isSuccess)
            {
                return;
            }
            AddUserData(notice.user);

            if (OnUserJoinRoom != null)
            {
                OnUserJoinRoom();
            }
        });
    }

    //添加user
    public void AddUserData(NetWorkManager.UserObject addedUserData)
    {
        for (int i = 0; i < UserList.Count; i++)
        {
            NetWorkManager.UserObject userObject = UserList[i];
            if (userObject.user_id.Equals(addedUserData.user_id))
            {
                UserList.Remove(userObject);
                break;
            }
        }
        UserList.Add(addedUserData);
    }

    //移除user
    public void RemoveUserData(NetWorkManager.UserObject removedUserData)
    {
        CanStart = false;

        for (int i = 0; i < UserList.Count; i++)
        {
            NetWorkManager.UserObject userObject = UserList[i];
            if (userObject.user_id.Equals(removedUserData.user_id))
            {
                UserList.Remove(userObject);
                return;
            }
        }
    }

    //设置user ready
    public void SetUserReady(string userid, bool isReady, bool canStart)
    {
        CanStart = canStart;

        if (userid.Equals(MyUserid))
            IsReady = isReady;

        for (int i = 0; i < UserList.Count; i++)
        {
            NetWorkManager.UserObject userObject = UserList[i];
            if (userObject.user_id.Equals(userid))
            {
                userObject.game_status = isReady ? 2 : 1;
                return;
            }
        }
    }

    //设置游戏开始数据
    public void SetGameStartData(List<NetWorkManager.UserObject> userList)
    {
        GameStatus = GameStatus.Start;

        UserList = userList;
        for (int i = 0; i < UserList.Count; i++)
        {
            NetWorkManager.UserObject userObject = UserList[i];
            if (userObject.user_id.Equals(MyUserid))
            {
                RoleType = (RoleType)userObject.game_role;
                return;
            }
        }
    }

    //设置玩家开始发言
    //到发言轮次的userid
    public void SetSpeakingTurnsUserid(string userid)   
    {
        SpeakingTurnsUserid = userid;
    }
}

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
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using StarkSDKSpace;
using UnityEngine.EventSystems;

public class ButtonTouchEventTrigger : EventTrigger
{
    public Action OnPressDown;
    public Action OnPressUp;

    public override void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPressDown");

        if (OnPressDown != null)
        {
            OnPressDown();
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("OnPressUp");

        if (OnPressUp != null)
        {
            OnPressUp();
        }
    }
}

public class WolfRoomController : MonoBehaviour
{
    public GameObject TopImage;
    public GameObject MiddleImage;
    public GameObject BottomImage;

    public Text TitleText;

    public PlayerController[] PlayerControllers = new PlayerController[3];

    public GameObject MicBack;
    public VolumeBarController BarController;

    public Text TimeCountText;

    public Button SpeakingButton;
    public Button ReadyButton;

    public GameObject GameStartPanel;
    public GameObject WolfCardPanel;
    public GameObject PersonCardPanel;
    public GameObject DayTipPanel;
    public GameObject NightTipPanel;
    public GameObject WolfAtNightTip;
    public GameObject PersonAtNightTip;
    public GameObject CloseRoomToast;

    private Sprite backgroundDayTopRes;
    private Sprite backgroundDayMiddleRes;
    private Sprite backgroundNightTopRes;
    private Sprite backgroundNightMiddleRes;
    private Sprite startButtonRes;
    private Sprite readyButtonRes;
    private Sprite cancelButtonRes;

    private int timeCount;

    private RTCEngineManager.RtcEngine RtcEngine;

    // Start is called before the first frame update
    void Start()
    {
        backgroundDayTopRes = Resources.Load<Sprite>("dayBackGround");
        backgroundDayMiddleRes = Resources.Load<Sprite>("dayContentBackGround");
        backgroundNightTopRes = Resources.Load<Sprite>("nightBackGround");
        backgroundNightMiddleRes = Resources.Load<Sprite>("nightContentBackGround");
        startButtonRes = Resources.Load<Sprite>("start");
        readyButtonRes = Resources.Load<Sprite>("ready");
        cancelButtonRes = Resources.Load<Sprite>("cancel");

        initRTCEngine();

        RegistRoomEvent();

        addSpeakingButtonEvent();

        refreshPageUI();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //初始化RTCEngine并加入房间
    private void initRTCEngine()
    {
        var appid = RoomDataManager.Instance.RoomData.app_id;
        var roomid = RoomDataManager.Instance.RoomData.room_id;
        var userid = RoomDataManager.Instance.MyUserid;
        var token = RoomDataManager.Instance.RtcToken;

        StarkSDK.Init();
        // 获取引擎
        RtcEngine = StarkSDK.API.GetRTCEngineManager().GetRtcEngine(appid);

        // 注册监听
        RtcEngine.OnJoinChannelSuccessEvent += OnJoinChannelSuccess;
        RtcEngine.OnUserJoinedEvent += OnUserJoined;
        RtcEngine.OnUserOfflineEvent += OnUserOffline;
        RtcEngine.OnUserMuteAudioEvent += OnUserMuteAudio;
        RtcEngine.OnWarningEvent += OnWarning;
        RtcEngine.OnErrorEvent += OnError;
        RtcEngine.OnAudioVolumeIndicationEvent += OnAudioVolumeIndication;

        // 加入房间
        RtcEngine.JoinChannel(roomid, userid, token, () => { }, (code, reason) => { });
    }

    //刷新整个页面UI
    private void refreshPageUI()
    {
        refreshBackgroundUI();
        refreshPlayerUI();
        refreshBottomUI();
        refreshTipsUI();

        TitleText.text = "房间名：" + RoomDataManager.Instance.RoomData.room_name;
    }

    //刷新背景UI
    private void refreshBackgroundUI()
    {
        var dataManager = RoomDataManager.Instance;
        bool isNight = dataManager.GameStatus == GameStatus.Night || dataManager.GameStatus == GameStatus.WolfSpeaking;

        if (!isNight)
        {
            TopImage.GetComponent<Image>().sprite = backgroundDayTopRes;
            MiddleImage.GetComponent<Image>().sprite = backgroundDayMiddleRes;
            BottomImage.GetComponent<Image>().color = new Color(0.886f, 0.922f, 0.957f, 1);
        }
        else
        {
            TopImage.GetComponent<Image>().sprite = backgroundNightTopRes;
            MiddleImage.GetComponent<Image>().sprite = backgroundNightMiddleRes;
            BottomImage.GetComponent<Image>().color = new Color(0.922f, 0.918f, 0.958f, 1);
        }
    }

    //刷新TipsUI
    private void refreshTipsUI()
    {
        var status = RoomDataManager.Instance.GameStatus;

        GameStartPanel.SetActive(false);
        WolfCardPanel.SetActive(false);
        PersonCardPanel.SetActive(false);
        DayTipPanel.SetActive(false);
        NightTipPanel.SetActive(false);
        WolfAtNightTip.SetActive(false);
        PersonAtNightTip.SetActive(false);
        TimeCountText.gameObject.SetActive(false);

        switch (status)
        {
            case GameStatus.Start:
                GameStartPanel.SetActive(true);
                break;
            case GameStatus.ShowCard:
                if (RoomDataManager.Instance.RoleType == RoleType.Wolf)
                {
                    WolfCardPanel.SetActive(true);
                }else
                {
                    PersonCardPanel.SetActive(true);
                }
                break;
            case GameStatus.Night:
                NightTipPanel.SetActive(true);
                break;
            case GameStatus.WolfSpeaking:
                TimeCountText.gameObject.SetActive(true);
                if (RoomDataManager.Instance.RoleType == RoleType.Wolf)
                {
                    WolfAtNightTip.SetActive(true);
                }
                else
                {
                    PersonAtNightTip.SetActive(true);
                }
                break;
            case GameStatus.DayTime:
                DayTipPanel.SetActive(true);
                break;
            case GameStatus.SpeakingTurn:
                TimeCountText.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }

    //刷新玩家UI
    private void refreshPlayerUI()
    {
        for (int i = 0; i < 3; i++)
        {
            PlayerController playerController = PlayerControllers[i];
            if (i >= RoomDataManager.Instance.UserList.Count)
            {
                playerController.RefreshUIWithSeat();
                continue;
            }

            NetWorkManager.UserObject userObject = RoomDataManager.Instance.UserList[i];
            playerController.RefreshUIWithUserData(userObject);
        }
    }

    //刷新底部UI
    private void refreshBottomUI()
    {
        if (RoomDataManager.Instance.GameStatus == GameStatus.Ready)
        {
            ReadyButton.gameObject.SetActive(true);

            if (RoomDataManager.Instance.IsHost)
            {
                ReadyButton.GetComponent<Image>().sprite = startButtonRes;
                ReadyButton.interactable = RoomDataManager.Instance.CanStart;
            }else
            {
                if (RoomDataManager.Instance.IsReady)
                {
                    ReadyButton.GetComponent<Image>().sprite = cancelButtonRes;
                }else
                {
                    ReadyButton.GetComponent<Image>().sprite = readyButtonRes;
                }
            }
        }
        else
        {
            ReadyButton.gameObject.SetActive(false);

            SpeakingButton.gameObject.SetActive(false);
        }
    }

    //注册房间事件
    private void RegistRoomEvent()
    {
        NetWorkManager.Instance.OnDisconnected(OnDisconnected);

        RoomDataManager.Instance.OnUserJoinRoom = () =>
        {
            refreshPlayerUI();
        };

        NetWorkManager.Instance.OnLeaveRoomNotification((notice, isSuccess) =>
        {
            if (!isSuccess)
            {
                return;
            }
            RoomDataManager.Instance.RemoveUserData(notice.user);

            refreshPlayerUI();
            refreshBottomUI();
        });

        NetWorkManager.Instance.OnCloseRoomNotification((notice, isSuccess) =>
        {
            if (!isSuccess)
            {
                return;
            }

            CloseRoomToast.SetActive(true);
            Invoke("leaveRoom", 2.0f);
        });

        NetWorkManager.Instance.OnPrepareGameNotification((notice, isSuccess) =>
        {
            if (!isSuccess)
            {
                return;
            }
            RoomDataManager.Instance.SetUserReady(notice.user_id, notice.game_status == 2, notice.can_start);

            refreshPlayerUI();
            refreshBottomUI();
        });

        NetWorkManager.Instance.OnStartGameNotification((notice, isSuccess) =>
        {
            if (!isSuccess)
            {
                return;
            }
            RoomDataManager.Instance.SetGameStartData(notice.user_list);
            RoomDataManager.Instance.GameStatus = GameStatus.Start;

            refreshPageUI();
            addShowCardTrigger();
        });

        NetWorkManager.Instance.OnChangeGameStatusNotification((notice, isSuccess) =>
        {
            if (!isSuccess)
            {
                return;
            }

            GameStatus status = GameStatus.Ready;
            switch (notice.game_status)
            {
                case 1:
                    status = GameStatus.Ready;
                    break;
                case 2:
                    status = GameStatus.Start;
                    break;
                case 3:
                    status = GameStatus.Night;
                    addWolfSpeakingTrigger();
                    break;
                case 4:
                    status = GameStatus.DayTime;
                    break;
                case 5:
                    break;
            }

            changeGameStatus(status);
        });

        NetWorkManager.Instance.OnUserSpeakNotification((notice, isSuccess) =>
        {
            if (!isSuccess)
            {
                return;
            }
            RoomDataManager.Instance.SetSpeakingTurnsUserid(notice.user_id);

            changeGameStatus(GameStatus.SpeakingTurn);

            addSpeakingTurnTimer();
        });
    }

    //网络断开回调事件
    private void OnDisconnected(object sender, string e)
    {
        leaveRoom();
    }

    //按下说话按钮事件
    private void addSpeakingButtonEvent()
    {
        var touchEventTrigger = SpeakingButton.GetComponent<ButtonTouchEventTrigger>();
        if (touchEventTrigger == null)
        {
            touchEventTrigger = SpeakingButton.gameObject.AddComponent<ButtonTouchEventTrigger>();
        }

        touchEventTrigger.OnPressDown = startSpeaking;
        touchEventTrigger.OnPressUp = endSpeaking;
    }

    //开始发言
    private void startSpeaking()
    {
        // 开启音频发送
        RtcEngine.UnMuteLocalAudioStream();
        MicBack.SetActive(true);
    }

    //结束发言
    private void endSpeaking()
    {
        // 关闭音频发送
        RtcEngine.MuteLocalAudioStream();
        MicBack.SetActive(false);
    }

    //游戏阶段改变
    private void changeGameStatus(GameStatus gameStatus)
    {
        RoomDataManager.Instance.GameStatus = gameStatus;

        var dataManager = RoomDataManager.Instance;

        if (dataManager.GameStatus == GameStatus.WolfSpeaking && dataManager.RoleType == RoleType.Person)
        {
            RtcEngine.MuteAllRemoteAudioStream();
        }
        else
        {
            RtcEngine.UnMuteAllRemoteAudioStream();
        }

        if (dataManager.GameStatus == GameStatus.SpeakingTurn && dataManager.SpeakingTurnsUserid.Equals(dataManager.MyUserid))
        {
            RtcEngine.UnMuteLocalAudioStream();
            MicBack.SetActive(true);
        }
        else if (dataManager.GameStatus == GameStatus.WolfSpeaking && dataManager.RoleType == RoleType.Wolf)
        {
            RtcEngine.UnMuteLocalAudioStream();
            MicBack.SetActive(true);
        }
        else
        {
            RtcEngine.MuteLocalAudioStream();
            MicBack.SetActive(false);
        }

        refreshBackgroundUI();
        refreshBottomUI();
        refreshTipsUI();
    }

    //显示身份牌
    private void addShowCardTrigger()
    {
        StartCoroutine(actionWithDelayTime(1.0f, () =>
        {
            if (RoomDataManager.Instance.GameStatus == GameStatus.Start)
            {
                changeGameStatus(GameStatus.ShowCard);
            }
        }));
    }

    //显示天黑提示
    private void addWolfSpeakingTrigger()
    {
        StartCoroutine(actionWithDelayTime(3.0f, () =>
        {
            if (RoomDataManager.Instance.GameStatus == GameStatus.Night)
            {
                changeGameStatus(GameStatus.WolfSpeaking);
                addWolfSpeakingTimer();
            }
        }));
    }

    //启动狼人发言倒计时
    private void addWolfSpeakingTimer()
    {
        timeCount = 15;
        InvokeRepeating("updateWolfSpeakingString", 0, 1.0f);
    }

    //更新狼人发言倒计时
    private void updateWolfSpeakingString()
    {
        if (RoomDataManager.Instance.GameStatus != GameStatus.WolfSpeaking)
        {
            TimeCountText.text = "";
            CancelInvoke("updateWolfSpeakingString");
            return;
        }

        TimeCountText.text = "狼人正在发言中...(倒计时 " + timeCount + "s 秒)";

        timeCount--;
        if (timeCount <= 0)
        {
            TimeCountText.text = "";
            CancelInvoke("updateWolfSpeakingString");
        }
    }

    //启动轮流发言倒计时
    private void addSpeakingTurnTimer()
    {
        timeCount = 15;
        CancelInvoke("updateSpeakingTurnString");
        InvokeRepeating("updateSpeakingTurnString", 0, 1.0f);
    }

    //更新轮流发言倒计时
    private void updateSpeakingTurnString()
    {
        var dataManager = RoomDataManager.Instance;
        if (dataManager.GameStatus != GameStatus.SpeakingTurn)
        {
            CancelInvoke("updateSpeakingTurnString");
            return;
        }

        if (dataManager.SpeakingTurnsUserid.Equals(dataManager.MyUserid))
        {
            TimeCountText.text = "我正在发言中...(倒计时 " + timeCount + "s 秒)";
        }else
        {
            for (int i = 0; i < dataManager.UserList.Count; i++)
            {
                NetWorkManager.UserObject userObject = dataManager.UserList[i];
                if (userObject.user_id.Equals(dataManager.SpeakingTurnsUserid))
                {
                    TimeCountText.text = (i + 1) + "号正在发言中...(倒计时 " + timeCount + "s 秒)";
                    break;
                }
            }
        }

        timeCount--;
        if (timeCount <= 0)
        {
            CancelInvoke("updateSpeakingTurnString");
        }
    }

    private IEnumerator actionWithDelayTime(float time, Action action)
    {
        yield return new WaitForSeconds(time);
        action();
    }

    //退出RTC房间
    private void leaveRoom()
    {
        RtcEngine.OnJoinChannelSuccessEvent -= OnJoinChannelSuccess;
        RtcEngine.OnUserJoinedEvent -= OnUserJoined;
        RtcEngine.OnUserOfflineEvent -= OnUserOffline;
        RtcEngine.OnUserMuteAudioEvent -= OnUserMuteAudio;
        RtcEngine.OnWarningEvent -= OnWarning;
        RtcEngine.OnErrorEvent -= OnError;
        RtcEngine.OnAudioVolumeIndicationEvent -= OnAudioVolumeIndication;

        // 关闭音频发送
        RtcEngine.MuteLocalAudioStream();
        // 关闭音频采集
        RtcEngine.DisableLocalAudio();
        // 离开房间
        RtcEngine.LeaveChannel();

        NetWorkManager.Instance.OffDisconnected(OnDisconnected);
        NetWorkManager.Instance.RemoveAllNotification();
        RoomDataManager.Instance.ClearData();
        RoomDataManager.Instance.OnUserJoinRoom = null;

        SceneManager.LoadScene("RoomListScene");
    }

    // 点击返回按钮
    public void OnClickReturnButton()
    {
        string roomid = RoomDataManager.Instance.RoomData.room_id;
        string userid = RoomDataManager.Instance.MyUserid;
        NetWorkManager.Instance.LeaveRoom(roomid, userid, (response, isSuccess) =>
        {
            if (!isSuccess)
            {
                return;
            }
            leaveRoom();
        });
    }

    // 点击准备按钮
    public void OnClickReadyButton()
    {
        string roomid = RoomDataManager.Instance.RoomData.room_id;
        string userid = RoomDataManager.Instance.MyUserid;
        if (RoomDataManager.Instance.IsHost)
        {
            NetWorkManager.Instance.StartGame(roomid, userid, (response, isSuccess) =>
            {

            });
        }else
        {
            bool isReady = RoomDataManager.Instance.IsReady;
            NetWorkManager.Instance.PrepareGame(roomid, userid, !isReady, (response, isSuccess) =>
            {

            });
        }
    }

    // 加入房间成功回调
    private void OnJoinChannelSuccess(int elapsed)
    {
        Debug.Log(string.Format("OnJoinChannelSuccess"));

        // 开启音频采集
        RtcEngine.EnableLocalAudio();
        // 关闭音频发送
        RtcEngine.MuteLocalAudioStream();
        // 开启音量回调
        RtcEngine.EnableAudioVolumeIndication(1000, null, null);
    }

    // 用户加房回调
    private void OnUserJoined(string uid, int elapsed)
    {
        Debug.Log(string.Format("OnUserJoined, uid: {0}", uid));
    }

    // 用户离房回调
    private void OnUserOffline(string uid, string reason)
    {
        Debug.Log(string.Format("OnUserOffline, uid: {0}", uid));
    }

    // 远端用户音频流发送状态变化回调
    private void OnUserMuteAudio(string uid, bool muted)
    {
        Debug.Log(string.Format("OnUserMuteAudio, uid: {0}, muted: {1}", uid, muted));
    }

    // 音量回调
    private void OnAudioVolumeIndication(string uid, int volume, int speakerNum)
    {
        Debug.Log(string.Format("OnAudioVolumeIndication"));

        if (RoomDataManager.Instance.MyUserid == uid)
        {
            BarController.UpdateVolume(volume, 255);
            return;
        }

        for (int i = 0; i < RoomDataManager.Instance.UserList.Count; i++)
        {
            if (i > 2)
            {
                return;
            }

            NetWorkManager.UserObject user = RoomDataManager.Instance.UserList[i];
            if (user.user_id != uid)
            {
                continue;
            }

            var playerController = PlayerControllers[i];
            playerController.SpeakingImage.SetActive(volume > 0);
            playerController.BarController.UpdateVolume(volume, 255);
            return;
        }
    }

    // 警告回调，详细可以看 {https://www.volcengine.com/docs/6348/70082#warncode}
    private void OnWarning(int warnCode)
    {
        Debug.Log(string.Format("OnWarning, warnCode: {0}", warnCode));
    }

    // 错误回调，详细可以看 {https://www.volcengine.com/docs/6348/70082#errorcode}
    private void OnError(int errCode)
    {
        Debug.Log(string.Format("OnError, errorCode: {0}", errCode));
        //if (errCode == -1004)
        //{
        //    WarnDialog?.SetActive(true);
        //}
    }
}

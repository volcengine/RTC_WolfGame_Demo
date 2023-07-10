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
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using StarkSDKSpace;

public class RoomListController : MonoBehaviour
{
    public Button CreateRoomButton;
    public Button RefreshListButton;
    public GameObject ListItemPrefab;
    public GameObject ListContent;
    public GameObject InputNamePannel;
    public InputField UserNameInput;
    public Button JoinRoomButton;
    public GameObject JoinRoomTipPannel;
    public Text JoinRoomTipText;

    private string selectedRoomId;

    //自定义输入框
    private class JoinRoomInputField : ClickableInputField
    {
        public RoomListController RoomListController;

        public override void OnKeyboardInput(string value)
        {
            var inputField = GetComponent<InputField>();

            Debug.Log($"OnKeyboardInput: {value}");
            if (inputField.isFocused)
            {
                inputField.text = value;
            }

            bool invalid = inputField.text.Length > 0;
            RoomListController.JoinRoomButton.interactable = invalid;
        }
    }

    void Start()
    {
        if (NetWorkManager.Instance.AlreadyConnected)
        {
            NetWorkManager.Instance.GetRoomList((response, isSuccess) =>
            {
                if (!isSuccess)
                {
                    return;
                }
                Debug.Log("response: " + JsonUtility.ToJson(response));
                BuildRoomListUI(response.room_list);
            });
        }else
        {
            CreateRoomButton.enabled = false;
            NetWorkManager.Instance.Connect(() =>
            {
                NetWorkManager.Instance.SetAppInfo((response, isSuccess) =>
                {
                    CreateRoomButton.enabled = true;
                    NetWorkManager.Instance.GetRoomList((response, isSuccess) =>
                    {
                        if (!isSuccess)
                        {
                            return;
                        }
                        BuildRoomListUI(response.room_list);
                    });
                });
            });
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        JoinRoomButton.interactable = false;
#endif
    }

    //设置输入框参数
    private void SetInputTexts()
    {
        UserNameInput.caretWidth = 0;
        var userNameInputComp = UserNameInput.GetComponent<JoinRoomInputField>();
        if (userNameInputComp == null)
        {
            userNameInputComp = UserNameInput.gameObject.AddComponent<JoinRoomInputField>();
            userNameInputComp.RoomListController = this;
        }
        userNameInputComp.Multiple = false;
        userNameInputComp.ConfirmType = "done";
        userNameInputComp.MaxInputLength = 10;
    }

    //点击加入房间按钮
    public void OnBtnJoinRoom()
    {
        if (!checkNameIsValid())
        {
            showTips("请输入1-10位字母或数字");
            return;
        }

        NetWorkManager.Instance.JoinRoom(selectedRoomId, UserNameInput.text, (response, isSuccess, code) =>
        {
            if (isSuccess)
            {
                RoomDataManager.Instance.SetJoinRoomData(response.room, response.user.user_id, response.user_list, response.rtc_token);
                SceneManager.LoadScene("WolfRoomScene");
            }else
            {
                if (code == 485)
                {
                    showTips("昵称已存在，请重新输入");
                }
                else if (code == 406)
                {
                    showTips("房间已满员");
                }
                else if (code == 422)
                {
                    showTips("房间不存在");
                }
            }
        });

        RoomDataManager.Instance.RegistJoinRoomEvent();
    }

    //点击创建房间按钮
    public void OnBtnCreateRoom()
    {
        SceneManager.LoadScene("CreateRoomScene");
    }

    //点击刷新按钮
    public void OnBtnRefreshList()
    {
        NetWorkManager.Instance.GetRoomList((response, isSuccess) =>
        {
            BuildRoomListUI(response.room_list);
        });
    }

    //点击返回按钮
    public void OnBtnReturn()
    {
        InputNamePannel.SetActive(false);

        StarkSDK.API.GetStarkKeyboard().HideKeyboard();
    }

    //更新房间列表UI
    private void BuildRoomListUI(List<NetWorkManager.RoomObject> roomList)
    {
        ClearChild(ListContent);
        if (roomList == null)
            return;

        foreach (NetWorkManager.RoomObject room in roomList)
        {
            GameObject item = GameObject.Instantiate(ListItemPrefab);
            item.transform.SetParent(ListContent.transform);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
            item.transform.localScale = Vector3.one;
            item.transform.Find("RoomTitle").gameObject.GetComponent<Text>().text = String.Format("房间名称：{0}", room.room_name);
            item.transform.Find("HostHeader/Text").gameObject.GetComponent<Text>().text = room.host_user_name.Substring(0,1);
            item.transform.Find("HostHeader/HostName").gameObject.GetComponent<Text>().text = room.host_user_name;
            item.transform.Find("RoomNumText").gameObject.GetComponent<Text>().text = String.Format("房间人数：{0}",room.user_count);
            item.transform.Find("RoomIDText").gameObject.GetComponent<Text>().text = String.Format("ID：{0}", room.room_id);
            item.transform.Find("ItemButton").gameObject.GetComponent<Button>().onClick.AddListener(() => OnSelectRoom(room));
        }
            
    }

    //点击选择房间
    private void OnSelectRoom(NetWorkManager.RoomObject room)
    {
        selectedRoomId = room.room_id;

        InputNamePannel.SetActive(true);
        SetInputTexts();    //必须在OnSelectRoom方法里调用SetInputTexts，避免出现切后台导致的AddComponent不成功的情况。
    }

    //检查姓名是否合法
    private bool checkNameIsValid()
    {
        System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9]+$");
        return regex.IsMatch(UserNameInput.text);
    }

    //显示Tips
    private void showTips(string content)
    {
        CancelInvoke("hideTips");

        JoinRoomTipPannel.SetActive(true);
        JoinRoomTipText.text = content;

        Invoke("hideTips", 2.0f);
    }

    //隐藏Tips
    private void hideTips()
    {
        JoinRoomTipPannel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //移除房间列表UI
    private void ClearChild(GameObject gameObject)
    {
        Transform transform;
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            transform = gameObject.transform.GetChild(i);
            GameObject.Destroy(transform.gameObject);
        }
    }
}

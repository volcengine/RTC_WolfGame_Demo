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

public class CreateRoomController : MonoBehaviour
{
    public InputField RoomNameInput;
    public InputField UserNameInput;
    public Button CreateRoomButton;
    public GameObject CreateRoomTipPannel;
    public Text CreateRoomTipText;

    //自定义输入框
    private class CreateRoomInputField : ClickableInputField
    {
        public CreateRoomController RoomController;

        public override void OnKeyboardInput(string value)
        {
            var inputField = GetComponent<InputField>();

            Debug.Log($"OnKeyboardInput: {value}");
            if (inputField.isFocused)
            {
                inputField.text = value;
            }

            bool invalid = RoomController.RoomNameInput.text.Length > 0 && RoomController.UserNameInput.text.Length > 0;
            RoomController.CreateRoomButton.interactable = invalid;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        CreateRoomButton.interactable = false;
#endif
        SetInputTexts();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //设置输入框参数
    private void SetInputTexts()
    {
        RoomNameInput.caretWidth = 0;
        var roomNameInputComp = RoomNameInput.GetComponent<CreateRoomInputField>();
        if (roomNameInputComp == null)
        {
            roomNameInputComp = RoomNameInput.gameObject.AddComponent<CreateRoomInputField>();
            roomNameInputComp.RoomController = this;
        }
        roomNameInputComp.Multiple = false;
        roomNameInputComp.ConfirmType = "done";
        roomNameInputComp.MaxInputLength = 10;

        UserNameInput.caretWidth = 0;
        var userNameInputComp = UserNameInput.GetComponent<CreateRoomInputField>();
        if (userNameInputComp == null)
        {
            userNameInputComp = UserNameInput.gameObject.AddComponent<CreateRoomInputField>();
            userNameInputComp.RoomController = this;
        }
        userNameInputComp.Multiple = false;
        userNameInputComp.ConfirmType = "done";
        userNameInputComp.MaxInputLength = 10;
    }

    //点击创建房间
    public void OnBtnCreateRoom()
    {
        if (!checkNameIsValid())
        {
            showTips("请输入1-10位字母或数字");
            return;
        }

        NetWorkManager.Instance.CreateRoom(RoomNameInput.text, UserNameInput.text, (response, isSuccess, code) =>
        {
            if (isSuccess)
            {
                RoomDataManager.Instance.SetCreateRoomData(response.room, response.user, response.rtc_token);

                SceneManager.LoadScene("WolfRoomScene");
            }else
            {
                if (code == 485)
                {
                    showTips("昵称已存在，请重新输入");
                }
                else if (code == 486)
                {
                    showTips("房间号已存在，请重新输入");
                }
                else if (code == 487)
                {
                    showTips("昵称和房间号已存在，请重新输入");
                }
            }
        });
        RoomDataManager.Instance.RegistJoinRoomEvent();
    }

    //点击返回按钮
    public void OnBtnReturn()
    {
        SceneManager.LoadScene("RoomListScene");
    }

    //检查姓名是否合法
    private bool checkNameIsValid()
    {
        System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9]+$");
        return regex.IsMatch(UserNameInput.text) && regex.IsMatch(RoomNameInput.text);
    }

    //显示Tips
    private void showTips(string content)
    {
        CancelInvoke("hideTips");

        CreateRoomTipPannel.SetActive(true);
        CreateRoomTipText.text = content;

        Invoke("hideTips", 2.0f);
    }

    //隐藏Tips
    private void hideTips()
    {
        CreateRoomTipPannel.SetActive(false);
    }
}

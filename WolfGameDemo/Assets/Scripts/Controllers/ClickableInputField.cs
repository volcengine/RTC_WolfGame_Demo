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
using StarkSDKSpace;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 可点击弹出软键盘
public class ClickableInputField : EventTrigger
{
    public string ConfirmType = "done"; // 可选值有: "done", "next", "search", "go", "send"
    public int MaxInputLength = 12; // 最大输入长度
    public bool Multiple = false; // 是否多行输入
    private InputField _inputField;
    private void Start()
    {
        _inputField = GetComponent<InputField>();

        RegisterKeyboardEvents();
    }

    private void OnDestroy()
    {
        StarkSDK.API.GetStarkKeyboard().HideKeyboard();
        UnregisterKeyboardEvents();
    }

    //注册StarkSDK的键盘回调事件
    private void RegisterKeyboardEvents()
    {
        StarkSDK.API.GetStarkKeyboard().onKeyboardInputEvent += OnKeyboardInput;
        StarkSDK.API.GetStarkKeyboard().onKeyboardConfirmEvent += OnKeyboardConfirm;
        StarkSDK.API.GetStarkKeyboard().onKeyboardCompleteEvent += OnKeyboardComplete;
    }

    //移除StarkSDK的键盘回调事件
    private void UnregisterKeyboardEvents()
    {
        StarkSDK.API.GetStarkKeyboard().onKeyboardInputEvent -= OnKeyboardInput;
        StarkSDK.API.GetStarkKeyboard().onKeyboardConfirmEvent -= OnKeyboardConfirm;
        StarkSDK.API.GetStarkKeyboard().onKeyboardCompleteEvent -= OnKeyboardComplete;
    }

    //键盘输入事件
    public virtual void OnKeyboardInput(string value)
    {
        Debug.Log($"OnKeyboardInput: {value}");
        if (_inputField.isFocused)
        {
            _inputField.text = value;
        }
    }

    //键盘确认事件
    public virtual void OnKeyboardConfirm(string value)
    {
        Debug.Log($"OnKeyboardConfirm: {value}");
    }

    //键盘完成事件
    public virtual void OnKeyboardComplete(string value)
    {
        Debug.Log($"OnKeyboardComplete: {value}");
    }

    //控件点击事件
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (_inputField != null)
        {
            if (_inputField.isFocused)
            {
                StarkSDK.API.GetStarkKeyboard().ShowKeyboard(new StarkKeyboard.ShowKeyboardOptions()
                {
                    maxLength = MaxInputLength,
                    multiple = Multiple,
                    defaultValue = _inputField.text,
                    confirmType = ConfirmType
                });
            }
        }
    }
}

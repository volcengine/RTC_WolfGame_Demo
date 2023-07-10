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

public class PlayerController : MonoBehaviour
{
    public GameObject SeatImage;
    public GameObject PlayerPanel;

    public GameObject CardImage;
    public GameObject ReadyImage;
    public Text NameText;
    public GameObject SpeakingImage;

    public VolumeBarController BarController;

    private Sprite wolfCardRes;
    private Sprite personCardRes;
    // Start is called before the first frame update
    void Start()
    {
        wolfCardRes = Resources.Load<Sprite>("wolf");
        personCardRes = Resources.Load<Sprite>("person");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //刷新UI
    public void RefreshUIWithUserData(NetWorkManager.UserObject userData)
    {
        SeatImage.SetActive(false);
        PlayerPanel.SetActive(true);

        switch (userData.game_status)
        {
            case 1:
                CardImage.SetActive(false);
                ReadyImage.SetActive(false);
                break;
            case 2:
                CardImage.SetActive(false);
                ReadyImage.SetActive(true);
                break;
            case 3:
                if (userData.game_role == (int)RoleType.Person)
                {
                    CardImage.GetComponent<Image>().sprite = personCardRes;
                }else
                {
                    CardImage.GetComponent<Image>().sprite = wolfCardRes;
                }
                CardImage.SetActive(true);
                ReadyImage.SetActive(false);
                break;
        }

        string userName = userData.user_name;
        bool isHost = userData.room_role == 1;
        bool isMyself = RoomDataManager.Instance.MyUserid.Equals(userData.user_id);
        if (isHost && isMyself)
        {
            NameText.text = "[房主](我)" + userName;
        }else if (isHost)
        {
            NameText.text = "[房主]" + userName;
        }else if (isMyself)
        {
            NameText.text = "(我)" + userName;
        }else
        {
            NameText.text = userName;
        }
    }

    //刷新座位UI
    public void RefreshUIWithSeat()
    {
        SeatImage.SetActive(true);
        PlayerPanel.SetActive(false);
    }


}

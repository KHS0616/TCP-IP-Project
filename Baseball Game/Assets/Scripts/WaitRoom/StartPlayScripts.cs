using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartPlayScripts : MonoBehaviour
{
    //유저 정보, 클라이언트 불러올 변수 선언
    UserInfo userInfo;
    Client client;

    //이벤트 처리 버튼 선언
    public Button button;

    //방 정보를 가져올 변수 선언
    WaitRoomScripts waitRoomScripts;

    // Start is called before the first frame update
    void Start()
    {
        userInfo = FindObjectOfType<UserInfo>();
        client = FindObjectOfType<Client>();
        button.onClick.AddListener(onClickStartPlay);
        waitRoomScripts = FindObjectOfType<WaitRoomScripts>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onClickStartPlay()
    {

        //현재 유저의 방장 유무 확인
        int slotNum = 0;
        for (int i = 0; i < 4; i++)
        {
            if (userInfo.userRoom.userID[i] != null)
            {
                if (userInfo.userRoom.userID[i].Equals(userInfo.GetUserID()))
                {
                    slotNum = i;
                    break;
                }
            }
        }

        //방장인 경우에만 실행
        if (slotNum == 0)
        {
            //현재 유저들의 상태 표시
            for(int i = 0; i < 4; i++)
            {
                if (waitRoomScripts.Ustatus[i].Equals("false"))
                {
                    Debug.Log(i);
                    break;
                }
                //준비 상태가 없을 경우
                if (i == 3)
                {
                    //Json데이터 전송
                    Dictionary<string, string> json = new Dictionary<string, string>
                    {
                        {"type", "startPlay" },
                        {"userID", userInfo.GetUserID() },
                        {"content", userInfo.userRoom.getNo().ToString() },
                        {"content2", "" }
                    };
                    client.OnSendData(JsonConvert.SerializeObject(json, Formatting.Indented));
                }
            }

        }

    }
}

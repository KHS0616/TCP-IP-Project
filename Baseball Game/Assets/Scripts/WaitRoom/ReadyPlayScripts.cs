using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReadyPlayScripts : MonoBehaviour
{
    //유저 정보, 클라이언트 불러올 변수 선언
    UserInfo userInfo;
    Client client;

    //이벤트 처리 버튼 선언
    public Button button;
    // Start is called before the first frame update
    void Start()
    {
        userInfo = FindObjectOfType<UserInfo>();
        client = FindObjectOfType<Client>();
        button.onClick.AddListener(onClickReadyPlay);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onClickReadyPlay()
    {

        //현재 유저의 방장 유무 확인
        int slotNum = 0;
        for(int i = 0; i < 4; i++)
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

        //방장이 아닌 경우에만 실행
        if (slotNum != 0)
        {
            //현재 유저의 상태 표시
            string userStatus = "false";
            if (!userInfo.checkReady)
            {
                userStatus = "true";
            }
            //현재 상태 반전
            userInfo.checkReady = !userInfo.checkReady;
            //JSON 데이터 전송
            Dictionary<string, string> json = new Dictionary<string, string>
            {
                {"type", "readyPlay" },
                {"userID", userInfo.GetUserID() },
                {"content", userInfo.userRoom.getNo().ToString() },
                {"content2", userStatus }
            };
            string sendData = JsonConvert.SerializeObject(json, Formatting.Indented);
            client.OnSendData(sendData);
        }

    }
}

using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExitRoomScript : MonoBehaviour
{
    //나가기 버튼 설정
    public Button button;

    //유저 정보 및 클라이언트 불러오기
    UserInfo userInfo;
    Client client;
    void Start()
    {
        //버튼 리스너 등록
        button.onClick.AddListener(onClickExitButton);

        userInfo = FindObjectOfType<UserInfo>();
        client = FindObjectOfType<Client>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //나가기 버튼 행동
    public void onClickExitButton()
    {
        //Json 데이터 전송
        Dictionary<string, string> json = new Dictionary<string, string>
        {
            {"type", "exitRoom" },
            {"userID", userInfo.GetUserID() },
            {"content", userInfo.userRoom.getNo().ToString() },
            {"content2", "" }
        };
        string sendData = JsonConvert.SerializeObject(json, Formatting.Indented);
        client.OnSendData(sendData);

        //유저가 속한 방 정보 초기화
        userInfo.userRoom = null;
        userInfo.userRoom = new UserInfo.Room();

        //로비로 이동
        SceneManager.LoadScene("MainLobby");
    }
}

﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginEvent : MonoBehaviour
{
    //변수선언
    //아이디, 비밀번호 입력 필드
    public InputField inputID, inputPW;

    //유저 정보 클래스
    public UserInfo user;

    //아이디, 비밀번호 임시 저장 변수
    string id, pw, pwcheck;

    //테스트용 변수
    public Text text;

    private Button loginbtn;

    //서버 접속용 클라이언트 변수
    Client client = null;

    //받은 데이터를 임시 저장할 변수
    public static string receivedData = "";
    public static string preReceivedData = "";
    JObject data;

    // Start is called before the first frame update
    void Start()
    {
        user = FindObjectOfType<UserInfo>();

        loginbtn = GameObject.Find("LoginActionButton").GetComponent<Button>();
        loginbtn.onClick.AddListener(LoginButton);

        //클라이언트 설정
        client = FindObjectOfType<Client>();
    }

    // Update is called once per frame
    void Update()
    {
        //데이터 받아오기
        if (client.receivedData != null && !preReceivedData.Equals(client.receivedData) && Client.checkData == false)
        {
            receivedData = client.receivedData;
            Debug.Log(receivedData);
            receivedData = client.receivedData;
            preReceivedData = receivedData;
            data = JObject.Parse(receivedData);
            processJson(data);
            receivedData = null;
            Client.checkData = true;
        }
    }

    public void LoginButton()   // use LoginScene
    {
        //입력받은 아이디, 비밀번호 변수 저장 및 할당
        id = inputID.text;
        pw = inputPW.text;

        //입력받은 값 JSON 데이터 형태로 서버 전송
        Dictionary<string, string> sendData = new Dictionary<string, string>
        {
            {"type", "login" },
            {"userID", id },
            {"content", pw },
            {"content2", "" }
        };
        string sendString = JsonConvert.SerializeObject(sendData, Formatting.Indented);
        client.OnSendData(sendString);


        user.SetUserInfo(id, pw);

        //테스트
        text.text = id + ", "+pw;
    }

    //JSON 명령에 따른 행동 분기 메소드
    private void processJson(JObject receivedData)
    {
        //타입 저장
        string type = receivedData["type"].ToString();

        //타입에 따른 명령어 분기
        if (type.Equals("login"))
        {
            //본인 아이디인지 비교
            string userID = receivedData["userID"].ToString();
            if (userID.Equals(id))
            {
                //로그인 성공 분기
                if (receivedData["content"].ToString().Equals("success"))
                {
                    text.text = "로그인 성공";
                    SceneManager.LoadScene("MainLobby");
                }
                //로그인 실패 원인에 따른 행동 분기
                else if(receivedData["content"].ToString().Equals("fail"))
                {
                    if (receivedData["content2"].ToString().Equals("id"))
                    {
                        text.text = "존재하지 않는 아이디 입니다.";
                    }
                    else if (receivedData["content2"].ToString().Equals("pw"))
                    {
                        text.text = "비밀번호를 잘못 입력하셨습니다.";
                    }

                }
            }
        }
    }

}

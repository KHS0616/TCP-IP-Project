using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SignUpEvent : MonoBehaviour
{
    public InputField inputID, inputPW, inputPWCheck;

    //유저 정보 클래스
    public UserInfo user;

    //아이디, 비밀번호 임시 저장 변수
    string id, pw, pwcheck;

    //테스트용 변수
    public Text text;

    private Button signUpbtn;

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

        signUpbtn = GameObject.Find("SignUpButton").GetComponent<Button>();
        signUpbtn.onClick.AddListener(SignUpButton);

        //클라이언트 설정
        client = FindObjectOfType<Client>();
    }

    // Update is called once per frame
    void Update()
    {
        //데이터 받아오기
        //기존 데이터와 동일한 데이터일 경우 데이터 실행 X
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
    //회원가입 처리 이벤트
    public void SignUpButton()
    { // use SignUpScene
        id = inputID.text;
        pw = inputPW.text;
        pwcheck = inputPWCheck.text;
        if (pw.Equals(pwcheck))
        {
            //비밀번호 일치할 경우 JSON 데이터 작성
            Dictionary<string, string> sendData = new Dictionary<string, string>
            {
                {"type", "signUp" },
                {"userID", id },
                {"content", pw },
                {"content2", "" }
            };
            string sendString = JsonConvert.SerializeObject(sendData, Formatting.Indented);
            client.OnSendData(sendString);
        }
        else
        {
            text.text = "Please check pw.";
        }
    }

    //JSON 처리 메소드
    private void processJson(JObject receivedData)
    {
        //타입 저장
        string type = receivedData["type"].ToString();

        //타입에 따른 명령어 분기
        if (type.Equals("signUp"))
        {
            //본인 아이디인지 비교
            string userID = receivedData["userID"].ToString();
            if (userID.Equals(id))
            {
                if (receivedData["content"].ToString().Equals("success"))
                {
                    text.text = "회원가입 성공";
                    SceneManager.LoadScene("LoginScene");
                }
                else
                {
                    text.text = "이미 존재하는 아이디 입니다.";
                }
            }
        }
    }
}

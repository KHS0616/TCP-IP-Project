using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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

    //에러메세지 출력 변수
    public Text errtext;

    private Button signUpbtn;

    //서버 접속용 클라이언트 변수
    Client client = null;

    //받은 데이터를 임시 저장할 변수
    public static string receivedData = "";
    public static string preReceivedData = "";
    JObject data;

    bool checkData = true;


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
        if (client.receivedDataList.Count > 0 && checkData)
        {
            checkData = false;
            receivedData = client.receivedDataList.Dequeue();
            preReceivedData = receivedData;
            data = JObject.Parse(receivedData);
            processJson(data);
            receivedData = null;
            Client.checkData = true;
        }
    }

    private bool ValidatePassword(string password)
    {
        var input = password;

        if (string.IsNullOrWhiteSpace(input))
        {
            throw new Exception("Password should not be empty");
        }

        var hasNumber = new Regex(@"[0-9]+");
        var hasUpperChar = new Regex(@"[A-Z]+");
        var hasMiniMaxChars = new Regex(@".{8,16}");
        var hasLowerChar = new Regex(@"[a-z]+");
        var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

        if (!hasLowerChar.IsMatch(input))
        {
            errtext.text = "비밀번호는 소문자가 포함되어야합니다.";
            return false;
        }
        else if (!hasUpperChar.IsMatch(input))
        {
            errtext.text = "비밀번호는 대문자가 포함되어야합니다.";
            return false;
        }
        else if (!hasMiniMaxChars.IsMatch(input))
        {
            errtext.text = "비밀번호는 8~15자 길이어야합니다.";
            return false;
        }
        else if (!hasNumber.IsMatch(input))
        {
            errtext.text = "비밀번호는 숫자가 포함되어야합니다.";
            return false;
        }

        else if (!hasSymbols.IsMatch(input))
        {
            errtext.text = "비밀번호는 특수문자가 포함되어야합니다.";
            return false;
        }
        else
        {
            return true;
        }
    }

    //회원가입 처리 이벤트
    public void SignUpButton()
    { // use SignUpScene
        id = inputID.text;
        pw = inputPW.text;
        pwcheck = inputPWCheck.text;
        if (ValidatePassword(pw))
        {
            if (pw.Equals(pwcheck))
            {
                //비밀번호 암호화
                pw = SHA256Hash(pw);
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
            } else {
                errtext.text = "비밀번호 체크와 값이 다릅니다.";
            }
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
                    errtext.text = "회원가입 성공";
                    SceneManager.LoadScene("LoginScene");
                }
                else
                {
                    errtext.text = "이미 존재하는 아이디 입니다.";
                }
            }
        }

        checkData = true;
    }

    public string SHA256Hash(string data)
    {

        SHA256 sha = new SHA256Managed();
        byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(data));
        StringBuilder stringBuilder = new StringBuilder();
        foreach (byte b in hash)
        {
            stringBuilder.AppendFormat("{0:x2}", b);
        }
        return stringBuilder.ToString();
    }
}

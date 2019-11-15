using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    // Start is called before the first frame update
    void Start()
    {
        user = FindObjectOfType<UserInfo>();

        loginbtn = GameObject.Find("LoginActionButton").GetComponent<Button>();
        loginbtn.onClick.AddListener(LoginButton);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoginButton()   // use LoginScene
    {
        //입력받은 아이디, 비밀번호 변수 저장 및 할당
        id = inputID.text;
        pw = inputPW.text;
        user.SetUserInfo(id, pw);

        //테스트
        text.text = id + ", "+pw;
    }
    
}

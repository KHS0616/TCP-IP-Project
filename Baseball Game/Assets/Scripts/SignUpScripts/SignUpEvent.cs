using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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



    // Start is called before the first frame update
    void Start()
    {
        user = FindObjectOfType<UserInfo>();

        signUpbtn = GameObject.Find("SignUpButton").GetComponent<Button>();
        signUpbtn.onClick.AddListener(SignUpButton);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SignUpButton()
    { // use SignUpScene
        id = inputID.text;
        pw = inputPW.text;
        pwcheck = inputPWCheck.text;
        if (pw.Equals(pwcheck))
        {
            text.text = id + ", " + pw;
        }
        else
        {
            text.text = "Please check pw.";
        }
    }
}

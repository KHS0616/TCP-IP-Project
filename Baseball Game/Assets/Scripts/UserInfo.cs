using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInfo : MonoBehaviour
{
    //변수 설정
    //아이디, 비밀번호
    string id, pw, nic;
    public static UserInfo instance = null;

    // Start is called before the first frame update
    void Start()
    {
        //삭제 방지
        if (instance == null) // 중복방지
        {
            DontDestroyOnLoad(this);
            instance = this;
        }
    }

    public void SetUserInfo(string id, string pw)
    {
        this.id = id;
        this.pw = pw;
    }

    public void SetUserInfo(string id, string pw, string nic)
    {
        this.id = id;
        this.pw = pw;
        this.nic = nic;
    }
}

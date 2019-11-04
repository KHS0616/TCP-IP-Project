using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInfo : MonoBehaviour
{
    //변수 설정
    //아이디, 비밀번호
    string id, pw;

    // Start is called before the first frame update
    void Start()
    {
        //삭제 방지
        DontDestroyOnLoad(this.gameObject);
    }

    public void SetUserInfo(string id, string pw)
    {
        this.id = id;
        this.pw = pw;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatContentList : MonoBehaviour
{
    //로그를 보여줄 TEXT UI
    private Text contentText = null;
    //스크롤 바
    private ScrollRect scroll_rect = null;

    // Start is called before the first frame update
    void Start()
    {
        //Text UI의 Text 컴포넌트 지정
        contentText = GameObject.Find("ChatContentList").GetComponent<Text>();

        //스크롤 바 지정
        scroll_rect = GameObject.Find("ChatContentScroll").GetComponent<ScrollRect>();

        //연결 성공시 인사말 출력
        if(contentText != null)
        {
            contentText.text += "서버 접속에 성공하였습니다." + "\n";
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            contentText.text += "Mouse down Position(" + "X: " + Input.mousePosition.x + " Y: " + Input.mousePosition.y + ")\n";
            scroll_rect.verticalNormalizedPosition = 0.0f;

        }
        
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatContentList : MonoBehaviour
{
    //받은 데이터를 임시 저장할 변수
    public static string receivedData = "";
    public static string preReceivedData = "";
    JObject data;
    Client client;
    UserInfo userInfo;

    //복제될 TEXT UI Object
    public GameObject preText;

    //스크롤 바
    private ScrollRect scroll_rect = null;

    //Text UI의 부모 컴포넌트
    public Transform textParent;

    // textlist의 마지막 오브젝트를 가리킴.
    private GameObject lastText;

    public Scrollbar textScroll;
    bool checkData = true;
    // textList 배열선언
    ArrayList textList = new ArrayList();
    private int i = 0;

    public Text chatContent;
    public Button chatButton;

    // Start is called before the first frame update
    void Start()
    {
        client = FindObjectOfType<Client>();
        userInfo = FindObjectOfType<UserInfo>();
        //Text UI의 Text 컴포넌트 지정
        preText = GameObject.Find("ChatContentList");

        //스크롤 바 지정
        scroll_rect = GameObject.Find("ChatContentScroll").GetComponent<ScrollRect>();

        //Text의 부모 컴포넌트 지정
        textParent = GameObject.Find("Content").GetComponent<Transform>();

        textScroll = GameObject.Find("Scrollbar").GetComponent<Scrollbar>();

        //연결 성공시 인사말 출력
        if (preText != null)
        {
            preText.GetComponentInChildren<Text>().text = "서버 접속에 성공하였습니다.";
        }

        chatButton.onClick.AddListener(onClickInputChat);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void processJson(JObject receivedData)
    {
        //타입 저장
        string type = receivedData["type"].ToString();
        string userID = receivedData["userID"].ToString();

        //타입에 따른 명령어 분기
        //방을 만들 때 행동
        if (type.Equals("inputChat"))
        {
            Debug.Log("dd");
            string content = receivedData["content"].ToString();
            //preText.text += "Mouse down Position(" + "X: " + Input.mousePosition.x + " Y: " + Input.mousePosition.y + ")\n";
            if (lastText == null)
            { // 텍스트가 첫번째라면
                lastText = Instantiate(preText.gameObject) as GameObject;   // text object 복제
                lastText.transform.SetParent(textParent);   // 복제한 text object 의 부모설정
                lastText.transform.localScale = new Vector3(1, 1, 1); // 복제한 text object의 스케일 설정 ( 크기비율 ) 
                lastText.name = "ChatContentList" + (++i);  // 복제한 text object의 이름
                lastText.GetComponentInChildren<Text>().text = userID + ": " + content;
                textList.Add(lastText); // ArrayList 에 object들 추가 [  TODO :  불필요시 삭제 ]
            }
            else // 텍스트가 추가된 후
            {
                lastText = Instantiate(lastText) as GameObject; // 위와 동일
                lastText.transform.SetParent(textParent);
                lastText.transform.localScale = new Vector3(1, 1, 1);
                lastText.name = "ChatContentList" + (++i);
                lastText.GetComponentInChildren<Text>().text = userID + ": " + content;
                textList.Add(lastText);
            }
        }
        //작업 종료후 데이터 처리 완료처리
        checkData = true;
    }

    private void onClickInputChat()
    {
        //입력받은 값 JSON 데이터 형태로 서버 전송
        Dictionary<string, string> sendData = new Dictionary<string, string>
        {
            {"type", "inputChat" },
            {"userID", userInfo.GetUserID() },
            {"content",  chatContent.text},
            {"content2", "" }
        };
        string sendString = JsonConvert.SerializeObject(sendData, Formatting.Indented);
        client.OnSendData(sendString);
    }
}

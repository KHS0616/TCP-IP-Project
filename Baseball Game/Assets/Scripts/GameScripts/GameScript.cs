using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameScript : MonoBehaviour
{
    //유저 정보 불러오기
    UserInfo userInfo;
    public Image[] user = new Image[4];
    public Image[] userStatus = new Image[4];

    //통신용 클라이언트 불러오기
    Client client;

    //보내기 텍스트 & 버튼 
    public InputField sendText;
    public Button sendBtn;
    public Text numView;
    public Text resultView;

    //받은 데이터를 임시 저장할 변수
    public static string receivedData = "";
    public static string preReceivedData = "";
    JObject data;

    int i;

    bool checkData = true;

    //상태 창 이미지
    public Sprite bar_unReady;
    public Sprite bar_Ready;

    //유저 이미지
    public Sprite[] charImage = new Sprite[4];
    public Sprite noneImage;
    public Sprite userKing;
    public Sprite userMe;
    public Sprite userOther;


    // Start is called before the first frame update
    void Start()
    {
        client = FindObjectOfType<Client>();
        userInfo = FindObjectOfType<UserInfo>();

        /*      // Image 속성 위치마다 user 정보가 들어가야 한다면 사용
        foreach(string user in userInfo.GetRoom().getUsers()){

        }
        */
        if (userInfo.GetRoom().getUsers()[0].Equals(userInfo.GetUserID())) // 방장만 게임데이터 송신
        {
            //게임 입장 JSON 메시지 전송
            Dictionary<string, string> json = new Dictionary<string, string>
                    {
                        {"type", "initGame" },
                        {"userID", userInfo.GetUserID()},
                        {"content", userInfo.GetRoom().getNo().ToString()},
                        {"content2", userInfo.GetRoom().getNowPeople().ToString()}
                    };
            //서버 데이터 전송
            string sendData = JsonConvert.SerializeObject(json, Formatting.Indented);
            client.OnSendData(sendData);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //데이터 받아오기
        //기존 데이터와 동일한 데이터일 경우 데이터 실행 X
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

    //받은 JSON 데이터 처리
    public void processJson(JObject receivedData)
    {
        //타입 저장
        string type = receivedData["type"].ToString();

        if (type.Equals("initGame")) // 게임 초기화
        {
            string userID = receivedData["userID"].ToString();
            int roomNo = int.Parse(receivedData["content"].ToString());
            int nowTurn = int.Parse((receivedData["content2"].ToString()));

            if (roomNo == userInfo.GetRoom().getNo()) // 방정보가 같을때 수행
            {

                for (i = 0; i < 4; i++) // 자기차례면 그 위치에 초록색 ( 차례가 아니면 빨간색)
                {
                    if (userInfo.GetRoom().getUsers()[i] == null)
                        continue;

                    user[i].sprite = charImage[i];
                    
                    if (userInfo.GetRoom().getUsers()[nowTurn].Equals(userInfo.GetRoom().getUsers()[i]))
                    {
                        userStatus[i].sprite = bar_Ready;
                    }
                    else
                    {
                        userStatus[i].sprite = bar_unReady;
                    }
                }
                // 자기차례면 버튼 텍스트 활성화
                if (userInfo.GetRoom().getUsers()[nowTurn].Equals(userInfo.GetUserID()))
                {
                    sendText.interactable = true;
                    sendBtn.interactable = true;
                }
                else
                {
                    sendText.interactable = false;
                    sendBtn.interactable = false;
                }
            }
        }else if (type.Equals("procGame")) // 게임 진행 상태
        {
            string userID = receivedData["userID"].ToString();
            int roomNo = int.Parse(receivedData["content"].ToString());
            
            if (roomNo == userInfo.GetRoom().getNo()) // 방정보가 같을때 수행
            {
                JObject gamedatas = JObject.Parse(receivedData["content2"].ToString());

                int nowTurn = int.Parse(gamedatas["nowTurn"].ToString());
                string result = gamedatas["result"].ToString();
                bool iscorrect = bool.Parse(gamedatas["iscorrect"].ToString());
                string answer_num = gamedatas["answerNum"].ToString();

                numView.text = answer_num;

                if (iscorrect) // 4strike (맞췄을경우)
                {
                    resultView.text = userID + "님의 정답입니다!!!!";

                    SceneManager.LoadScene("WaitRoom");
                }
                else
                {
                    resultView.text = result;

                    for(i=0; i<4; i++) // 자기차례면 그 위치에 초록색 ( 차례가 아니면 빨간색)
                    {
                        if (userInfo.GetRoom().getUsers()[i] == null)
                            continue;

                        if (userInfo.GetRoom().getUsers()[nowTurn].Equals(userInfo.GetRoom().getUsers()[i]))
                        {
                            userStatus[i].sprite = bar_Ready;
                        }
                        else
                        {
                            userStatus[i].sprite = bar_unReady;

                        }
                    }
                    // 자기차례면 버튼 텍스트 활성화
                    if (userInfo.GetRoom().getUsers()[nowTurn].Equals(userInfo.GetUserID()))
                    {
                        sendText.interactable = true;
                        sendBtn.interactable = true;
                    }
                    else
                    {
                        sendText.interactable = false;
                        sendBtn.interactable = false;
                    }
                }
            }

        }

        checkData = true;
    }
    
}

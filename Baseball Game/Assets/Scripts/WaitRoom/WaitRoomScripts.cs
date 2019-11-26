using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;

public class WaitRoomScripts : MonoBehaviour
{
    //유저 정보 불러오기
    UserInfo userInfo;
    public Image[] user = new Image[4];
    public Image[] userStatus = new Image[4];
    public Image[] userCheck = new Image[4];

    //통신용 클라이언트 불러오기
    Client client;

    //방 정보 설정
    Room room;
    //유저가 속한 방의 슬롯 번호
    int slotNum = 0;

    //시작 버튼 텍스트 변수
    public Text buttonText;

    //받은 데이터를 임시 저장할 변수
    public static string receivedData = "";
    public static string preReceivedData = "";
    JObject data;

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

    //방의 유저의 상태정보를 담을 배열 변수
    public string[] Ustatus = new string[4];

    // Start is called before the first frame update
    void Start()
    {
        client = FindObjectOfType<Client>();
        userInfo = FindObjectOfType<UserInfo>();
        room = new Room();
        initRoomData();
        refreshUser();
        sendEnterDate();

        //방의 유저의 상태정보 초기화
        for(int i = 0; i < 4; i++)
        {
            Ustatus[i] = "true";
        }
    }

    // Update is called once per frame
    void Update()
    {
        //데이터 수신
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

    //방의 정보 초기 설정
    public void initRoomData()
    {
        //유저가 속한 방의 번호, 이름, 유저 아이디 설정
        this.room.setNo(userInfo.userRoom.getNo());
        this.room.setBtnNo(userInfo.userRoom.getBtnNo());
        this.room.createRoom(userInfo.userRoom.getUsers()[0]);
        for (int k = 1; k < 4; k++)
        {
            if (userInfo.userRoom.getUsers()[k] != null)
            {
                this.room.updatePeople(userInfo.userRoom.getUsers()[k]);
                if (userInfo.userRoom.getUsers()[k].Equals(userInfo.GetUserID()))
                {
                    //유저가 속한 방 슬롯 저장 방장일경우 이쪽으로 분기 미실시(0번 그대로)
                    slotNum = k;
                }
            }
        }

        //방장일 경우 시작하기 버튼으로 변경
        if (room.getUsers()[0].Equals(userInfo.GetUserID())){
            buttonText.text = "시작하기";
        }
        
    }

    //방의 유저 정보 새로고침
    public void refreshUser()
    {
        for(int i = 0; i < 4; i++)
        {
            if (room.getUsers()[i] != null)
            {
                user[i].sprite = charImage[i];
                if (room.getUsers()[i].Equals(userInfo.GetUserID()))
                {
                    if (i == 0)
                    {
                        userCheck[i].sprite = userKing;
                    }
                    else
                    {
                        userCheck[i].sprite = userMe;
                    }
                    
                }
            }else if (room.getUsers()[i] == null)
            {
                user[i].sprite = noneImage;
            }
        }
    }

    //방 입장 데이터 전송
    public void sendEnterDate()
    {
        //방을 만든사람이 아닌경우에만 입장 데이터 전송
        if (slotNum != 0)
        {
            Dictionary<string, string> json = new Dictionary<string, string>
        {
            {"type","enterRoom" },
            {"userID",userInfo.GetUserID() },
            {"content", room.getNo().ToString() },
            {"content2", slotNum.ToString() }
        };
            string sendData = JsonConvert.SerializeObject(json, Formatting.Indented);
            client.OnSendData(sendData);
        }
    }

    //받은 JSON 데이터 처리
    public void processJson(JObject receivedData)
    {
        //타입 저장
        string type = receivedData["type"].ToString();
        //누군가가 방을 들어왔을 때 행동
        if (type.Equals("enterRoom"))
        {
            //실행 유저 아이디, 방번호, 슬롯번호 저장
            string ID = receivedData["userID"].ToString();
            int no = int.Parse(receivedData["content"].ToString());
            int slotNum = int.Parse(receivedData["content2"].ToString());

            //실행 유저가 본인이 아니고, 방 번호가 일치할 때 실행
            if (!ID.Equals(userInfo.GetUserID()) && room.getNo()==no)
            {
                //방 정보에 해당 유저 정보 추가
                room.userID[slotNum] = ID;
                room.nowPeople++;
                Ustatus[slotNum] = "false";
                userInfo.GetRoom().userID[slotNum] = ID;
                userInfo.GetRoom().nowPeople++;
                refreshUser();
            }
        }
        //누군가가 방을 나갔을 때 행동
        else if (type.Equals("exitRoom"))
        {
            //실행 유저 아이디, 방번호 저장
            string ID = receivedData["userID"].ToString();
            int no = int.Parse(receivedData["content"].ToString());
            //방번호가 같을 때 행동
            if(room.getNo() == no)
            {
                for(int i = 0; i < 4; i++)
                {
                    //유저 아이디 탐색 후 해당 유저 슬롯 제거
                    if (room.userID[i].Equals(ID))
                    {
                        room.userID[i] = null;
                        room.nowPeople--;
                        userInfo.GetRoom().userID[slotNum] = null;
                        userInfo.GetRoom().nowPeople--;
                        Ustatus[i] = "false";
                        userStatus[i].sprite = bar_unReady;
                        userCheck[i].sprite = userOther;
                        refreshUser();
                        break;
                    }
                }
            }
        }
        //누군가가 준비완료를 했을 때
        else if (type.Equals("readyPlay"))
        {
            string ID = receivedData["userID"].ToString();
            int no = int.Parse(receivedData["content"].ToString());
            string status = receivedData["content2"].ToString();
            //방번호가 같을 때 행동
            if (room.getNo() == no)
            {
                for (int i = 0; i < 4; i++)
                {
                    //유저 아이디 탐색 후 해당 유저 슬롯의 상태 변경
                    if (room.userID[i].Equals(ID))
                    {
                        //유저의 상태 변경
                        if(status.Equals("true"))
                        {
                            Ustatus[i] = "true";
                            userStatus[i].sprite = bar_Ready;
                        }else if (status.Equals("false"))
                        {
                            Ustatus[i] = "false";
                            userStatus[i].sprite = bar_unReady;
                        }
                        refreshUser();
                        break;
                    }
                }
            }
        }
        //방장이 게임시작을 눌렀을 때 행동
        else if (type.Equals("startPlay"))
        {
            string ID = receivedData["userID"].ToString();
            int no = int.Parse(receivedData["content"].ToString());
            //방 번호가 일치하면 게임시작
            if(room.getNo() == no)
            {
                
                SceneManager.LoadScene("GameScene");
            }
        }

        checkData = true;
    }


    //방 객체
    public class Room
    {
        //방 이름, 현재 인원, 최대 인원, 입장 유저 아이디 변수 선언

        private string roomName = "새로운 방";
        public int nowPeople = 1;
        [SerializeField]
        private int maxPeople = 4;
        public string[] userID = new string[4];

        //방 구분을 위한 방 번호, 현재 방 버튼의 위치 및 활성화 여브
        [SerializeField]
        private int no = 0;
        [SerializeField]
        private int btnNo = -1;

        //방 초기 생성
        public void createRoom(string userID)
        {
            this.userID[0] = userID;
        }

        //방 이름 설정
        public void setRoomName(string roomName)
        {
            this.roomName = roomName;
        }

        //방 인원수 업데이트
        public void updatePeople(string userID)
        {
            this.userID[nowPeople] = userID;
            nowPeople++;
        }

        //방 나가기
        public void exitRoom(string userID)
        {
            for (int i = 0; i < 4; i++)
            {
                if (this.userID[i].Equals(userID))
                {
                    this.userID[i] = null;
                    nowPeople--;
                }
            }
        }

        public int getNowPeople()
        {
            return nowPeople;
        }

        //방 이름 반환
        public string getRoomName()
        {
            return this.roomName;
        }

        //방 번호 설정
        public void setNo(int no)
        {
            this.no = no;
        }

        //방 번호 반환
        public int getNo()
        {
            return this.no;
        }

        //방 버튼 번호 설정
        public void setBtnNo(int btnNo)
        {
            this.btnNo = btnNo;
        }

        //방 버튼 번호 반환
        public int getBtnNo()
        {
            return this.btnNo;
        }

        //방 인원 배열 반환
        public string[] getUsers()
        {
            return this.userID;
        }

    }
}

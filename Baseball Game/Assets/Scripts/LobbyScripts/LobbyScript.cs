using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;

public class LobbyScript : MonoBehaviour
{
    //최대 페이지 번호, 최대 페이지당 방 개수, 현재 페이지 번호, 현재 방 번호 변수 선언
    int maxPageNum = 10;
    int maxRoomNum = 6;
    int pageNum = 0;
    int roomNum = 0;

    //화면에 보이는 페이지 번호 변수 선언
    int nowPageNum = 0;

    //방 객체 선언
    public Room[,] roomList;
    UserInfo user;
    Client client;

    //방 정보를 담을 버튼 및 정보 불러오기
    public Button[] buttonRoom = new Button[6];
    public Text[] buttonRoomName = new Text[6];

    //왼쪽 오른쪽 페이지 이동 버튼
    public Button LeftPageButton;
    public Button RightPageButton;

    //방 제목 입력 필드
    public InputField inputRoomName;

    //받은 데이터를 임시 저장할 변수
    public static string receivedData="";
    public static string preReceivedData="";
    JObject data;

    //데이터 관련 카운팅 임시 변수 
    private static int once = 0;
    bool checkData = true;
    ChatContentList chat;

    //점수 표시
    public Text pointText;
    void Start()
    {
        //서버 불러오기
        client = FindObjectOfType<Client>();
        
        //초기 버튼 설정
        if (nowPageNum == 0)
        {
            LeftPageButton.interactable = false;
        }

        //방 빈객체 생성
        roomList = new Room[maxPageNum,maxRoomNum];

        //유저 정보 불러오기
        user = FindObjectOfType<UserInfo>();

        
            //로비 입장 JSON 메시지 전송
            Dictionary<string, string> json = new Dictionary<string, string>
                    {
                        {"type", "loadRoom" },
                        {"userID", user.GetUserID()},
                        {"content", ""},
                        {"content2", ""}
                    };
            //서버 데이터 전송
            string sendData = JsonConvert.SerializeObject(json, Formatting.Indented);
            client.OnSendData(sendData);



        chat = FindObjectOfType<ChatContentList>();


        //현재 방 정보 불러와서 저장
        

    }

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

    //방 생성 버튼 클릭 이벤트
    public void onClickCreateRoom()
    {
        //방 번호 임시 변수 및 방 만들기 체크용 임시 변수

        string roomName="";
        roomName = inputRoomName.text;

        //방 번호 임시 변수 및 방 만들기 체크용 임시 변수
        int no = 0;
        bool checkNumMode = false;

        //비어있는 방 번호 확인 후 지정
        for (int i = 1; i < 61; i++)
        {
            checkNumMode = false;
            for (int j = 0; j < maxPageNum; j++)
            {
                for (int k = 0; k < 6; k++)
                {
                    if (roomList[j, k] != null)
                    {
                        if (i != roomList[j, k].getNo())
                        {
                            no = i;
                        }
                        else
                        {
                            checkNumMode = true;
                            break;
                        }
                    }
                }
                if (checkNumMode == true)
                {
                    break;
                }
            }
            if (checkNumMode == false)
            {

                break;

            }
        }

        //모두 비어있을 때 방번호 설정
        if (no == 0)
        {
            no = 1;
        }


        //방 자동 입장

        //Json 데이터 생성
        Dictionary<string, string> json = new Dictionary<string, string>
                    {
                        {"type", "createRoom" },
                        {"userID", user.GetUserID()},
                        {"content", no.ToString() },
                        {"content2", roomName}
                    };
        //서버 데이터 전송
        string sendData = JsonConvert.SerializeObject(json, Formatting.Indented);
        client.OnSendData(sendData);

        //유저가 속한 방의 번호, 이름, 유저 아이디 설정
        user.userRoom.setNo(no);
        user.userRoom.setRoomName(roomName);
        user.userRoom.createRoom(user.GetUserID());
        //화면 전환
        SceneManager.LoadScene("WaitRoom");
    }
    
    //왼쪽 페이지 이동 버튼 클릭 이벤트
    public void onClickMoveLeftPage()
    {
        if(!RightPageButton.IsInteractable())
        {
            RightPageButton.interactable = true;
        }
        nowPageNum--;
        refreshRoom();
        if (nowPageNum == 0)
        {
            LeftPageButton.interactable = false;
        }
    }

    //오른쪽 페이지 이동 버튼 클릭 이벤트
    public void onClickMoveRightPage()
    {
        if (!LeftPageButton.IsInteractable())
        {
            LeftPageButton.interactable = true;
        }
        nowPageNum++;
        refreshRoom();
        if (nowPageNum == 100)
        {
            RightPageButton.interactable = false;
        }
    }

    //방 현황 최신화 메소드
    public void refreshRoom()
    {
        //방 버튼 카운팅을 위한 임시 변수 및 방 활성화 여부 확인을 위한 변수
        int roomCount = 0;
        bool refreshMode = false;

        //현재 방 버튼 상태 초기화
        for (int i = 0; i < 6; i++)
        {
            buttonRoom[i].interactable = false;
            buttonRoomName[i].text = "Empty Room";
        }
               


        for(int i = 0 ; i < maxPageNum; i++)
        {
            for(int j = 0; j < 6; j++)
            {
                //방이 있을 경우 분기
                if(roomList[i,j] != null)
                {
                    //기존에 등록된 방의 버튼 번호 초기화
                    roomList[i,j].setBtnNo(-1);

                    //방의 개수를 센다.
                    roomCount++;
                    //방의 개수와 현재 페이지를 비교하여 현재페이지에 할당되는 방만 표시되게 분기
                    if (roomCount > nowPageNum*6 && roomCount <= (nowPageNum+1)*6)
                    {
                        for (int k = 0; k < 6; k++)
                        {
                            //방 버튼 중 빈공간을 찾아 방을 매칭한다.
                            if (!buttonRoom[k].IsInteractable())
                            {
                                buttonRoom[k].interactable = true;
                                buttonRoomName[k].text =roomList[i,j].getNo() + "\n" +  roomList[i, j].getRoomName() + "\n" + roomList[i, j].getNowPeople() + "/4";
                                roomList[i, j].setBtnNo(k);
                                refreshMode = true;
                                break;
                            }
                        }
                    }
                }
            }
        }

    }

    //JSON 처리 메소드
    private void processJson(JObject receivedData)
    {
        //타입 저장
        string type = receivedData["type"].ToString();
        string userID = receivedData["userID"].ToString();

        //타입에 따른 명령어 분기
        //방을 만들 때 행동
        if (type.Equals("createRoom"))
        {
            createRoom(receivedData["content2"].ToString(), receivedData["userID"].ToString(), int.Parse(receivedData["content"].ToString()));
            refreshRoom();
        }
        //로비 입장시 방 정보 현황 불러오기
        else if (type.Equals("loadRoom"))
        {
            string score = receivedData["content2"].ToString();
            if (userID.Equals(user.GetUserID())){
                if (receivedData["content"].ToString().Equals(""))
                {
                    pointText.text = "Point : " + score;
                }
                else if (JObject.Parse(receivedData["content"].ToString()) != null)
                {
                    JObject roomData = JObject.Parse(receivedData["content"].ToString());
                    createBeforeRoom(roomData);
                    refreshRoom();
                }
            }
        }
        //누군가가 방 입장시 방 현황 최신화
        else if (type.Equals("enterRoom"))
        {
            string ID = (receivedData["userID"].ToString());
            int no = int.Parse(receivedData["content"].ToString());
            int slotNum = int.Parse((receivedData["content2"].ToString()));
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    //입장한 방 번호 검색 후 행동 실행
                    if (roomList[i, j].getNo() == no)
                    {
                        roomList[i, j].userID[slotNum] = ID;
                        roomList[i, j].nowPeople++;
                        
                    }
                }
            }
        }
        //누군가가 방을 나올 때 방 현황 최신화
        else if (type.Equals("exitRoom"))
        {
            string ID = receivedData["userID"].ToString();
            int no = int.Parse(receivedData["content"].ToString());
            for(int i = 0; i < 10; i++)
            {
                for(int j = 0; j < 6; j++)
                {
                    if (roomList[i,j] != null)
                    {
                        for(int k = 0; k < 4; k++)
                        {
                            //해당 아이디의 방의 슬롯 정보 제거
                            if(roomList[i, j].userID[k].Equals(ID))
                            {
                                roomList[i, j].userID[k] = null;
                                roomList[i, j].nowPeople--;
                                //방의 인원이 0명일 경우 방 제거
                                if (roomList[i, j].nowPeople == 0)
                                {
                                    roomList[i, j] = null;
                                }
                                refreshRoom();
                                break;
                            }
                        }
                        
                    }
                }
            }
        }
        else if (type.Equals("inputChat"))
        {
            chat.processJson(receivedData);
        }
        //작업 종료후 데이터 처리 완료처리
        checkData = true;
    }

    //방을 만드는 메소드
    private void createRoom(string roomName, string userID, int no)
    {
        //방 번호 임시 변수 및 방 만들기 체크용 임시 변수
 
        bool createRoomMode = false;


        //비어있는 방 배열 확인
        for (int i = 0; i < maxPageNum; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                //방이 비어있으면 방 생성
                if (roomList[i, j] == null)
                {
                    roomList[i, j] = new Room();
                    roomList[i, j].createRoom(userID);
                    roomList[i, j].setRoomName(roomName);
                    roomList[i, j].setNo(no);
                    createRoomMode = true;
                    break;
                }
            }
            if (createRoomMode == true)
            {
                break;
            }
        }
    }

    //기존 방을 활성화 하는 메소드
    private void createBeforeRoom(JObject roomData)
    {
        //방 번호 임시 변수 및 방 만들기 체크용 임시 변수
        bool createRoomMode = false;

        //방 정보 변수
        int no = int.Parse(roomData["no"].ToString());
        string roomName = roomData["name"].ToString();
        string userID1 = roomData["user1"].ToString();
        string userID2 = roomData["user2"].ToString();
        string userID3 = roomData["user3"].ToString();
        string userID4 = roomData["user4"].ToString();

        //비어있는 방 배열 확인
        for (int i = 0; i < maxPageNum; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                //방이 비어있으면 방 생성
                if (roomList[i, j] == null)
                {
                    roomList[i, j] = new Room();
                    roomList[i, j].createRoom(userID1);
                    roomList[i, j].setRoomName(roomName);
                    roomList[i, j].setNo(no);

                    if (!userID2.Equals(""))
                    {
                        Debug.Log(userID2);
                        roomList[i, j].updatePeople(userID2);
                    }
                    if (!userID3.Equals(""))
                    {
                        roomList[i, j].updatePeople(userID3);
                    }
                    if (!userID4.Equals(""))
                    {
                        roomList[i, j].updatePeople(userID4);
                    }

                    createRoomMode = true;
                    break;
                }
            }
            if (createRoomMode == true)
            {
                break;
            }
        }
    }
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

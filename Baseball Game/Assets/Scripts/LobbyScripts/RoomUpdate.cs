using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public class RoomUpdate : MonoBehaviour
{
    //최대 페이지 번호, 최대 페이지당 방 개수, 현재 페이지 번호, 현재 방 번호 변수 선언
    int maxPageNum = 10;
    int maxRoomNum = 6;
    int pageNum = 0;
    int roomNum = 0;

    //화면에 보이는 페이지 번호 변수 선언
    int nowPageNum = 0;

    //방 객체 선언
    Room[,] roomList;
    UserInfo user;
    ClientTest client;

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

    void Start()
    {
        //서버 불러오기
        client = FindObjectOfType<ClientTest>();
        
        //초기 버튼 설정
        if (nowPageNum == 0)
        {
            LeftPageButton.interactable = false;
        }

        //방 빈객체 생성
        roomList = new Room[maxPageNum,maxRoomNum];

        //방 버튼 정보 속성
        

        //유저 정보 불러오기
        user = FindObjectOfType<UserInfo>();

        //현재 방 정보 불러와서 저장


    }

    void Update()
    {
        //데이터 받아오기
        //기존 데이터와 동일한 데이터일 경우 데이터 실행 X
        receivedData = client.getReceivedData();
        Debug.Log(receivedData);        
        if (receivedData != null && !preReceivedData.Equals(receivedData))
        {
            preReceivedData = receivedData;
            data = JObject.Parse(receivedData);
            processJson(data);
            receivedData = null;
        }
        

        //데이터 종류에 ㄸ

        //방 정보 업데이트
        //방 입장한 이용자나 나간 이용자를 받아서 방을 비활성화 또는 활성화
        //페이지를 넘기면 해당 페이지에 맞는 방 정보를 방 배열로 부터 받아와서 활성화 또는 비활성화

        //방 정보를 받아와서 현재 페이지의 방이 만들어져 있으면 버튼 활성화
        //if (roomList[nowPageNum, 0] != null) { buttonRoom[0].interactable = true; }
        
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
                        {"userID", "tt"/*user.GetUserID()*/ },
                        {"content", no.ToString() },
                        {"content2", roomName}
                    };
        //서버 데이터 전송
        string sendData = JsonConvert.SerializeObject(json, Formatting.Indented);
        client.DataInput(sendData);
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

        //타입에 따른 명령어 분기
        if (type.Equals("createRoom"))
        {
            createRoom(receivedData["content2"].ToString(), receivedData["userID"].ToString(), int.Parse(receivedData["content"].ToString()));
            refreshRoom();
        }
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
}

//방 객체
public class Room
{
    //방 이름, 현재 인원, 최대 인원, 입장 유저 아이디 변수 선언
    
    private string roomName = "새로운 방";
    [SerializeField]
    private int nowPeople = 1;
    [SerializeField]
    private int maxPeople = 4;
    [SerializeField]
    private string[] userID = new string[4];

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

}

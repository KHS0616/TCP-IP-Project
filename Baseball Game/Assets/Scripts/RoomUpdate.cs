using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomUpdate : MonoBehaviour
{
    //최대 페이지 번호, 최대 페이지당 방 개수, 현재 페이지 번호, 현재 방 번호 변수 선언
    int maxPageNum = 100;
    int maxRoomNum = 6;
    int pageNum = 0;
    int roomNum = 0;

    //화면에 보이는 페이지 번호 변수 선언
    int nowPageNum = 0;

    //방 객체 선언
    Room[,] roomList;
    UserInfo user;

    //방 정보를 담을 버튼 및 정보 불러오기
    public Button[] buttonRoom = new Button[6];
    public Text[] buttonRoomName = new Text[6];

    //왼쪽 오른쪽 페이지 이동 버튼
    public Button LeftPageButton;
    public Button RightPageButton;

    //방 제목 입력 필드
    public InputField inputRoomName;

    void Start()
    {
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
        //방 정보 데이터 수신

        //방 정보 업데이트
        //방 입장한 이용자나 나간 이용자를 받아서 방을 비활성화 또는 활성화
        //페이지를 넘기면 해당 페이지에 맞는 방 정보를 방 배열로 부터 받아와서 활성화 또는 비활성화

        //방 정보를 받아와서 현재 페이지의 방이 만들어져 있으면 버튼 활성화
        //if (roomList[nowPageNum, 0] != null) { buttonRoom[0].interactable = true; }
        
    }

    //방 생성 버튼 클릭 이벤트
    public void onClickCreateRoom()
    {
        //방 생성
        //화면에 보이는 방 개수에 따른 분기
        if (roomNum != 6)
        {
            roomList[pageNum,roomNum] = new Room();



            //방 생성 정보 불러오기
            //테스트 용으로 방을 생성만 하고 입장은 제한, 아래 코드는 페이지 전환시 발생되는 이벤트 함수로 이동예정
            //테스트용으로 바로 방 생성 임시로 유저아이디 임의부여
            //인풋 필드로 부터 방제목 불러오기
            roomList[pageNum, roomNum].createRoom("tt"/*user.GetUserID()*/);
            roomList[pageNum, roomNum].setRoomName(inputRoomName.text);
            if (nowPageNum == pageNum)
            {
                buttonRoom[roomNum].interactable = true;
                buttonRoomName[roomNum].text = roomList[nowPageNum, roomNum].getRoomName();
            }            
            roomNum++;
            //인풋 필드 초기화
            //inputRoomName.text = "";



        }
        else
        {
            pageNum++;
            roomNum = 0;
            roomList[pageNum, roomNum] = new Room();
            roomList[pageNum, roomNum].createRoom("tt"/*user.GetUserID()*/);
            roomList[pageNum, roomNum].setRoomName(inputRoomName.text);
            roomNum++;

        }

        //방 자동 입장

        //서버 데이터 전송
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
        for(int i = 0; i < 6; i++)
        {
            if (roomList[nowPageNum, i] != null)
            {
                buttonRoom[i].interactable = true;
                buttonRoomName[i].text = roomList[nowPageNum, i].getRoomName();
            }
            else
            {
                buttonRoom[i].interactable = false;
                buttonRoomName[i].text = "Empty Room";
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
    
    //방 이름 반환
    public string getRoomName()
    {
        return this.roomName;
    }
}

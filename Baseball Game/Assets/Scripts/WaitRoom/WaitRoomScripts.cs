using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaitRoomScripts : MonoBehaviour
{
    //유저 정보 불러오기
    UserInfo userInfo;
    public Image[] user = new Image[4];

    //방 정보 설정
    Room room;

    //시작 버튼 텍스트 변수
    public Text buttonText;



    // Start is called before the first frame update
    void Start()
    {
        userInfo = FindObjectOfType<UserInfo>();
        room = new Room();
        initRoomData();
        refreshUser();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //방의 정보 초기 설정
    public void initRoomData()
    {
        //유저가 속한 방의 번호, 이름, 유저 아이디 설정
        this.room.setNo(userInfo.userRoom.getNo());
        this.room.setBtnNo(userInfo.userRoom.getBtnNo());
        this.room.createRoom(userInfo.userRoom.getUsers()[0]);
        for (int k = 1; k < userInfo.userRoom.getNowPeople(); k++)
        {
            this.room.updatePeople(userInfo.userRoom.getUsers()[k]);
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
                user[i].color = Color.blue;
                if (room.getUsers()[i].Equals(userInfo.GetUserID()))
                {
                    user[i].color = Color.cyan;
                }
            }
        }
    }

    //방 입장 데이터 전송
    public void sendEnterDate()
    {
        Dictionary<string, string> json = new Dictionary<string, string>
        {
            {"type","enterRoom" },
            {"userID",userInfo.GetUserID() },
            {"content", room.getNo().ToString() }
        };
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AsyncServer
{
    //방 객체
    public class Room
    {
        //방 이름, 현재 인원, 최대 인원, 입장 유저 아이디 변수 선언

        private string roomName = "새로운 방";        
        private int nowPeople = 1;
        private int maxPeople = 4;
        private string[] userID = new string[4];

        //방 구분을 위한 방 번호, 현재 방 버튼의 위치 및 활성화 여브
        private int no = 0;
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

    //데이터 형식
    public struct Telegram
    {
        private int m_DataLength;
        private byte[] m_Data;

        public void SetLength(byte[] Data)
        {
            if (Data.Length < 4)
                return;
            m_DataLength = BitConverter.ToInt32(Data, 0);
        }
        public int DataLength
        {
            get { return m_DataLength; }
        }
        public void InitData()
        {
            m_Data = new byte[m_DataLength];
        }
        public byte[] Data
        {
            get { return m_Data; }
            set { m_Data = value; }
        }
        public String GetData()
        {
            return Encoding.Unicode.GetString(m_Data);
        }
        public byte[] GetBuffer()
        {
            return new byte[4];
        }
        public void SetData(String Data)
        {
            m_Data = Encoding.Unicode.GetBytes(Data);
            m_DataLength = m_Data.Length;
        }
    }
    enum ChatType
    {
        Send,
        Receive,
        System
    }
    class AsyncServer
    {
        //현재 방의 정보들을 담을 배열
        Room[,] roomList = new Room[10, 6];
        static int maxPageNum = 10;

        //명령어를 수신 할 JObject
        string receivedData = null;

        

        //클라이언트들을 담을 리스트
        private List<Socket> m_Client = null;

        //화면에 표시할 텍스트 리스트
        private List<StringBuilder> m_Display = null;

        //화면에 표시할 텍스트 최대 크기
        private int m_Line;

        static void Main(string[] args)
        {
            new AsyncServer();
        }
        public AsyncServer()
        {
            //초기값 부여
            m_Display = new List<StringBuilder>();
            m_Line = 0;

            //소켓 생성
            Socket _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint _ipep = new IPEndPoint(IPAddress.Any, 8000);
            _server.Bind(_ipep);
            _server.Listen(20);

            //소켓 비동기 연결 설정
            SocketAsyncEventArgs _args = new SocketAsyncEventArgs();
            _args.Completed += new EventHandler<SocketAsyncEventArgs>(Accept_Completed);

            //클라이언트 연결을 비동기로 받는다.
            _server.AcceptAsync(_args);


            //m_Client = new List<Socket>();
            //Console.ReadLine();
            //데이터 입력
            DataInput();
        }
        //서버->클라이언트 데이터 전송
        public void DataInput()
        {

            String sData;
            Telegram _telegram = new Telegram();
            m_Client = new List<Socket>();
            //서버 구동시 인사말 출력
            SendDisplay("ChattingProgram ServerStart", ChatType.System);

            while (true)
            {
                if (receivedData != null)
                {
                    
                    sData = receivedData;
                    receivedData = null;
                    if (sData.CompareTo("exit") == 0)
                    {
                        break;
                    }
                    else
                    {
                        foreach (Socket _client in m_Client)
                        {
                            //클라이언트 미 연결시 등록 제거
                            if (!_client.Connected)
                            {
                                m_Client.Remove(_client);
                            }
                            else
                            {
                                //데이터 전송
                                _telegram.SetData(sData);
                                SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
                                _sendArgs.SetBuffer(BitConverter.GetBytes(_telegram.DataLength), 0, 4);
                                _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Send_Completed);
                                _sendArgs.UserToken = _telegram;
                                _client.SendAsync(_sendArgs);
                            }
                        }
                    }
                    
                }
                //sData = Console.ReadLine();

                //exit 입력시 서버 종료

            }
        }


        //서버 화면에 내용 표시용 함수
        public void SendDisplay(String nMessage,ChatType nType)
        {
            StringBuilder buffer = new StringBuilder();
            //송신자에 따른 머릿말 구분
            switch(nType)
            {
                case ChatType.Send:
                    buffer.Append("SendMessage : ");
                    break;
                case ChatType.Receive:
                    buffer.Append("ReceiveMessage : ");
                    break;
                case ChatType.System:
                    buffer.Append("SystemMessage : ");
                    break;
            }
            buffer.Append(nMessage);

            //텍스트 줄에 따른 분기, 꽉 찼으면 맨 앞 텍스트 제거
            if (m_Line < 20)
            {
                m_Display.Add(buffer);
            }
            else
            {
                m_Display.RemoveAt(0);
                m_Display.Add(buffer);
            }
            m_Line++;

            //콘솔 초기화
            Console.Clear();

            //콘솔 표시 내용 재 출력
            for (int i = 0; i < 20; i++)
            {
                if (i < m_Display.Count)
                {
                    Console.WriteLine(m_Display[i].ToString());
                }
                else
                {
                    Console.WriteLine();
                }
            }
            //입력 대기 메시지
            Console.Write("Input Message: ");
        }


        //데이터 전송 세부과정
        private void Send_Completed(object sender, SocketAsyncEventArgs e)
        {
            Socket _client = (Socket)sender;
            Telegram _telegram = (Telegram)e.UserToken;
            _client.Send(_telegram.Data);
            SendDisplay(_telegram.GetData(), ChatType.Send);
        }

        //클라이언트 접속 연결 과정
        private void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {
            Socket _client = e.AcceptSocket;

            Telegram _telegram = new Telegram();
            SocketAsyncEventArgs _receiveArgs = new SocketAsyncEventArgs();
            _receiveArgs.UserToken = _telegram;
            _receiveArgs.SetBuffer(_telegram.GetBuffer(), 0, 4);
            _receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Recieve_Completed);
            _client.ReceiveAsync(_receiveArgs);
            m_Client.Add(_client);

            SendDisplay("Client Connection!",ChatType.System);

            Socket _server = (Socket)sender;
            e.AcceptSocket = null;
            _server.AcceptAsync(e);
        }

        private void Recieve_Completed(object sender, SocketAsyncEventArgs e)
        {
            Socket _client = (Socket)sender;
            Telegram _telegram = (Telegram)e.UserToken;
            _telegram.SetLength(e.Buffer);
            _telegram.InitData();
            //클라이언트 연결이 되 있으면 데이터 수신 및 저장
            if (_client.Connected)
            {
                _client.Receive(_telegram.Data, _telegram.DataLength, SocketFlags.None);

                //데이터 저장 및 처리 메소드 호출
                

                //데이터 클라이언트 회신

                SendDisplay(_telegram.GetData(), ChatType.Receive);

                receivedData = _telegram.GetData();
                

            }
            else
            {
                SendDisplay("Connection Failed.", ChatType.System);
            }
            if (_client.Connected)
            {
                _client.ReceiveAsync(e);
            }
            else
            {
                m_Client.Remove(_client);
            }
        }



        //방을 만드는 메소드
       

    }
}

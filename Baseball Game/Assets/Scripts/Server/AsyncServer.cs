using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace AsyncServer
{
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
                sData = Console.ReadLine();

                //exit 입력시 서버 종료
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
            Console.Write("Input Message (exit - 終了): ");
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
            if (_client.Connected)
            {
                _client.Receive(_telegram.Data, _telegram.DataLength, SocketFlags.None);
                SendDisplay(_telegram.GetData(), ChatType.Receive);
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
    }
}

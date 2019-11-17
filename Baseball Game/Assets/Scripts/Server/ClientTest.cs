using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ClientTest : MonoBehaviour
{
    //데이터 전송 형식
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

    private Socket m_Client = null;

    public InputField inputText;
    public Button sendToServerBtn;

    //받은 데이터 임시 저장 변수
    public static string receivedData;

    public static ClientTest instance = null;

    //서버 연결 카운팅 변수
    private static int once = 0;

    // Start is called before the first frame update
    void Start()
    {
        //다중 접속 방지용 코드
        if (once == 0)
        {
            once = 1;
            //소켓 설정 및 서버 연결
            Socket _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint _ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);

            SocketAsyncEventArgs _args = new SocketAsyncEventArgs();
            _args.RemoteEndPoint = _ipep;
            _args.Completed += new EventHandler<SocketAsyncEventArgs>(Connect_Completed);

            _client.ConnectAsync(_args);
        }
        

        // http://rapapa.net/?p=2936 onclick listener에 파라미터가 있는 메서드를 추가해주려면 아래와 같이 한다.
        // 기본형 : sendToServerBtn.onClick.AddListener( DataInput );
        // 파라미터가 있는 경우 : sendToServerBtn.onClick.AddListener(delegate { 함수명(파라미터); });

        //삭제 방지
        if (instance == null) // 중복방지
        {
            DontDestroyOnLoad(this);
            instance = this;
        }

        //sendToServerBtn.onClick.AddListener(delegate { DataInput(inputText.text); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //데이터 전송
    public void DataInput(string sendData)
    {
        String sData;
        Telegram _telegram = new Telegram();

        sData = sendData;
            if (sData.CompareTo("exit") == 0)   // exit란 텍스트가 넘어오면 무시가되는데 불필요할듯함.
            {
                
            }
            else
            {
                if (m_Client != null)
                {
                    if (!m_Client.Connected)
                    {
                        m_Client = null;

                    }
                    else
                    {
                        _telegram.SetData(sData);
                        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
                        _sendArgs.SetBuffer(BitConverter.GetBytes(_telegram.DataLength), 0, 4);
                        _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Send_Completed);
                        _sendArgs.UserToken = _telegram;
                        m_Client.SendAsync(_sendArgs);
                    }
                }
                else
                {
                    
                }
            }
    }

    //연결 성공
    private void Connect_Completed(object sender, SocketAsyncEventArgs e)
    {
        m_Client = (Socket)sender;

        if (m_Client.Connected)
        {
            Telegram _telegram = new Telegram();
            SocketAsyncEventArgs _receiveArgs = new SocketAsyncEventArgs();
            _receiveArgs.UserToken = _telegram;
            _receiveArgs.SetBuffer(_telegram.GetBuffer(), 0, 4);
            _receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Recieve_Completed);
            m_Client.ReceiveAsync(_receiveArgs);
        }
        else
        {
            m_Client = null;
        }
    }

    //데이터 수신
    private void Recieve_Completed(object sender, SocketAsyncEventArgs e)
    {
        Socket _client = (Socket)sender;
        Telegram _telegram = (Telegram)e.UserToken;
        _telegram.SetLength(e.Buffer);
        _telegram.InitData();
        if (_client.Connected)
        {
            //데이터 수신후 반환 메소드 호출
            _client.Receive(_telegram.Data, _telegram.DataLength, SocketFlags.None);
            receivedData = _telegram.GetData();
            _client.ReceiveAsync(e);
        }
        else
        {
            m_Client = null;
        }
    }

    //받은 데이터를 반환
    public string getReceivedData()
    {
        return receivedData;
    }

    //전송 성공
    private void Send_Completed(object sender, SocketAsyncEventArgs e)
    {
        Socket _client = (Socket)sender;
        Telegram _telegram = (Telegram)e.UserToken;
        _client.Send(_telegram.Data);
    }
}

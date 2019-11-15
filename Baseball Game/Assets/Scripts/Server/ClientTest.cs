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
    private List<StringBuilder> m_Display = null;
    private int m_Line;

    public InputField inputText;

    // Start is called before the first frame update
    void Start()
    {
        m_Display = new List<StringBuilder>();
        m_Line = 0;

        Socket _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint _ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);

        SocketAsyncEventArgs _args = new SocketAsyncEventArgs();
        _args.RemoteEndPoint = _ipep;
        _args.Completed += new EventHandler<SocketAsyncEventArgs>(Connect_Completed);

        _client.ConnectAsync(_args);

        //DataInput();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DataInput()
    {
        String sData;
        Telegram _telegram = new Telegram();
      
            sData = inputText.text;
            if (sData.CompareTo("exit") == 0)
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

    private void Recieve_Completed(object sender, SocketAsyncEventArgs e)
    {
        Socket _client = (Socket)sender;
        Telegram _telegram = (Telegram)e.UserToken;
        _telegram.SetLength(e.Buffer);
        _telegram.InitData();
        if (_client.Connected)
        {
            _client.Receive(_telegram.Data, _telegram.DataLength, SocketFlags.None);
            _client.ReceiveAsync(e);
        }
        else
        {
            m_Client = null;
        }
    }

    private void Send_Completed(object sender, SocketAsyncEventArgs e)
    {
        Socket _client = (Socket)sender;
        Telegram _telegram = (Telegram)e.UserToken;
        _client.Send(_telegram.Data);
    }
}

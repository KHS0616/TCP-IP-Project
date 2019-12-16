using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class Client : MonoBehaviour
{
    //변수 선언
    //소켓 선언
    Socket mainSock;

    //수신 데이터 저장용 리스트 변수
    public Queue<string> receivedDataList = new Queue<string>();

    //데이터 확인용 변수
    public static bool checkData = false;
    public string receivedData = "";

    //서버 연결 카운팅 변수
    private static int once = 0;
    public static Client instance = null;

    // Start is called before the first frame update
    void Start()
    {

        //삭제 방지
        if (instance == null) // 중복방지
        {
            DontDestroyOnLoad(this);
            instance = this;
        }

        if (once == 0)
        {
            once = 1;
            mainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            //IPHostEntry he = Dns.GetHostEntry(Dns.GetHostName());

            // ipv4 주소를 설정한다.
            IPAddress defaultHostAddress = IPAddress.Parse("10.70.41.194");
            //IPAddress defaultHostAddress = IPAddress.Parse("121.172.15.221");
            //IPAddress defaultHostAddress = IPAddress.Parse("192.168.219.115");
            //서버에 이미 연결되어 있는 경우
            if (mainSock.Connected)
            {
                Debug.Log("이미 연결되어 있습니다!");
                return;
            }

            //포트번호 설정
            int port = 15000;


            //연결에 따른 에러 제어
            try { mainSock.Connect(defaultHostAddress, port); }
            catch (Exception ex)
            {
                Debug.Log("연결에 실패했습니다!\n오류 내용: {0}" + ex.Message);
                return;
            }

            // 연결 완료되었다는 메세지를 띄워준다.
            Debug.Log("서버와 연결되었습니다.");

            // 연결 완료, 서버에서 데이터가 올 수 있으므로 수신 대기한다.
            AsyncObject obj = new AsyncObject(4096);
            obj.WorkingSocket = mainSock;
            mainSock.BeginReceive(obj.Buffer, 0, obj.BufferSize, 0, DataReceived, obj);
        }


    }

    // Update is called once per frame
    void Update()
    {
        if (checkData == true)
        {
            receivedData = null;
            checkData = false;
        }
    }

    //데이터 받는 콜백 메소드
    void DataReceived(IAsyncResult ar)
    {
        // BeginReceive에서 추가적으로 넘어온 데이터를 AsyncObject 형식으로 변환한다.
        AsyncObject obj = (AsyncObject)ar.AsyncState;

        // 데이터 수신을 끝낸다.
        int received = obj.WorkingSocket.EndReceive(ar);

        // 받은 데이터가 없으면(연결끊어짐) 끝낸다.
        if (received <= 0)
        {
            obj.WorkingSocket.Close();
            return;
        }

        // 텍스트로 변환한다.
        string text = Encoding.UTF8.GetString(obj.Buffer).Trim('\0');
        
        // 0x01 기준으로 짜른다.
        // tokens[0] - 보낸 사람 IP
        // tokens[1] - 보낸 메세지
        string[] tokens = text.Split('\x01');
        string ip = tokens[0];
        string msg = tokens[1];

        //복호화        
        msg = Decrypt(msg);
        
        Debug.Log("받은데이터 : " + msg);
        receivedDataList.Enqueue(msg);
        

        // 텍스트박스에 추가해준다.
        // 비동기식으로 작업하기 때문에 폼의 UI 스레드에서 작업을 해줘야 한다.
        // 따라서 대리자를 통해 처리한다.
        

        // 클라이언트에선 데이터를 전달해줄 필요가 없으므로 바로 수신 대기한다.
        // 데이터를 받은 후엔 다시 버퍼를 비워주고 같은 방법으로 수신을 대기한다.
        obj.ClearBuffer();

        // 수신 대기
        obj.WorkingSocket.BeginReceive(obj.Buffer, 0, 4096, 0, DataReceived, obj);
    }

    public void OnSendData(string sendData)
    {
        // 서버가 대기중인지 확인한다.
        if (!mainSock.IsBound)
        {
            Debug.Log("서버가 실행되고 있지 않습니다!");
            return;
        }
        //암호화
        sendData = Encrypt(sendData);
        Debug.Log(Decrypt(sendData));

        // 보낼 텍스트
        string tts = sendData;
        if (string.IsNullOrEmpty(tts))
        {
            Debug.Log("텍스트가 입력되지 않았습니다!");
            return;
        }

        // 서버 ip 주소와 메세지를 담도록 만든다.
        IPEndPoint ip = (IPEndPoint)mainSock.LocalEndPoint;
        string addr = ip.Address.ToString();

        // 문자열을 utf8 형식의 바이트로 변환한다.
        byte[] bDts = Encoding.UTF8.GetBytes(addr + '\x01' + tts);

        // 서버에 전송한다.
        mainSock.Send(bDts);

        // 전송 완료 후 텍스트박스에 추가하고, 원래의 내용은 지운다.
        Debug.Log(string.Format("[보냄]{0}: {1}", addr, tts));
    }

    //3DES 암호화 키
    private const string mysecurityKey = "MyTestSampleKey";

    //3DES 암호화
    public string Encrypt(string TextToEncrypt)
    {
        byte[] MyEncryptedArray = UTF8Encoding.UTF8
           .GetBytes(TextToEncrypt);

        MD5CryptoServiceProvider MyMD5CryptoService = new
           MD5CryptoServiceProvider();

        byte[] MysecurityKeyArray = MyMD5CryptoService.ComputeHash
           (UTF8Encoding.UTF8.GetBytes(mysecurityKey));

        MyMD5CryptoService.Clear();

        var MyTripleDESCryptoService = new TripleDESCryptoServiceProvider();

        MyTripleDESCryptoService.Key = MysecurityKeyArray;

        MyTripleDESCryptoService.Mode = CipherMode.ECB;

        MyTripleDESCryptoService.Padding = PaddingMode.PKCS7;

        var MyCrytpoTransform = MyTripleDESCryptoService
           .CreateEncryptor();

        byte[] MyresultArray = MyCrytpoTransform
           .TransformFinalBlock(MyEncryptedArray, 0,
           MyEncryptedArray.Length);

        MyTripleDESCryptoService.Clear();

        return Convert.ToBase64String(MyresultArray, 0,
           MyresultArray.Length);
    }

    //3DES 복호화
    public string Decrypt(string TextToDecrypt)
    {
        byte[] MyDecryptArray = Convert.FromBase64String
           (TextToDecrypt);

        MD5CryptoServiceProvider MyMD5CryptoService = new MD5CryptoServiceProvider();

        byte[] MysecurityKeyArray = MyMD5CryptoService.ComputeHash
           (UTF8Encoding.UTF8.GetBytes(mysecurityKey));

        MyMD5CryptoService.Clear();

        var MyTripleDESCryptoService = new
           TripleDESCryptoServiceProvider();

        MyTripleDESCryptoService.Key = MysecurityKeyArray;

        MyTripleDESCryptoService.Mode = CipherMode.ECB;

        MyTripleDESCryptoService.Padding = PaddingMode.PKCS7;

        var MyCrytpoTransform = MyTripleDESCryptoService
           .CreateDecryptor();

        byte[] MyresultArray = MyCrytpoTransform
           .TransformFinalBlock(MyDecryptArray, 0,
           MyDecryptArray.Length);

        MyTripleDESCryptoService.Clear();

        return UTF8Encoding.UTF8.GetString(MyresultArray);
    }
}

//소켓 통신용 객체
public class AsyncObject
{
    public byte[] Buffer;
    public Socket WorkingSocket;
    public readonly int BufferSize;
    public AsyncObject(int bufferSize)
    {
        BufferSize = bufferSize;
        Buffer = new byte[BufferSize];
    }

    public void ClearBuffer()
    {
        Array.Clear(Buffer, 0, BufferSize);
    }
}

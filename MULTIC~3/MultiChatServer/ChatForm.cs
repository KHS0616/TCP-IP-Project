using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace MultiChatServer {
    public partial class ChatForm : Form {
        

        delegate void AppendTextDelegate(Control ctrl, string s);
        AppendTextDelegate _textAppender;
        Socket mainSock;
        IPAddress thisAddress;


        //변수 선언

        // 게임별 리스트 생성
        GameData[] gameList = new GameData[60];


        //현재 방의 정보들을 담을 배열
        Room[,] roomList = new Room[10, 6];
        static int maxPageNum = 10;


        //오라클 데이터베이스 연동 관련 변수
        OracleConnection pgOraConn;
        OracleCommand pgOraCmd;

        public ChatForm() {
            InitializeComponent();
            mainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            _textAppender = new AppendTextDelegate(AppendText);
        }

        void AppendText(Control ctrl, string s) {
            if (ctrl.InvokeRequired) ctrl.Invoke(_textAppender, ctrl, s);
            else {
                string source = ctrl.Text;
                ctrl.Text = source + Environment.NewLine + s;
            }
        }
        //폼 로드 후 행동
        void OnFormLoaded(object sender, EventArgs e) {
            IPHostEntry he = Dns.GetHostEntry(Dns.GetHostName());

            // 처음으로 발견되는 ipv4 주소를 사용한다.
            foreach (IPAddress addr in he.AddressList) {
                if (addr.AddressFamily == AddressFamily.InterNetwork) {
                    thisAddress = addr;
                    break;
                }    
            }

            // 주소가 없다면..
            if (thisAddress == null)
                // 로컬호스트 주소를 사용한다.
                thisAddress = IPAddress.Loopback;

            txtAddress.Text = thisAddress.ToString();
        }

        //서버 시작 직전 행동
        void BeginStartServer(object sender, EventArgs e) {
            int port;
            if (!int.TryParse(txtPort.Text, out port)) {
                MsgBoxHelper.Error("포트 번호가 잘못 입력되었거나 입력되지 않았습니다.");
                txtPort.Focus();
                txtPort.SelectAll();
                return;
            }

            // 서버에서 클라이언트의 연결 요청을 대기하기 위해
            // 소켓을 열어둔다.
            IPEndPoint serverEP = new IPEndPoint(thisAddress, port);
            mainSock.Bind(serverEP);
            mainSock.Listen(10);

           
            // 비동기적으로 클라이언트의 연결 요청을 받는다.
            mainSock.BeginAccept(AcceptCallback, null);

            //DB연결
            ConnectionDB("192.168.219.115", "xe", "tcptest", "1234");
            //ConnectionDB("112.160.58.8", "orcl", "tcptest", "1234");
            //ConnectionDB("127.0.0.1", "xe", "tcptest", "1234");


        }

        //클라이언트 가변 리스트 선언
        List<Socket> connectedClients = new List<Socket>();
        //클라이언트 연결 콜백 메소드
        void AcceptCallback(IAsyncResult ar) {
            // 클라이언트의 연결 요청을 수락한다.
            Socket client = mainSock.EndAccept(ar);

            // 또 다른 클라이언트의 연결을 대기한다.
            mainSock.BeginAccept(AcceptCallback, null);

            AsyncObject obj = new AsyncObject(4096);
            obj.WorkingSocket = client;

            // 연결된 클라이언트 리스트에 추가해준다.
            connectedClients.Add(client);

            // 텍스트박스에 클라이언트가 연결되었다고 써준다.
            AppendText(txtHistory, string.Format("클라이언트 (@ {0})가 연결되었습니다.", client.RemoteEndPoint));

            // 클라이언트의 데이터를 받는다.
            client.BeginReceive(obj.Buffer, 0, 4096, 0, DataReceived, obj);
        }

        //데이터 수신
        void DataReceived(IAsyncResult ar) {
            // BeginReceive에서 추가적으로 넘어온 데이터를 AsyncObject 형식으로 변환한다.
            AsyncObject obj = (AsyncObject)ar.AsyncState;

            // 데이터 수신을 끝낸다.
            int received = obj.WorkingSocket.EndReceive(ar);

            // 받은 데이터가 없으면(연결끊어짐) 끝낸다.
            if (received <= 0) {
                obj.WorkingSocket.Close();
                return;
            }

            // 텍스트로 변환한다.
            var text = Encoding.UTF8.GetString(obj.Buffer).Trim('\0');
            
            // 0x01 기준으로 짜른다.
            // tokens[0] - 보낸 사람 IP
            // tokens[1] - 보낸 메세지
            string[] tokens = text.Split('\x01');
            string ip = tokens[0];
            string msg = tokens[1];
            
            // 텍스트박스에 추가해준다.
            // 비동기식으로 작업하기 때문에 폼의 UI 스레드에서 작업을 해줘야 한다.
            // 따라서 대리자를 통해 처리한다.
            AppendText(txtHistory, string.Format("[받음]{0}: {1}", ip, msg));
            
            //데이터 복호화
            msg = Decrypt(msg);

            
            //데이터 JSON화
            msg = msg.Trim('\0');
            JObject receivedData = JObject.Parse(msg);

            processJson(receivedData);

            // for을 통해 "역순"으로 클라이언트에게 데이터를 보낸다.(보낸 클라이언트 제외)
            /*for (int i = connectedClients.Count - 1; i >= 0; i--)
            {
                Socket socket = connectedClients[i];
                if (socket != obj.WorkingSocket)
                {
                    try { socket.Send(obj.Buffer); }
                    catch
                    {
                        // 오류 발생하면 전송 취소하고 리스트에서 삭제한다.
                        try { socket.Dispose(); } catch { }
                        connectedClients.RemoveAt(i);
                    }
                }
            }*/

            // 데이터를 받은 후엔 다시 버퍼를 비워주고 같은 방법으로 수신을 대기한다.
            obj.ClearBuffer();

            // 수신 대기
            obj.WorkingSocket.BeginReceive(obj.Buffer, 0, 4096, 0, DataReceived, obj);
        }

        //자동 데이터 전송
        void SendData(string sendData)
        {
            // 서버가 대기중인지 확인한다.
            if (!mainSock.IsBound)
            {
                MsgBoxHelper.Warn("서버가 실행되고 있지 않습니다!");
                return;
            }

            //암호화
            sendData = Encrypt(sendData);

            // 문자열을 utf8 형식의 바이트로 변환한다.
            byte[] bDts = Encoding.UTF8.GetBytes(thisAddress.ToString() + '\x01' + sendData);

            // 연결된 모든 클라이언트에게 전송한다.
            for (int i = connectedClients.Count - 1; i >= 0; i--)
            {
                Socket socket = connectedClients[i];
                try { socket.Send(bDts); }
                catch
                {
                    // 오류 발생하면 전송 취소하고 리스트에서 삭제한다.
                    try { socket.Dispose(); } catch { }
                    connectedClients.RemoveAt(i);
                }
            }

            // 전송 완료 후 텍스트박스에 추가하고, 원래의 내용은 지운다.
            AppendText(txtHistory, string.Format("[보냄]{0}: {1}", thisAddress.ToString(), sendData));
            //txtTTS.Clear();
        }

        //버튼 클릭 데이터 전송
        void OnSendData(object sender, EventArgs e) {
            // 서버가 대기중인지 확인한다.
            if (!mainSock.IsBound) {
                MsgBoxHelper.Warn("서버가 실행되고 있지 않습니다!");
                return;
            }
            
            // 보낼 텍스트
            /*string tts = txtTTS.Text.Trim();
            if (string.IsNullOrEmpty(tts)) {
                MsgBoxHelper.Warn("텍스트가 입력되지 않았습니다!");
                txtTTS.Focus();
                return;
            }
            
            // 문자열을 utf8 형식의 바이트로 변환한다.
            byte[] bDts = Encoding.UTF8.GetBytes(thisAddress.ToString() + '\x01' + tts);

            // 연결된 모든 클라이언트에게 전송한다.
            for (int i = connectedClients.Count - 1; i >= 0; i--) {
                Socket socket = connectedClients[i];
                try { socket.Send(bDts); } catch {
                    // 오류 발생하면 전송 취소하고 리스트에서 삭제한다.
                    try { socket.Dispose(); } catch { }
                    connectedClients.RemoveAt(i);
                }
            }

            // 전송 완료 후 텍스트박스에 추가하고, 원래의 내용은 지운다.
            AppendText(txtHistory, string.Format("[보냄]{0}: {1}", thisAddress.ToString(), tts));
            txtTTS.Clear();*/
        }

        //받은 데이터에 따른 행동 분기
        private void processJson(JObject receivedData)
        {
            //누군가가 방을 만들 때 행동
            if ((receivedData["type"].ToString()).Equals("createRoom"))
            {
                createRoom(receivedData["content2"].ToString(), receivedData["userID"].ToString(), int.Parse(receivedData["content"].ToString()));
                SendData(receivedData.ToString());
            }
            //누군가가 회원가입을 시도했을 때 행동
            else if ((receivedData["type"].ToString()).Equals("signUp"))
            {
                string userID = receivedData["userID"].ToString();
                string userPW = receivedData["content"].ToString();
                string temp = ExecuteDataReader("SELECT * FROM userInfo WHERE id='" + userID + "'", "id");

                //salt값 설정
                int length = userID.Length;
                char a = userID[0];
                char b = userID[length-1];
                userPW = a + userPW + b;

                //암호화
                userPW = SHA256Hash(userPW);

                if (temp == null)
                {
                    //일치 아이디가 없으면 회원 데이터 추가
                    ExecuteDataQuery("INSERT INTO userInfo VALUES('" + userID + "', '" + userPW + "', 0)");

                    //회원가입 성공 JSON 메시지 작성 및 전송
                    Dictionary<string, string> json = new Dictionary<string, string>
                    {
                        {"type", "signUp" },
                        {"userID", userID },
                        {"content", "success" },
                        {"content2", ""}
                    };
                    //클라이언트 데이터 전송
                    SendData(JsonConvert.SerializeObject(json, Formatting.Indented));

                }
                else
                {
                    //회원가입 실패 JSON 메시지 작성 및 전송
                    Dictionary<string, string> json = new Dictionary<string, string>
                    {
                        {"type", "signUp" },
                        {"userID", userID },
                        {"content", "fail" },
                        {"content2", ""}
                    };
                    //클라이언트 데이터 전송
                    SendData(JsonConvert.SerializeObject(json, Formatting.Indented));
                }

            }
            //누군가가 로그인을 시도했을 때 행동
            else if ((receivedData["type"].ToString()).Equals("login"))
            {

                string userID = receivedData["userID"].ToString();
                string userPW = receivedData["content"].ToString();
                string temp = ExecuteDataReader("SELECT * FROM userInfo WHERE id='" + userID + "'", "id");

                //salt값 설정
                int length = userID.Length;
                char a = userID[0];
                char b = userID[length - 1];
                userPW = a + userPW + b;

                //암호화
                userPW = SHA256Hash(userPW);

                if (temp == null)
                {
                    //일치 아이디가 없으면 아이디 미 존재 JSON 메시지 작성 및 전송
                    Dictionary<string, string> json = new Dictionary<string, string>
                    {
                        {"type", "login" },
                        {"userID", userID },
                        {"content", "fail" },
                        {"content2", "id"}
                    };
                    SendData(JsonConvert.SerializeObject(json, Formatting.Indented));
                }
                else
                {
                    //비밀번호 불러오기
                    string savedPW = ExecuteDataReader("SELECT * FROM userInfo WHERE id='" + userID + "'", "pw");

                    if (userPW.Equals(savedPW))
                    {
                        //로그인 성공 JSON 메시지 작성 및 전송
                        Dictionary<string, string> json = new Dictionary<string, string>
                    {
                        {"type", "login" },
                        {"userID", userID },
                        {"content", "success" },
                        {"content2", ""}
                    };
                        //클라이언트 데이터 전송
                        SendData(JsonConvert.SerializeObject(json, Formatting.Indented));
                    }
                    else
                    {
                        //회원가입 실패 JSON 메시지 작성 및 전송
                        Dictionary<string, string> json = new Dictionary<string, string>
                    {
                        {"type", "login" },
                        {"userID", userID },
                        {"content", "fail" },
                        {"content2", "pw"}
                    };
                        //클라이언트 데이터 전송
                        SendData(JsonConvert.SerializeObject(json, Formatting.Indented));
                    }


                }

            }
            //누군가가 메인 로비 입장시 행동
            else if ((receivedData["type"].ToString()).Equals("loadRoom"))
            {
                string userID = receivedData["userID"].ToString();

                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        if (roomList[i, j] != null)
                        {
                            string score = ExecuteDataReader("SELECT * FROM userInfo WHERE id LIKE '" + userID + "'", "POINT");

                            var roomData = new JObject();
                            roomData.Add("no", roomList[i, j].getNo());
                            roomData.Add("name", roomList[i, j].getRoomName());
                            roomData.Add("user1", roomList[i, j].getUsers()[0]);
                            roomData.Add("user2", roomList[i, j].getUsers()[1]);
                            roomData.Add("user3", roomList[i, j].getUsers()[2]);
                            roomData.Add("user4", roomList[i, j].getUsers()[3]);

                            var json = new JObject();
                            json.Add("type", "loadRoom");
                            json.Add("userID", userID);
                            json.Add("content", roomData);
                            json.Add("content2", score);
                            SendData(JsonConvert.SerializeObject(json, Formatting.Indented));
                        }else if(i==9 && j == 5)
                        {
                            string score = ExecuteDataReader("SELECT * FROM userInfo WHERE id LIKE '" + userID + "'", "POINT");
                            var json = new JObject();
                            json.Add("type", "loadRoom");
                            json.Add("userID", userID);
                            json.Add("content", "");
                            json.Add("content2", score);
                            SendData(JsonConvert.SerializeObject(json, Formatting.Indented));
                        }
                    }
                }



            }
            //누군가가 방 입장시 행동
            else if ((receivedData["type"].ToString()).Equals("enterRoom"))
            {
                string userID = receivedData["userID"].ToString();
                int no = int.Parse(receivedData["content"].ToString());
                int slotNum = int.Parse((receivedData["content2"].ToString()));
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        if (roomList[i, j] != null)
                        {
                            //입장한 방 번호 검색 후 행동 실행
                            if (roomList[i, j].getNo() == no)
                            {
                                roomList[i, j].userID[slotNum] = userID;
                                roomList[i, j].nowPeople++;

                                Dictionary<string, string> json = new Dictionary<string, string>
                            {
                                {"type", "enterRoom" },
                                {"userID", userID },
                                {"content", no.ToString() },
                                {"content2", slotNum.ToString() }
                            };
                                //json 데이터 전송
                                string sendData = JsonConvert.SerializeObject(json, Formatting.Indented);
                                SendData(sendData);
                            }
                        }

                    }
                }

            }
            //누군가가 방을 나갈시 행동
            else if ((receivedData["type"].ToString()).Equals("exitRoom"))
            {
                string ID = receivedData["userID"].ToString();
                int no = int.Parse(receivedData["content"].ToString());
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        if (roomList[i, j] != null && roomList[i, j].getNo() == no)
                        {
                            for (int k = 0; k < 4; k++)
                            {
                                
                                //해당 아이디의 방의 슬롯 정보 제거
                                if (roomList[i, j].userID[k].Equals(ID))
                                {
                                    if (k == 0)
                                    {
                                        roomList[i, j] = null;
                                        //Json 데이터 생성 및 전송
                                        Dictionary<string, string> json = new Dictionary<string, string>
                                    {
                                        {"type", "exitRoom" },
                                        {"userID", ID },
                                        {"content", no.ToString() },
                                        {"content2", "" }
                                    };
                                        string sendData = JsonConvert.SerializeObject(json, Formatting.Indented);
                                        SendData(sendData);
                                        break;
                                    }
                                    else
                                    {
                                        roomList[i, j].userID[k] = null;
                                        roomList[i, j].nowPeople--;
                                        //방의 인원이 0명일 경우 방 제거
                                        if (roomList[i, j].nowPeople == 0)
                                        {
                                            roomList[i, j] = null;
                                        }


                                        //Json 데이터 생성 및 전송
                                        Dictionary<string, string> json = new Dictionary<string, string>
                                    {
                                        {"type", "exitRoom" },
                                        {"userID", ID },
                                        {"content", no.ToString() },
                                        {"content2", "" }
                                    };
                                        string sendData = JsonConvert.SerializeObject(json, Formatting.Indented);
                                        SendData(sendData);
                                        break;
                                    }
                                    
                                }
                            }

                        }
                    }
                }
            }
            //누군가가 준비완료시 행동
            else if ((receivedData["type"].ToString()).Equals("readyPlay"))
            {
                string sendData = JsonConvert.SerializeObject(receivedData);
                SendData(sendData);
            }
            //방장이 게임 시작시 행동
            else if ((receivedData["type"].ToString()).Equals("startPlay"))
            {
                string sendData = JsonConvert.SerializeObject(receivedData);
                SendData(sendData);
            }
            //게임 알고리즘 행동
            else if ((receivedData["type"].ToString()).Equals("initGame"))
            {
                string userID = receivedData["userID"].ToString();
                int roomNo = int.Parse(receivedData["content"].ToString());
                int peoplecnt = int.Parse(receivedData["content2"].ToString());

                // TODO : 여기에 게임 알고리즘을 넣어야하는데 방별로 해야됨..
                GameData gameData = new GameData(peoplecnt);
                gameList[roomNo] = gameData;  // 방번호를 index 첨자로 넣어 구분

                // 이후 접근방법
                //gameData = gameList[roomNo];

                Dictionary<string, string> json = new Dictionary<string, string>
                                    {
                                        {"type", "initGame" },
                                        {"userID", userID },
                                        {"content", roomNo.ToString() },
                                        {"content2", gameData.nowTurn.ToString() }
                                    };
                string sendData = JsonConvert.SerializeObject(json, Formatting.Indented);
                SendData(sendData);
            }
            //게임내 누군가가 숫자 입력시 행동
            else if ((receivedData["type"].ToString()).Equals("procGame"))
            {
                string userID = receivedData["userID"].ToString();
                int roomNo = int.Parse(receivedData["content"].ToString());
                int answer_nums = int.Parse(receivedData["content2"].ToString());
                string answer_nums_str = receivedData["content2"].ToString();

                GameData gameData = gameList[roomNo];
                string calcStr = gameData.calcData(answer_nums);

                var game = new JObject();
                game.Add("nowTurn", gameData.nowTurn.ToString());
                game.Add("result", calcStr);
                game.Add("iscorrect", gameData.correct);
                game.Add("answerNum", answer_nums_str);

                var json = new JObject();
                json.Add("type", "procGame");
                json.Add("userID", userID);
                json.Add("content", roomNo.ToString());
                json.Add("content2", game);
                SendData(JsonConvert.SerializeObject(json, Formatting.Indented));

                //결과가 성공일 경우 성공인 사람은 점수추가
                if (calcStr.Equals("정답!"))
                {
                    int temp;
                    string score = ExecuteDataReader("SELECT * FROM userInfo WHERE id LIKE '" + userID + "'", "POINT");
                    if (score == null)
                    {
                        temp = 0;
                    }
                    else
                    {
                        temp = int.Parse(score);
                    }
                    score = (temp + 50).ToString();
                    ExecuteDataQuery("UPDATE userinfo SET point = " + score + " WHERE id='" + userID + "'");
                }
            }
            //채팅 입력시 행동
            else if ((receivedData["type"].ToString()).Equals("inputChat"))
            {
                string sendData = JsonConvert.SerializeObject(receivedData);
                SendData(sendData);
            }
            //점수 집계화면 이동시 행동
        }

        //SHA-256 해시함수
        public string SHA256Hash(string data)
        {

            SHA256 sha = new SHA256Managed();
            byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(data));
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in hash)
            {
                stringBuilder.AppendFormat("{0:x2}", b);
            }
            return stringBuilder.ToString();
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

        // DB 연결
        private bool ConnectionDB(string dbIp, string dbName, string dbId, string dbPw)
        {
            bool retValue = false;
            try
            {
               
                pgOraConn = new OracleConnection($"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={dbIp})(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={dbName})));User ID={dbId};Password={dbPw};Connection Timeout=30;");
                pgOraConn.Open();
                pgOraCmd = pgOraConn.CreateCommand();
                retValue = true;
            }
            catch (Exception e)
            {
                txtHistory.Text += e.ToString();
                retValue = false;

            }

            return retValue;
        }

        //sql select 구문
        private string ExecuteDataReader(string query, string row)
        {
            string colA;
            pgOraCmd.CommandText = query;

            using (OracleDataReader dr = pgOraCmd.ExecuteReader())
            {
                if (dr != null)
                {
                    while (dr.Read())
                    {
                        colA = dr[row] as string;
                        return colA;
                    }
                }
            }
            return null;
        }

        //sql insert, create, update 구문
        private void ExecuteDataQuery(string query)
        {
            pgOraCmd.CommandText = query;
            pgOraCmd.ExecuteNonQuery();

        }

        //3DES 암호화 키
        private const string mysecurityKey = "MyTestSampleKey";

        //3DES 암호화
        public static string Encrypt(string TextToEncrypt)
        {
            byte[] MyEncryptedArray = UTF8Encoding.UTF8
               .GetBytes(TextToEncrypt);

            MD5CryptoServiceProvider MyMD5CryptoService = new
               MD5CryptoServiceProvider();

            byte[] MysecurityKeyArray = MyMD5CryptoService.ComputeHash
               (UTF8Encoding.UTF8.GetBytes(mysecurityKey));

            MyMD5CryptoService.Clear();

            var MyTripleDESCryptoService = new
               TripleDESCryptoServiceProvider();

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
        public static string Decrypt(string TextToDecrypt)
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
}

    public class GameData
    {
        public int peoplecnt;
        public int nowTurn;

        public int[] question_num { get;  } = new int[4];    //랜덤 숫자가 들어갈 배열
        public int[] answer_num { set; get; } = new int[4];  //도전해볼 숫자가 들어갈 배열

        public int ball = 0;            //볼의 개수
        public int strike = 0;              //스트라이크 개수
        private string text;                    // text를 담는 변수

        public bool correct { get; set; } = false;           // 정답 여부를 판별하는 변수

        private int i, j;                 // for 첨자
        private Random rand = new Random();

        public GameData(int peoplecnt)
        {
            this.peoplecnt = peoplecnt;
            nowTurn = 0;
            // 클래스 생성시 게임 질문 데이터가 생성됨
            for (i = 0; i < 4; i++)
            {
                question_num[i] = rand.Next(0, 9);
                for (j = 0; j < i; j++)
                {       //중복숫자가 나오지 않게 하기위함
                    if (question_num[i] == question_num[j])
                    {
                        i--;
                        break;
                    }
                }
            }
        }

        public string calcData(int answer_nums) // 수행 시 마다 순번이 바뀜. nowTurn++ ;
        {
            answer_num[0] = answer_nums / 1000;
            answer_num[1] = (answer_nums % 1000) / 100;
            answer_num[2] = ((answer_nums % 1000) % 100 )/ 10;
            answer_num[3] = ((answer_nums % 1000) % 100 )% 10;

            //숫자와 자리가 맞다면 스트라이크+1, 숫자는 맞고 자리가 틀리면 볼+1
            for (i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (question_num[i] == answer_num[j])
                    {
                        if (i == j)
                        {
                            strike++;
                        }
                        else
                        {
                            ball++;
                        }
                    }
                }
            }

            if (ball == 0)
            {
                if (strike == 0)
                {
                    text = "OUT!";
                }
                else if (strike == 4)
                {
                    text = "정답!";
                    correct = true;
                }
                else
                {
                    text = strike + "Strike";
                }
            }
            else
            {
                if (strike == 0)
                {
                    text = ball + "Ball";
                }
                else
                {
                    text = strike + "Strike " + ball + "Ball";
                }
            }
            nowTurn++;
            if(peoplecnt == nowTurn)
            {
                nowTurn = 0;
            }
            strike = 0;
            ball = 0;
            return text;
        }
    }




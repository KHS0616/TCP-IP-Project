using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SendScript : MonoBehaviour
{
    UserInfo userInfo;
    Client client;

    public Button sendBtn;
    public InputField sendNumber;

    private Text numText;
    // Start is called before the first frame update
    void Start()
    {
        client = FindObjectOfType<Client>();
        userInfo = FindObjectOfType<UserInfo>();

        numText = GameObject.Find("Num").GetComponent<Text>();

        sendBtn.onClick.AddListener(SendNumber);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SendNumber()
    {
        if (sendNumber.text.Length != 4)
        {
            numText.text = "숫자 4개를 입력해주세요!";
        }
        else
        {
            //Json 데이터 생성
            Dictionary<string, string> json = new Dictionary<string, string>
                    {
                        {"type", "procGame" },
                        {"userID", userInfo.GetUserID()},
                        {"content",userInfo.GetRoom().getNo().ToString() },
                        {"content2", sendNumber.text}
                    };
            //서버 데이터 전송
            string sendData = JsonConvert.SerializeObject(json, Formatting.Indented);
            client.OnSendData(sendData);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnterRoom : MonoBehaviour
{
    //정보를 불러올 버튼과 로비변수 선언
    public Button button;
    public Text buttonText;
    LobbyScript lobbyScript;
    UserInfo userInfo;
    // Start is called before the first frame update
    void Start()
    {
        lobbyScript = FindObjectOfType<LobbyScript>();
        userInfo = FindObjectOfType<UserInfo>();
        button.onClick.AddListener(enterRoom);
    }

    public void enterRoom()
    {
        
        int btnNum = -1;
        for(int i = 0; i < 6; i++)
        {
            if (buttonText.text.Equals(lobbyScript.buttonRoomName[i].text))
            {
                btnNum = i;
                break;
            }
            
        }

        for (int i = 0; i < 10; i++)
        {
            for(int j = 0; j < 6; j++)
            {
                if (lobbyScript.roomList[i, j] != null)
                {
                    if (btnNum == lobbyScript.roomList[i, j].getBtnNo() && lobbyScript.roomList[i, j].getNowPeople() != 4)
                    {
                        //유저가 속한 방의 번호, 이름, 유저 아이디 설정
                        userInfo.userRoom.setNo(lobbyScript.roomList[i, j].getNo());
                        userInfo.userRoom.setRoomName(lobbyScript.roomList[i, j].getRoomName());
                        userInfo.userRoom.createRoom(lobbyScript.roomList[i, j].getUsers()[0]);
                        for (int k = 1; k < 4; k++)
                        {
                            //방에 속한 유저 정보 대입 -> 중간에 빈공간 대비하여 null값 비교후 동일한 위치에 대입
                            if(lobbyScript.roomList[i, j].getUsers()[k]!=null)
                            {
                                userInfo.userRoom.userID[k] = lobbyScript.roomList[i, j].getUsers()[k];
                                userInfo.userRoom.nowPeople++;
                            }                            
                        }
                        //방에 속한 유저 정보중 빈 공간에 자신의 아이디 추가
                        for(int k = 1; k < 4; k++)
                        {
                            if (userInfo.userRoom.getUsers()[k]==null)
                            {
                                userInfo.userRoom.userID[k] = userInfo.GetUserID();
                                userInfo.userRoom.nowPeople++;
                                break; 
                            }
                        }
                        //화면 전환
                        SceneManager.LoadScene("WaitRoom");

                    }
                }

            }
        }
        
    }

}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiChatServer
{
    public class Room
    {
        //방 이름, 현재 인원, 최대 인원, 입장 유저 아이디 변수 선언

        private string roomName = "새로운 방";
        public int nowPeople = 1;
        private int maxPeople = 4;
        public string[] userID = new string[4];

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

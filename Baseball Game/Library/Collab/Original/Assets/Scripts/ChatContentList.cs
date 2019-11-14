﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatContentList : MonoBehaviour
{
    //복제될 TEXT UI Object
    public GameObject preText;

    //스크롤 바
    private ScrollRect scroll_rect = null;

    //Text UI의 부모 컴포넌트
    public Transform textParent;


    private GameObject lastText;
    ArrayList textList = new ArrayList();
    private int i = 0;

    // Start is called before the first frame update
    void Start()
    {
        //Text UI의 Text 컴포넌트 지정
        preText = GameObject.Find("ChatContentList");

        //스크롤 바 지정
        scroll_rect = GameObject.Find("ChatContentScroll").GetComponent<ScrollRect>();

        //Text의 부모 컴포넌트 지정
        textParent = GameObject.Find("Content").GetComponent<Transform>();

        //연결 성공시 인사말 출력
        if(preText != null)
        {
            preText.GetComponentInChildren<Text>().text = "서버 접속에 성공하였습니다.";
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            //preText.text += "Mouse down Position(" + "X: " + Input.mousePosition.x + " Y: " + Input.mousePosition.y + ")\n";
            /*if (lastText == null) {
                float x = preText.transform.position.x;
                float y = preText.transform.position.y;
                float z = preText.transform.position.z;

                lastText = Instantiate(preText.gameObject, new Vector3(x, y - 20f, z), Quaternion.identity) as GameObject;
                lastText.transform.SetParent(textParent);
                lastText.transform.localScale = new Vector3(1,1,1);
                lastText.name = "ChatContentList" + (++i);
                lastText.GetComponentInChildren<Text>().text = "Mouse down Position(" + "X: " + Input.mousePosition.x + " Y: " + Input.mousePosition.y + ")\n";
                textList.Add(lastText);
            }
            else
            {
                float x = lastText.transform.position.x;
                float y = lastText.transform.position.y;
                float z = lastText.transform.position.z;

                lastText = Instantiate(lastText, new Vector3(x, y - 15f, z), Quaternion.identity) as GameObject;
                lastText.transform.SetParent(textParent);
                lastText.transform.localScale = new Vector3(1, 1, 1);
                lastText.name = "ChatContentList" + (++i);
                lastText.GetComponentInChildren<Text>().text = "Mouse down Position(" + "X: " + Input.mousePosition.x + " Y: " + Input.mousePosition.y + ")\n";
                textList.Add(lastText);
            }*/


            scroll_rect.verticalNormalizedPosition = 0.0f;
        }
        
    }
}

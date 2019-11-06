using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MoveScene : MonoBehaviour
{
    public Button btn;
    //public GameObject obj;
    //public int sceneNum = 1;
    public string sceneName_str;

    // Use this for initialization
    void Start()
    {
        btn.onClick.AddListener(MoveToSignUp);
    }

    public void MoveToSignUp()
    {
        SceneManager.LoadScene(sceneName_str);
        //Scene scene = SceneManager.GetSceneByBuildIndex(sceneNum);
        //SceneManager.MoveGameObjectToScene(obj, scene);
    }
}

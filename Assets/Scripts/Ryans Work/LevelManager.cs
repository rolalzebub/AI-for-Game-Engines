using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

    public static LevelManager instance = null;
    public string CurrentScene;

    public GameObject Player;


    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(gameObject);
        }

        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        if (CurrentScene == "GameLevel")
        {
            Player.transform.position = new Vector3(6, 0, 23);
        }

    }
    public void LoadScene(string SceneName)
    {
        CurrentScene = SceneName;
        SceneManager.LoadScene(SceneName);
        
    }
}

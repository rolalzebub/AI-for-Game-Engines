using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelManager : MonoBehaviour
{

    public static LevelManager instance = null;
    public string CurrentScene;
    public string PreviousScene;
    public GameObject Player;
    public int PlayerLives = 3;
    public int PlayerScore;
    public TextMeshProUGUI Score;
    public TextMeshProUGUI Lives;



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
        if (CurrentScene == "Dungeon")
        {
            GameObject ScoreObject = GameObject.Find("Score");
            Score = ScoreObject.GetComponent<TextMeshProUGUI>();
            Score.SetText("Score: " + PlayerScore.ToString());
            

            GameObject LivesObject = GameObject.Find("Lives");
            Lives = LivesObject.GetComponent<TextMeshProUGUI>();
            Lives.SetText("Lives: " + PlayerLives.ToString());
        }
        if (CurrentScene == "Graveyard")
        {
            Player.transform.position = new Vector3(34, 0, 6);
        }

        if (CurrentScene == "Dungeon" && PreviousScene == "Graveyard")
        {
            Player.transform.position = new Vector3(6, 0, 23);
        }

    }
    public void LoadScene(string SceneName)
    {
        PreviousScene = CurrentScene;
        CurrentScene = SceneName;
        SceneManager.LoadScene(CurrentScene);
        
    }

    public void LoseLives()
    {
        PlayerLives = PlayerLives - 1;
        Lives.SetText("Lives: " + PlayerLives.ToString());
    }

    public void GainScore(int ScoreAmount)
    {
        PlayerScore = PlayerScore + ScoreAmount;
        Score.SetText("Score: " + PlayerScore.ToString());
    }
}

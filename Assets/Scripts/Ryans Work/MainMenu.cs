using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public GameObject Manager;
    public LevelManager LevelManager;

    private void Awake()
    {
        LevelManager = Manager.GetComponent<LevelManager>();
    }
    public void PlayGame()
    {
        LevelManager.PreviousScene = "MainMenu";
        LevelManager.CurrentScene = "Dungeon";
        SceneManager.LoadScene("Dungeon");
    }


    public void QuitGame()
    {
        Application.Quit();
    }

}

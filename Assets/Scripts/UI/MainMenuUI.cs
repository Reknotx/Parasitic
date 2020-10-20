/// AUTHOR: Jeremy Casada
/// DATE: 9/15/2020
/// 
/// Used to Control UI of The Main Menu
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public GameObject levelSelect;
    public GameObject options;


    private void Start()
    {
        Application.targetFrameRate = Screen.currentResolution.refreshRate;
    }

    /// <summary>
    /// Shows Level Select Screen
    /// </summary>
    public void StartGame()
    {
        //SceneManager.LoadScene("scene to load");
        levelSelect.SetActive(true);
    }

    /// <summary>
    /// Show Options UI
    /// </summary>
    public void ShowOptions()
    {
        options.SetActive(true);
    }


    /// <summary>
    /// Loads Level by name using levelName
    /// </summary>
    /// <param name="levelName">name of level to be loaded</param>
    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }


    /// <summary>
    /// Exits the Game
    /// </summary>
    public void Quit()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    /// <summary>
    /// Returns to the Main portion of the Main Menu
    /// </summary>
    public void Return()
    {
        levelSelect.SetActive(false);
        options.SetActive(false);
    }
}
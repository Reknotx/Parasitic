/// AUTHOR: Jeremy Casada
/// DATE: 9/15/2020
/// 
/// Used to Control Menu UI while In-Game
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField]
    private GameObject pauseMenu;
    [SerializeField]
    private GameObject controlMenu;
    [SerializeField]
    private GameObject optionMenu;
    [SerializeField]
    private GameObject menuPrompt;
    [SerializeField]
    private GameObject exitPrompt;
    

    private bool _isPaused;


    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
    }

    /// <summary>
    /// Pause if not paused, otherwise Unpause
    /// </summary>
    public void Pause()
    {
        if(_isPaused)
        {
            HideAll();
            _isPaused = false;
            Time.timeScale = 1;
        }
        else
        {
            ShowMenu(pauseMenu);
            _isPaused = true;
            Time.timeScale = 0;
        }
    }

    

    /// <summary>
    /// Hides All Menus
    /// </summary>
    public void HideAll()
    {
        controlMenu.SetActive(false);
        pauseMenu.SetActive(false);
        exitPrompt.SetActive(false);
        menuPrompt.SetActive(false);
        optionMenu.SetActive(false);
    }

    /// <summary>
    /// Quits The Game
    /// </summary>
    public void Quit()
    {
        Application.Quit();
    }

    /// <summary>
    /// Either Returns to the Main Menu or Shows the Pause Menu based on value
    /// </summary>
    /// <param name="value">True = Go to Main Menu, False = Go Back to Pause Menu</param>
    public void ReturnToMain(bool value)
    {
        if(value)
        {
            Debug.Log("Return to Main Menu");
        }
        else
        {
            ShowMenu(pauseMenu);
        }
    }

    /// <summary>
    /// Hides Other Menus and Shows the Menu that is Input
    /// </summary>
    /// <param name="menu"> Menu to Show</param>
    public void ShowMenu(GameObject menu)
    {
        HideAll();
        menu.SetActive(true);
    }
}

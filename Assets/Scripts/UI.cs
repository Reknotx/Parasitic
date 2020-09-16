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
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
    }

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

    


    public void HideAll()
    {
        controlMenu.SetActive(false);
        pauseMenu.SetActive(false);
        exitPrompt.SetActive(false);
        menuPrompt.SetActive(false);
        optionMenu.SetActive(false);
    }


    public void Quit()
    {
        Application.Quit();
    }

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


    public void ShowMenu(GameObject menu)
    {
        HideAll();
        menu.SetActive(true);
    }
}

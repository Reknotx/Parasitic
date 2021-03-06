﻿/// AUTHOR: Jeremy Casada
/// DATE: 9/15/2020
/// 
/// Used to Control Menu UI while In-Game
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour
{
    public bool showHowToPlayOnStart = false; 
    public GameObject pauseMenu;
    public GameObject controlMenu;
    public GameObject optionMenu;
    public GameObject restartPrompt;
    public GameObject mainMenuPrompt;
    public GameObject quitPrompt;
    public GameObject pauseBG;
    public GameObject howToPlay;


    public Sprite screenCenterDefault, screenCenterHover, screenCenterClick;
    public Sprite cameraDefault, cameraHover, cameraClick;

    public static UI Instance;

    private bool _isPaused;

    public bool PausedStatus { get { return _isPaused; } }

    private CombatSystem combatSystem;

    private SpriteState cameraSpriteState;
    private SpriteState centerSpriteState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        Application.targetFrameRate = Screen.currentResolution.refreshRate;
        LoadAudioLevels();
        SetGraphics(PlayerPrefs.GetInt("Quality Level", 5));
        combatSystem = CombatSystem.Instance;
        
        if(showHowToPlayOnStart)
        {
            _isPaused = true;
        }
        else
        {
            Hide(pauseBG);
            Hide(howToPlay);
        }
        

        SetPivotSpriteState();

        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(_isPaused)
            {
                UnPause();
            }
            else if (CombatSystem.Instance.state != BattleState.Lost && CombatSystem.Instance.state != BattleState.Won && Time.timeSinceLevelLoad > 1)
            {
                Pause();
            }
        }
    }



    /// <summary>
    /// Pause Game
    /// </summary>
    public void Pause()
    {
        ShowMenu(pauseBG);
        ShowMenu(pauseMenu);
        _isPaused = true;
        Time.timeScale = 0;

    }

    /// <summary>
    /// Unpause Game
    /// </summary>
    public void UnPause()
    {
        HideAll();
        Hide(pauseBG);
        _isPaused = false;
        Time.timeScale = 1;
    }
    
    /// <summary>
    /// Disables the Specified Menu
    /// </summary>
    /// <param name="menuObject">Menu to Disable</param>
    public void Hide(GameObject menuObject)
    {
        menuObject.SetActive(false);
    }

    /// <summary>
    /// Hides All Menus
    /// </summary>
    public void HideAll()
    {
        Hide(controlMenu);
        Hide(pauseMenu);
        Hide(quitPrompt);
        Hide(mainMenuPrompt);
        Hide(restartPrompt);
        Hide(optionMenu);
        Hide(howToPlay);
    }

    /// <summary>
    /// Restarts the Level
    /// </summary>
    public void Restart()
    {
        Debug.Log("Restart Level");
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Quits The Game
    /// </summary>
    public void Quit()
    {
        Debug.Log("Quit Game");
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
            Time.timeScale = 1;
            SceneManager.LoadScene("MainMenuTest");
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

        //HideAll();
        menu.SetActive(true);
    }

    /// <summary>
    /// Opens Link Specified by input string "url"
    /// </summary>
    /// <param name="url">URL to be opened</param>
    public void OpenLink(string url)
    {
        Application.OpenURL(url);
    }

    #region Options
    public AudioMixer mixer;
    public Slider master, music, sfx, qualitySlider;


    public Text masterText, musicText, sfxText, qualityText;
    public Button camPivotButton;

    public void SetGraphics(float qualityIndex)
    {
        QualitySettings.SetQualityLevel((int)qualityIndex);
        PlayerPrefs.SetInt("Quality Level", (int)qualityIndex);
        qualityText.text = "Quality: " + QualitySettings.names[(int)qualityIndex];

        if (qualitySlider.value != PlayerPrefs.GetInt("Quality Level"))
        {
            qualitySlider.value = PlayerPrefs.GetInt("Quality Level");
        }
    }

    public void ToggleFullScreen()
    {
        //Debug.LogAssertion("(Previous) Width: " + Screen.width + "  " + "Height: " + Screen.height);
        //Debug.LogAssertion("Ratio: " + (float)Screen.width / Screen.height);
        if (Screen.fullScreen)
        {
            Screen.SetResolution((int)(Screen.width * 0.90), (int)(Screen.height * 0.90), false);
        }
        else
        {
            Screen.SetResolution((int)(Screen.currentResolution.width), (int)(Screen.currentResolution.height), true);
        }
        //StartCoroutine(DebugResolution());
    }

    public void SetPivotSpriteState()
    {
        cameraSpriteState = new SpriteState();
        cameraSpriteState.highlightedSprite = cameraHover;
        cameraSpriteState.pressedSprite = cameraClick;

        centerSpriteState = new SpriteState();
        centerSpriteState.highlightedSprite = screenCenterHover;
        centerSpriteState.pressedSprite = screenCenterClick;

        SetPivotState(CameraMovement.Instance.useRotateAround);
    }

    /// <summary>
    /// Toggles the Camera Pivot and Sets Text if isInitialSet is false, else just Sets Text
    /// </summary>
    public void ToggleCameraPivot()
    {
        
        if (CameraMovement.Instance.useRotateAround)
        {
            CameraMovement.Instance.useRotateAround = false;
            
        }
        else
        {
            CameraMovement.Instance.useRotateAround = true;
            
        }

        SetPivotState(CameraMovement.Instance.useRotateAround);

        PlayerPrefs.SetInt("Camera Pivot", CameraMovement.Instance.useRotateAround ? 1 : 0);
        

    }

    /// <summary>
    /// Sets Sprites based on rotateAround
    /// </summary>
    /// <param name="rotateAround">if true, pivot is center, else pivot is camera</param>
    public void SetPivotState(bool rotateAround)
    {
        if(rotateAround)
        {
            camPivotButton.image.sprite = screenCenterDefault;
            camPivotButton.spriteState = centerSpriteState;
        }
        else
        {
            camPivotButton.image.sprite = cameraDefault;
            camPivotButton.spriteState = cameraSpriteState;
        }
    }


    /// <summary>
    /// Sets the Volume of Master based on the float sliderValue
    /// </summary>
    /// <param name="sliderValue">value to set Master Volume to</param>
    public void SetMasterLevel(float sliderValue)
    {
        mixer.SetFloat("Master", Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat("MasterVol", Mathf.Log10(sliderValue) * 20);
        masterText.text = "Master: " + Mathf.Round(sliderValue * 100);

    }

    /// <summary>
    /// Sets the Volume of Music based on the float sliderValue
    /// </summary>
    /// <param name="sliderValue">value to set Music Volume to</param>
    public void SetMusicLevel(float sliderValue)
    {
        mixer.SetFloat("Music", Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat("MusicVol", Mathf.Log10(sliderValue) * 20);
        musicText.text = "Music: " + Mathf.Round(sliderValue * 100);

    }

    /// <summary>
    /// Sets the Volume of SFX based on the float sliderValue
    /// </summary>
    /// <param name="sliderValue">value to set SFX Volume to</param>
    public void SetSFXLevel(float sliderValue)
    {
        mixer.SetFloat("SFX", Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat("SFXVol", Mathf.Log10(sliderValue) * 20);
        sfxText.text = "SFX: " + Mathf.Round(sliderValue * 100);
    }

    /// <summary>
    /// Loads the Audio Levels from Player Prefs if Possible,
    /// if not then the current values are saved to Player Prefs
    /// </summary>
    private void LoadAudioLevels()
    {
        mixer.GetFloat("Master", out float mastValue);
        mixer.GetFloat("Music", out float musicValue);
        mixer.GetFloat("SFX", out float sfxValue);

        if (PlayerPrefs.HasKey("MasterVol"))
        {
            master.value = Mathf.Pow(10f, (PlayerPrefs.GetFloat("MasterVol") / 20));
            mixer.SetFloat("Master", PlayerPrefs.GetFloat("MasterVol"));
        }
        else
        {
            PlayerPrefs.SetFloat("MasterVol", mastValue);
            master.value = Mathf.Pow(10f, (mastValue / 20));
        }

        if (PlayerPrefs.HasKey("MusicVol"))
        {
            music.value = Mathf.Pow(10f, (PlayerPrefs.GetFloat("MusicVol") / 20));
            mixer.SetFloat("Music", PlayerPrefs.GetFloat("MusicVol"));
        }
        else
        {
            PlayerPrefs.SetFloat("MusicVol", musicValue);
            music.value = Mathf.Pow(10f, (musicValue / 20));
        }

        if (PlayerPrefs.HasKey("SFXVol"))
        {
            sfx.value = Mathf.Pow(10f, (PlayerPrefs.GetFloat("SFXVol") / 20));
            mixer.SetFloat("SFX", PlayerPrefs.GetFloat("SFXVol"));
        }
        else
        {
            PlayerPrefs.SetFloat("SFXVol", sfxValue);
            sfx.value = Mathf.Pow(10f, (sfxValue / 20));
        }



        masterText.text = "Master: " + Mathf.Round(master.value * 100);
        musicText.text = "Music: " + Mathf.Round(music.value * 100);
        sfxText.text = "SFX: " + Mathf.Round(sfx.value * 100);



    }
    #endregion
}

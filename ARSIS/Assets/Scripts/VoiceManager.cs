﻿using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 

/// <summary>
/// Manages all ADELE voice commands. 
/// </summary>
public class VoiceManager : MonoBehaviour {
    public static VoiceManager S; 

    private KeywordRecognizer _keywordRecognizer = null; 
    private readonly Dictionary<string, System.Action> _keywords = new Dictionary<string, System.Action>();
    private bool _visible = false;
    
    private MenuController mc;

    // Needed to check which settings menu is open for slider function 
    public GameObject m_brightnessMenu;
    public GameObject m_volumeMenu;

    [Header("Audio")]
    public AudioSource m_Source;
    public AudioSource m_musicSource; 

    public AudioClip m_OpenMenu;
    public AudioClip m_CloseMenu;
    public AudioClip m_ChangeMenu; 
    public AudioClip m_NextButton;
    public AudioClip m_BackButton;
    public AudioClip m_ZoomIn;
    public AudioClip m_ZoomOut;
    public AudioClip m_SliderSound; 
    
    void Start () {
        S = this; 

        #region keywords
        // Menus 
        _keywords.Add("Adele Main", MainMenu);
        _keywords.Add("Adele Settings", Settings);
        _keywords.Add("Adele Brightness", Brightness);
        _keywords.Add("Adele Volume", Volume);
        _keywords.Add("Adele Biometrics", Biometrics);
        _keywords.Add("Adele Houston", Houston);
        _keywords.Add("Adele Help", Help);
        _keywords.Add("Help", Help); 
        _keywords.Add("Adele Procedures", TaskList);

        // Navigation
        _keywords.Add("Adele Menu", Menu);
        _keywords.Add("Adele Move", Menu); 
        _keywords.Add("Adele Reset", ResetScene);
        _keywords.Add("Adele Clear", ResetScene);
        _keywords.Add("Adele Previous", Previous); 

        // Special Functions
        _keywords.Add("Increase", Increase);
        _keywords.Add("Decrease", Decrease);
        _keywords.Add("Adele Capture", TakePhoto);
        _keywords.Add("Adele Toggle", Toggle);

        // Task List 
        _keywords.Add("Adele Task", generateTaskMenu);
        _keywords.Add("Next", Next);
        _keywords.Add("Back", Back);
        _keywords.Add("Zoom Out", zoomOut);
        _keywords.Add("Zoom In", zoomIn);

        // Tasks 
        _keywords.Add("Disable Alarm", disableAlarm);
        _keywords.Add("Reroute Power", reroutePower);
        _keywords.Add("Light Switch", lightSwitch); 

        //Music
        _keywords.Add("Adele Hello", PlayAdele);
        _keywords.Add("Adele Africa", PlayAfrica);
        _keywords.Add("Adele Skyfall", PlaySkyfall);
        _keywords.Add("Adele Space Oddity", PlaySpaceOddity);
        _keywords.Add("Adele Thunderstruck", PlayThunderstruck);
        _keywords.Add("Adele Stop", StopMusic);
        _keywords.Add("Adele Music", musicMenu);
        _keywords.Add("Adele Eclipse", PlayEclipse);
        _keywords.Add("Adele Rocket Man", PlayRocketMan);

        #endregion

        // Sets up keyword recognition 
        _keywordRecognizer = new KeywordRecognizer(_keywords.Keys.ToArray());
        _keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        _keywordRecognizer.Start();

        // Initializes menu controller 
        mc = FindObjectOfType(typeof(MenuController)) as MenuController;
    }

    // Keyword Functions 
    #region Menu Functions

    public void MainMenu()
    {
        mc.ChangeMenu(mc.m_mainMenu); 
    }

    public void musicMenu()
    {
        mc.ChangeMenu(mc.m_musicMenu);
    }

    public void Settings()
    {
        mc.ChangeMenu(mc.m_settingsMenu); 
    }

    public void Houston()
    {
        mc.ChangeMenu(mc.m_sosMenu);
        ServerConnect.S.sos(); 
    }

    public void Help()
    {
        mc.ChangeMenu(mc.m_helpMenu); 
    }

    public void Biometrics()
    {
        mc.ChangeMenu(mc.m_biometricsMenu); 
    }

    public void Brightness()
    {
        mc.ChangeMenu(mc.m_brightnessMenu);
    }

    public void Volume()
    {
        mc.ChangeMenu(mc.m_volumeMenu);
    }

    public void TaskList()
    {
        mc.ChangeMenu(mc.m_taskList);  
    }

    #endregion

    #region Navigation Functions 

    public void Menu()
    {
        mc.ChangeMenu(mc.m_CurrentMenu);

        m_Source.clip = m_OpenMenu;
        m_Source.Play();
    }

    public void ResetScene()
    {
        m_Source.clip = m_CloseMenu;
        m_Source.Play();

        SceneManager.LoadScene(0);
    }

    public void Previous()
    {
        mc.GoBack(); 
    }

    #endregion

    #region Special Functions 

    public void TakePhoto()
    {
        HoloLensSnapshotTest.S.TakePhoto();

        m_Source.clip = m_ZoomOut;
        m_Source.Play();
    }

    public void Toggle()
    {
        HoloLensSnapshotTest.S.ToggleImage();

        m_Source.clip = m_ZoomIn;
        m_Source.Play();
    }

    public void Increase()
    {
        if (mc.m_CurrentMenu.Equals(m_brightnessMenu))
        {
            Debug.Log("Increasing Brightness");
            GameObject GOlt = GameObject.Find("Point light");
            Light lt = GOlt.GetComponent<Light>();
            if (lt.intensity < 1.4)
            {
                lt.intensity += 0.2f;
                SliderMove sm = mc.m_CurrentMenu.GetComponent<SliderMove>();
                sm.Increase();

                m_Source.clip = m_SliderSound;
                m_Source.Play();
            }
        }
        if (mc.m_CurrentMenu.Equals(m_volumeMenu))
        {
            Debug.Log("Increasing Volume");
            if (m_Source.volume < 1)
            {
                m_Source.volume += 0.2f;
                m_musicSource.volume += 0.2f; 
                SliderMove sm = mc.m_CurrentMenu.GetComponent<SliderMove>();
                sm.Increase();

                m_Source.clip = m_SliderSound;
                m_Source.Play();
            }
        }
    }

    public void Decrease()
    {
        if (mc.m_CurrentMenu.Equals(GameObject.Find("ToggleSliderMenu")))
        {
            GameObject GOlt = GameObject.Find("Point light");
            Light lt = GOlt.GetComponent<Light>();
            if (lt.intensity > 0.6)
            {
                lt.intensity -= 0.2f;
                SliderMove sm = mc.m_CurrentMenu.GetComponent<SliderMove>();
                sm.Decrease();

                m_Source.clip = m_SliderSound;
                m_Source.Play();
            }
        }
        if (mc.m_CurrentMenu.Equals(m_volumeMenu))
        {
            Debug.Log("Decreasing Volume");
            if (m_Source.volume > 0)
            {
                m_Source.volume -= 0.2f;
                m_musicSource.volume -= 0.2f;
                SliderMove sm = mc.m_CurrentMenu.GetComponent<SliderMove>();
                sm.Decrease();

                m_Source.clip = m_SliderSound;
                m_Source.Play();
            }
        }
    }

    #endregion

    #region Task List Functions 

    public void generateTaskMenu()
    {
        mc.ChangeMenu(mc.m_blankTaskMenu);
        displayStep();

        m_Source.clip = m_OpenMenu;
        m_Source.Play();
    }

    public void Next()
    {
        //int maxLength = TaskManager.S.allTasks[mc.currentTask];
        mc.currentStep++;
        displayStep();

        m_Source.clip = m_NextButton;
        m_Source.Play();
    }

    public void Back()
    {
        if (mc.currentStep <= 0) { return; }
        mc.currentStep--;
        displayStep();

        m_Source.clip = m_BackButton;
        m_Source.Play();
    }

    public void zoomOut()
    {
        mc.zoomOut();

        m_Source.clip = m_ZoomOut;
        m_Source.Play();
    }

    public void zoomIn()
    {
        mc.zoomIn();

        m_Source.clip = m_ZoomIn;
        m_Source.Play();
    }

    public void displayStep()
    {
        int curStep = mc.currentStep;
        int curTask = mc.currentTask;

        string curText = TaskManager.S.getStep(curTask, curStep);
        string prevText = TaskManager.S.getStep(curTask, curStep - 1);
        string nextText = TaskManager.S.getStep(curTask, curStep + 1);

        mc.m_stepText.text = curText;

        mc.m_stepPrevText.text = prevText;
        mc.m_stepCurText.text = curText;
        mc.m_stepNextText.text = nextText;

        Texture2D curImage = TaskManager.S.getPic(curTask, curStep);

        mc.m_stepImage.texture = curImage;

        string warningText = TaskManager.S.getWarning(curTask, curStep);
        mc.m_warningText.text = warningText;
    }

    #endregion

    #region Task Names

    public void disableAlarm()
    {
        mc.currentTask = 1;
        mc.currentStep = 1;
        generateTaskMenu();
    }

    public void reroutePower()
    {
        mc.currentTask = 2;
        mc.currentStep = 1;
        generateTaskMenu();
    }

    public void lightSwitch()
    {
        mc.currentTask = 3;
        mc.currentStep = 1;
        generateTaskMenu(); 
    }

    #endregion

    #region Music Functions 

    public void PlayAdele()
    {
        MusicManager.m_Instance.PlaySong(MusicManager.m_Instance.m_AdeleSong);
    }

    public void PlayAfrica()
    {
        MusicManager.m_Instance.PlaySong(MusicManager.m_Instance.m_Africa);
    }

    public void PlaySkyfall()
    {
        MusicManager.m_Instance.PlaySong(MusicManager.m_Instance.m_Skyfall);
    }

    public void PlaySpaceOddity()
    {
        MusicManager.m_Instance.PlaySong(MusicManager.m_Instance.m_SpaceOddity);
    }

    public void PlayThunderstruck()
    {
        MusicManager.m_Instance.PlaySong(MusicManager.m_Instance.m_Thunderstruck);
    }

    public void PlayEclipse()
    {
        MusicManager.m_Instance.PlaySong(MusicManager.m_Instance.m_Eclipse); 
    }

    public void PlayRocketMan()
    {
        MusicManager.m_Instance.PlaySong(MusicManager.m_Instance.m_RocketMan); 
    }

    public void StopMusic()
    {
        MusicManager.m_Instance.StopMusic();
    }
    #endregion 
    
    // Keyword Recognition 
    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        System.Action keywordAction; 
        if (_keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke(); 
        }
    }
}

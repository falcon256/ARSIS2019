using System.Collections;
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
    private GameObject menuToUse;

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
    private DictationRecognizer dictationRecognizer;

    public Image dot;
    public Text recognizedWord; 

    float dictationTimer = 5.0f;
    bool dictationIsOn = false; 

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
        _keywords.Add("Adele Procedures", ProcedureList);
        _keywords.Add("Adele Tasklist", TaskList);
        _keywords.Add("Adele Retrieve", Retrieve);

        // Navigation
       // _keywords.Add("Adele Menu", Menu);
       // _keywords.Add("Adele Move", Menu); 
        _keywords.Add("Adele Reset", ResetScene);
        _keywords.Add("Adele Clear", ResetScene);
       // _keywords.Add("Adele Previous", Previous);
        _keywords.Add("Adele Close", Close);

        // Special Functions
        _keywords.Add("Increase", Increase);
        _keywords.Add("Decrease", Decrease);
        _keywords.Add("Adele Capture", TakePhoto);
        _keywords.Add("Adele Toggle", Toggle);

        // Task List 
        _keywords.Add("Adele Task", generateTaskMenu);
        _keywords.Add("Next", Next);
        _keywords.Add("Continue", Next);
        _keywords.Add("Back", Back);
        _keywords.Add("Zoom Out", zoomOut);
        _keywords.Add("Zoom In", zoomIn);

        // Tasks 
       // _keywords.Add("Disable Alarm", disableAlarm);
       // _keywords.Add("Reroute Power", reroutePower);
       // _keywords.Add("Light Switch", lightSwitch); 

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

        //Translation 
        _keywords.Add("Adele Record Path", StartTranslation);
        _keywords.Add("Adele End Path", StopTranslation);
        _keywords.Add("Adele Show Path", ShowPath);
        _keywords.Add("Adele Hide Path", HidePath);

        //Mesh 
        _keywords.Add("Enable Mesh", enableMesh);
        _keywords.Add("Disable Mesh", disableMesh);
        _keywords.Add("Enable Mapping", enableMapping);
        _keywords.Add("Disable Mapping", disableMapping);

        _keywords.Add("Adele uh", Help);
        _keywords.Add("Adele um", Help);
        _keywords.Add("uh", Help);
        _keywords.Add("um", Help); 
        _keywords.Add("Adele shut up", PlaySkyfall); 

#endregion

        // Sets up keyword recognition 
        _keywordRecognizer = new KeywordRecognizer(_keywords.Keys.ToArray());
        _keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        _keywordRecognizer.Start();

        // Initializes menu controller 
        mc = FindObjectOfType(typeof(MenuController)) as MenuController;
    }

    public void resetKeywordRecognizer()
    {
        _keywordRecognizer.Stop();
        _keywordRecognizer.Dispose();
        _keywordRecognizer = new KeywordRecognizer(_keywords.Keys.ToArray());
        _keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        _keywordRecognizer.Start();
    }

    public void addProcedureCommand(string name, int index)
    {
        Debug.Log("Adding command " + name);
        _keywords.Add(name, () => {
            mc.currentProcedure = index;
            mc.currentTask = 0;
            mc.currentSubTask = 0;
            generateTaskMenu();
        });
        resetKeywordRecognizer();
    }


    // Keyword Functions 
#region Menu Functions

    public void MainMenu()
    {
        mc.addMenu(mc.m_mainMenu); 
    }

    public void musicMenu()
    {
        mc.addMenu(mc.m_musicMenu);
    }

    public void Settings()
    {
        mc.addMenu(mc.m_settingsMenu); 
    }

    public void Houston()
    {
        mc.addMenu(mc.m_sosMenu);
        ServerConnect.S.sos(); 
    }

    public void Help()
    {
        mc.addMenu(mc.m_helpMenu); 
    }

    public void Biometrics()
    {
        mc.addMenu(mc.m_biometricsMenu); 
    }

    public void Brightness()
    {
        mc.addMenu(mc.m_brightnessMenu);   
    }

    public void Volume()
    {
        mc.addMenu(mc.m_volumeMenu);
    }

    public void ProcedureList()
    {
        mc.addMenu(mc.m_procedureList);
    }

    public void TaskList()
    {
        mc.addMenu(mc.m_taskList);
        displayStep();
    }


    // handles voice cmds to retreive menu based off menu name
    public void Retrieve()
    {

        PhraseRecognitionSystem.Shutdown();

        dictationRecognizer = new DictationRecognizer();

        // start dictation reconizer
        dictationRecognizer.Start();
        Debug.Log("DicRec started");
        dictationRecognizer.DictationResult += DictationRecognizer_DictationResult;

        dictationIsOn = true;
        dictationTimer = 5.0f;
    }

    // handles voice cmds to decide to replace the menu
    public void Answer(GameObject holoMenu)
    {
        Debug.Log("Made it to Answer!");
        menuToUse = holoMenu;
        PhraseRecognitionSystem.Shutdown();

        dictationRecognizer = new DictationRecognizer();

        // start dictation reconizer
        dictationRecognizer.Start();
        dictationRecognizer.DictationResult += Dictation_yesNo;

        dictationIsOn = true;
        dictationTimer = 5.0f; 
    }



    private void DictationRecognizer_DictationResult(string text, ConfidenceLevel confidence)
    {
        GameObject holoMenu = null;
        Debug.Log("String heard: " + text);
        dictationRecognizer.Stop();


        // Cases to set holoMenu to the correct menu
        if (text.ToLower().Equals("main") || text.ToLower().Equals("main menu") || text.ToLower().Equals("maine") || text.ToLower().Equals("mean")) holoMenu = mc.m_mainMenu;
        else if (text.ToLower().Equals("biometrics")) holoMenu = mc.m_biometricsMenu;
        else if (text.ToLower().Equals("help")) holoMenu = mc.m_helpMenu;
        else if (text.ToLower().Equals("music")) holoMenu = mc.m_musicMenu;
        else if (text.ToLower().Equals("settings")) holoMenu = mc.m_settingsMenu;
        else if (text.ToLower().Equals("brightness")) holoMenu = mc.m_brightnessMenu;
        else if (text.ToLower().Equals("volume")) holoMenu = mc.m_volumeMenu;
        else if (text.ToLower().Equals("procedure")) holoMenu = mc.m_blankTaskMenu; 
        else
        {
            Debug.Log("Cmd not recognized.");
            // This does not fail eloquently
        }


        // call function in MenuController to retrieve the specific menu
        if (holoMenu != null) {
            mc.Retrieve(holoMenu);
        }

        dictationRecognizer.Dispose();
        PhraseRecognitionSystem.Restart();
    }

    private void Dictation_yesNo(string text, ConfidenceLevel confidence)
    {


        dictationRecognizer.Stop();
        // dispose of dictation reconizer

        if (text.ToLower().Equals("yes"))
        {
            Debug.Log("String heard: " + text);
            mc.ChangeMenu(menuToUse);
            //mc.toggleDisplay(menuToUse);

        }
        else 
        {
            Debug.Log("String heard: " + text);
            
        }

        mc.toggleDisplay(mc.m_overlapMessage);
        dictationRecognizer.Dispose();
        PhraseRecognitionSystem.Restart();
    }

    #endregion

#region Navigation Functions 

    public void Menu()
    {
        mc.m_blankTaskMenu.gameObject.SetActive(false); 
        mc.ChangeMenu(mc.m_blankTaskMenu);

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

    public void Close()
    {
        m_Source.clip = m_CloseMenu;
        m_Source.Play();

        mc.closeMenu();
    }

    #endregion

    #region Special Functions 

    public void TakePhoto()
    {
        VuforiaCameraCapture.S.TakePhoto(true);

        m_Source.clip = m_ZoomOut;
        m_Source.Play();
    }

    public void Toggle()
    {
        VuforiaCameraCapture.S.ToggleImage();

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
        mc.addMenu(mc.m_blankTaskMenu);
        displayStep();

        m_Source.clip = m_OpenMenu;
        m_Source.Play();
    }

    public void Next()
    {

        mc.currentSubTask++;
        //int maxLength = TaskManager.S.allTasks[mc.currentTask];
        if (mc.currentSubTask > TaskManager.S.GetTask(mc.currentProcedure, mc.currentTask).SubTasks.Length - 1)
        {
            //if there are no more subtasks, task is complete
            mc.currentSubTask = 0;
            mc.currentTask++;

            if (mc.currentTask > TaskManager.S.GetProcedure(mc.currentProcedure).Tasks.Length - 1)
            {
                //when procedure is complete
                mc.currentTask = 0;
                mc.currentProcedure++;
            }
        }

        displayStep();
        Debug.Log("sub task count: " + mc.currentSubTask);

        m_Source.clip = m_NextButton;
        m_Source.Play();
    }

    public void Back()
    {
        mc.currentSubTask--;
        //int maxLength = TaskManager.S.allTasks[mc.currentTask];
        if (mc.currentSubTask < 0)
        {
            mc.currentTask--;
            
            if (mc.currentTask < 0)//if there are no more subtasks, task is complete
            {
                //when procedure is complete
                mc.currentProcedure--;

                if (mc.currentProcedure < 0)
                {
                    mc.currentProcedure = 0; //ensure that procedure index is never less than 0
                    mc.currentTask = TaskManager.S.GetProcedure(mc.currentProcedure).Tasks.Length - 1;
                    mc.currentSubTask = TaskManager.S.GetTask(mc.currentProcedure, mc.currentTask).SubTasks.Length - 1;
                }
                else
                {
                    mc.currentTask = TaskManager.S.GetProcedure(mc.currentProcedure).Tasks.Length - 1;
                    mc.currentSubTask = TaskManager.S.GetTask(mc.currentProcedure, mc.currentTask).SubTasks.Length - 1;
                }
            }
            else
            {
                mc.currentSubTask = TaskManager.S.GetTask(mc.currentProcedure, mc.currentTask).SubTasks.Length - 1;
            }
        }

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
        int curProcedure = mc.currentProcedure;
        int curTask = mc.currentTask;
        int curSubTask = mc.currentSubTask;

        Debug.Log("Trying to display procedure " + mc.currentProcedure + "task" + mc.currentTask + " subtask " + mc.currentSubTask);

        string taskText = TaskManager.S.GetTask(curProcedure, curTask).Title;
        string curText = TaskManager.S.GetSubTask(curProcedure, curTask, curSubTask);
        string prevText = TaskManager.S.GetSubTask(curProcedure, curTask, curSubTask - 1);
        string nextText = TaskManager.S.GetSubTask(curProcedure, curTask, curSubTask + 1);

        mc.m_TaskText.text = taskText;
        mc.m_SubTaskText.text = curText;

        mc.m_stepPrevText.text = prevText;
        mc.m_stepCurText.text = curText;
        mc.m_stepNextText.text = nextText;

        Texture2D curImage = TaskManager.S.getPic(curProcedure, curTask, curSubTask);

        mc.m_stepImage.texture = curImage;

        string warningText = TaskManager.S.getWarning(curProcedure, curTask, curSubTask);
        mc.m_warningText.text = warningText;
    }

    private void Update()
    {
        if (dictationIsOn)
        {
            dictationTimer -= Time.deltaTime;
        }
        if (dictationTimer < 0)
        {
            destroyDictationRecognizer();
            dictationIsOn = false;
            dictationTimer = 5.0f;
            Debug.Log("Dictation stopped"); 
        }

        //For Debugging without voice
        if (Input.GetKeyDown(KeyCode.I))
        {
            TaskList();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            Next();
        }
    
    }

    void destroyDictationRecognizer()
    {
        if(mc.m_overlapMessage.gameObject.activeSelf)
        {
            mc.toggleDisplay(mc.m_overlapMessage);
        }
        dictationRecognizer.Stop();
        dictationRecognizer.Dispose();
        PhraseRecognitionSystem.Restart();
    }

    #endregion

    #region Task Names

    //public void disableAlarm()
    //{
    //    mc.currentTask = 1;
    //    mc.currentStep = 1;
    //    generateTaskMenu();
    //}

    //public void reroutePower()
    //{
    //    mc.currentTask = 2;
    //    mc.currentStep = 1;
    //    generateTaskMenu();
    //}

    //public void lightSwitch()
    //{
    //    mc.currentTask = 3;
    //    mc.currentStep = 1;
    //    generateTaskMenu(); 
    //}

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
        m_Source.Stop();
    }
#endregion

#region Translation 

    void StartTranslation()
    {
        TranslationController.S.startPathCapture(); 
    }

    void StopTranslation()
    {
        TranslationController.S.stopPathCapture(); 
    }

    void ShowPath()
    {
        TranslationController.S.showPath(); 
    }

    void HidePath()
    {
        TranslationController.S.hidePath();
    }

#endregion

#region Mesh

    public void enableMesh()
    {
        MeshDataGatherer.S.enableMeshDisplay(); 
    }


    public void disableMesh()
    {
        MeshDataGatherer.S.disableMeshDisplay();
    }

    public void enableMapping()
    {

    }

    public void disableMapping()
    {

    }

#endregion
    // Keyword Recognition 
    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        dot.color = Color.red;
        string input = args.text; 
        if (input.ToLower().Contains("adele"))
        {
            string[] array = input.Split(' ');
            input = "";
            for (int i = 1; i < array.Length; i++)
            {
                input += array[i];
                input += " ";
            }
        }
        recognizedWord.text = input;
        Invoke("dotWhite", 1f);
       
        System.Action keywordAction; 
        if (_keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke(); 
        }
    }

    private void dotWhite()
    {
        recognizedWord.text = "";
        dot.color = Color.white; 
    }
}

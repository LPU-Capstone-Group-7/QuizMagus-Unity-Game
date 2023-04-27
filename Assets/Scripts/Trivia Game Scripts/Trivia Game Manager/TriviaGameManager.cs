using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

public class TriviaGameManager : MonoBehaviour
{
    private TriviaGameSettings triviaGameSettings;
    private List<TriviaData> triviaDataList;
    private TriviaQuestion[] inputtedTriviaQuestions;
    private TriviaQuestion currentQuestion;
    private int level = 1;
    public Action onLevelFinished;
    private bool isTriviaGamePaused = false;
    private bool isGameFinished = false;

    [Header("Game Timer")]
    private TriviaGameTimer triviaGameTimer;
    private float currentTimePerQuestion;

    [Header("InputField UI Components")]
    [SerializeField] private TMP_InputField inputField;

    [Header("Text UI Components")]
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI numberText;

    [Header("TriviaGame UI Animator")]
    [SerializeField] private Animator questionBoardAnimator;
    [SerializeField] private Animator inputFieldAnimator;

    [Header("Camera Shake Components")]
    [SerializeField] private float shakeIntensity;
    [SerializeField] private float shakeDuration;

    private HintManager hintManager;

    void Start()
    {

        triviaGameSettings = DataManager.instance.GetGameSettings<TriviaGameSettings>();
        Debug.Log(triviaGameSettings.triviaQuestions.Length);
        //COPY THE ELEMENTS OF THE TRIVIA QUESTIONS ARRAY TO RETAIN THE ORIGINAL ARRAY INCASE OF NEEDING TO RANDOMIZE IT
        inputtedTriviaQuestions = new TriviaQuestion[triviaGameSettings.triviaQuestions.Length];
        if(triviaGameSettings == null){Debug.Log("Wrong Parse");}
        System.Array.Copy(triviaGameSettings.triviaQuestions, inputtedTriviaQuestions, triviaGameSettings.triviaQuestions.Length);

        //RANDOMIZES ORDER WHILE ALSO RETAINING THE ORGINAL ORDER OF THE ARRAY
        if(triviaGameSettings.randomizeQuestions){
            inputtedTriviaQuestions = GameManager.instance.ShuffleTriviaQuestions(inputtedTriviaQuestions);
        }

        hintManager = GameObject.FindObjectOfType<HintManager>().GetComponent<HintManager>();

        triviaGameTimer = GameObject.FindObjectOfType<TriviaGameTimer>().GetComponent<TriviaGameTimer>();
        triviaGameTimer.StartTimer(triviaGameSettings.timePerQuestions);

        //UPDATE HINT BASED ON CURRENT TIME
        if(triviaGameSettings.enableHints)
        {
            triviaGameTimer.OnTimeChange += () => {UpdateHint(75, 10);};
            triviaGameTimer.OnTimeChange += () => {UpdateHint(50, 25);};
        }

        //EVENT HANDLER WHEN TIMER RUNS OUT
        triviaGameTimer.OnTimeFinish += LoadTimerElapseTransition;

        triviaDataList = new List<TriviaData>();
        GenerateQuestion(level);
        inputField.onSubmit.AddListener(delegate{inputField.ActivateInputField();});

        //SUBSCRIBE TO MINI GAME LOAD EVENTS
        MiniGameManager.instance.onMiniGameLoads += PauseTriviaGame;
        MiniGameManager.instance.onMiniGameUnloads += ResumeTriviaGame;

    }

    void Update()
    {
        if(!inputField.isFocused){inputField.Select();}

        if(!triviaGameTimer.IsTimerUIPaused()){currentTimePerQuestion += Time.deltaTime;}

        if(Input.GetKeyDown(KeyCode.Return) && inputField.text != "")
        {

            if(IsAnswerCorrect(inputField.text)) //DO THIS WHEN ANSWER IS CORRECT
            {
                //CORRECT SFX
                AudioManager.instance.Play("Correct");

                hintManager.ShowAnswer();
                AddTriviaDataToList(currentQuestion, inputField.text.ToLower(), Mathf.FloorToInt(currentTimePerQuestion), true); //PUT NUMBER IN CORRECT ANSWER ARRAY
                currentTimePerQuestion = 0;
            }
            else //ELSE DO THIS
            {
                //ERROR SFX
                hintManager.ShowAnswer("incorrect");
                PlayErrorVisualFeedback();
                AddTriviaDataToList(currentQuestion, inputField.text.ToLower(), Mathf.FloorToInt(currentTimePerQuestion), false);  //PUT NUMBER IN WRONG ANSWER ARRAY
                currentTimePerQuestion = 0;
            }

            level++;
            //PAUSE THE TIMER THEN SHOWS THE CORRECT ANSWER AND THEEEEN LOAD THE NEXT QUESTION
            CloseTriviaGameUI();

            triviaGameTimer.PauseTimerUI(true);
            inputField.enabled = false;

            FunctionTimer.Create(() =>{
                LoadNextQuestion(level);
            }, 1.5f);
        }
    }

    //ADD TRIVIA DATA TO THE TRIVIA DATA LIST
    private void AddTriviaDataToList(TriviaQuestion currentQuestion, string inputtedAnswer, float timeToAnswer, bool isCorrect)
    {
        triviaDataList.Add(new TriviaData(currentQuestion.id, timeToAnswer, isCorrect, currentQuestion.question, currentQuestion.answer, inputtedAnswer));
    }

    public void CloseTriviaGameUI(){
        
        FunctionTimer.Create(() => {
            questionBoardAnimator.Play("QuestionBoard_Close");
            inputFieldAnimator.Play("InputField_Close");
            onLevelFinished?.Invoke();
            hintManager.EnableHintText(false);
        },1f);
    }

    public void PauseTriviaGame()
    {
        //PAUSE TIMER.... DIASBLE KEYBOARD INPUT
        if(!isTriviaGamePaused)
        {
            triviaGameTimer.PauseTimerUI(true);
            inputField.enabled = false;
            isTriviaGamePaused = true;
        }
    }

    public void ResumeTriviaGame()
    {
        //RESUME TIMER.... ACTIVE INPUT FIELD
        if(isTriviaGamePaused)
        {
            triviaGameTimer.PauseTimerUI(false);
            inputField.enabled = true;
            inputField.ActivateInputField();
            isTriviaGamePaused = false;
        }
    }
    
    private void LoadTimerElapseTransition()
    {
        level++;
        AddTriviaDataToList(currentQuestion, "", currentTimePerQuestion, false);; //PUT NUMBER IN WRONG ANSWER ARRAY
        PlayErrorVisualFeedback();
        currentTimePerQuestion = 0;
        hintManager.ShowAnswer("incorrect");;

        //PAUSE THE TIMER THEN SHOWS THE CORRECT ANSWER AND THEEEEN LOAD THE NEXT QUESTION
        CloseTriviaGameUI();

        //REOPEN ALL UI BACK IN THE SCENE AND LOADS NEXT QUESTION
        FunctionTimer.Create(() => {
            LoadNextQuestion(level); 
        }, 2f);

    }

    void PlayErrorVisualFeedback()
    {
        CinemachineCameraShake.instance.ShakeCamera(shakeIntensity, shakeDuration);
        AudioManager.instance.Play("Thud");
    }

    void LoadNextQuestion(int level)
    {
        if(level > triviaGameSettings.triviaQuestions.Length)//THERE ARE NO MORE QUESTIONS LEFT TO ANSWER THEN LOAD THE RESULT SCENE
        {
            triviaGameTimer.PauseTimerUI(false);
            isGameFinished = true;
            TriviaGameResultManager.instance.SetTriviaGameResults(triviaDataList.ToArray(), triviaGameSettings.basedGrading);
            GameManager.instance.LoadLevel("TriviaGameCredits");
            return;
        }

        //RESETS THE TIMER
        triviaGameTimer.SetTimer(SetTimerBasedOnDifficulty(triviaGameSettings.timePerQuestions, inputtedTriviaQuestions[level-1].difficulty));
        triviaGameTimer.ResetTimer();
        triviaGameTimer.PauseTimerUI(false);

        //OPENS QUESTION BOARD
        questionBoardAnimator.Play("QuestionBoard_Open");
        inputFieldAnimator.Play("InputField_Open");

        //CLEAR INPUT FIELD AND ENABLE IT BACK
        hintManager.EnableHintText(true);
        inputField.enabled = true;
        inputField.text = "";

        //SCREEN TRANSIITION AND THEN LOAD NEXT QUESTION AFTER A FEW SECONDS DELAY
        GenerateQuestion(level);
    }

    void GenerateQuestion(int level) //GENERATE QUESTIONS TO BE ANSWERED BY USER
    {
        currentQuestion = inputtedTriviaQuestions[level-1];
        questionText.text = currentQuestion.question;
        numberText.text = "#" + level.ToString();

        hintManager.HideAnswer(currentQuestion.answer);
    }

    float SetTimerBasedOnDifficulty(float defaultTime, string difficulty)
    {
        switch (difficulty)
        {
            case "normal":  return defaultTime * 1.25f;
            case "hard":    return defaultTime * 1.5f;
            default:        return defaultTime;
        }
    }

    bool IsAnswerCorrect(string answer)
    {
        return answer.ToUpper() == currentQuestion.answer.ToUpper();
    }


    void UpdateHint(float timePercentage, int percentageOfLetter)
    {
        float currentTime = triviaGameTimer.GetCurrentTime();
        float timeToUpdate = triviaGameSettings.timePerQuestions * (timePercentage/100f);
        if(currentTime <= timeToUpdate)
        {
            hintManager.ShowPercentageOfLetters(percentageOfLetter);
        }
    }

    public int GetCurrentLevel()
    {
        return level;
    }

    internal bool IsGameFinished()
    {
        return isGameFinished;
    }
}

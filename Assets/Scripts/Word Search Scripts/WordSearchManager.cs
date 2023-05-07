using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using System;

public class WordSearchManager : MonoBehaviour
{
    public static WordSearchManager instance;
    private WordSearchSettings wordSearchSettings;
    private float preferredGridSize;
    private List<string> answers = new List<string>();

    [Header("Grid Components")]
    [SerializeField] int gridSize;
    [SerializeField] float gridLength;
    private WordSearchGridManager gridManager;
    private bool allowBackwards = false;
    private bool allowDiagonals = false;

    [Header("Word Search Quiz Components")]
    private int level = 1;
    private TriviaQuestion[] inputtedTriviaQuestions;
    private TriviaQuestion currentQuestion;
    private List<TriviaData> triviaDataList = new List<TriviaData>();
    private List<string> currentWordSearchContents = new List<string>();

    [Header("Word Search UI Components")]
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI answerText;

    [Header("Word Search Animator Components")]
    [SerializeField] private Animator questionBoardAnimator;

    [Header("EventHandler Shits")]
    public Action onQuestionLoads;

    private void Awake()
    {
        if(instance == null)
        {
            instance= this;
        }    
    }
    // Start is called before the first frame update
    void Start()
    {
        //GET WORD SEARCH UNIQUE GAME SETTINGS
        gridManager = WordSearchGridManager.instance;
        wordSearchSettings = DataManager.instance.GetGameSettings<WordSearchSettings>();

        //INITIALIZE REQUIRED PARAMETERS AND LISTS
        inputtedTriviaQuestions = new TriviaQuestion[wordSearchSettings.triviaQuestions.Length];
        System.Array.Copy(wordSearchSettings.triviaQuestions, inputtedTriviaQuestions, wordSearchSettings.triviaQuestions.Length);

        //RANDOMIZES ORDER WHILE ALSO RETAINING THE ORGINAL ORDER OF THE ARRAY
        if(wordSearchSettings.randomizeQuestion) inputtedTriviaQuestions = GameManager.instance.ShuffleTriviaQuestions(inputtedTriviaQuestions);
        
        answers = GetTriviaAnswersList(inputtedTriviaQuestions);
        gridSize =  GetLongestAnswerLength(answers) > wordSearchSettings.maxNumOfChoices? GetLongestAnswerLength(answers) : wordSearchSettings.maxNumOfChoices;
        
        //GENERATE FIRST QUESTION
        currentQuestion = inputtedTriviaQuestions[level-1];
        List<string> wordSearchContents = GetWordSearchChoices(currentQuestion.answer, answers, wordSearchSettings.maxNumOfChoices);
        
        //DISPLAY UI CHUCHU
        DisplayWordSearchQuestion(currentQuestion);
        WordSearchTextUI.instance.DisplayWordSearchAnswers(wordSearchContents);
        DisplayAllowableMistakeUI(currentQuestion.difficulty);

        WordSearchGridManager.instance.CreateWordSearchGrid(gridSize, gridLength, wordSearchContents, wordSearchSettings.allowBackwards, wordSearchSettings.allowDiagonals);

        //START TIMER AND SUBSCRIBE TO ON TIMER EVENTS
        WordSearchTimer.instance.StartWordSearchTimer(wordSearchSettings.timeLimit);
        WordSearchTimer.instance.onTimeFinished += ForceEndWordSearchGame;

    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            ForceEndWordSearchGame();
        }
    }

    private void LoadWordSearchQuestion(int level)
    {
        //CHECK IF USER HAS ANSWERED ALL POSSIBLE QUESTIONS
        if(level > inputtedTriviaQuestions.Length)
        {
            LoadGameCredits();
            return;
        }

        //TRIGGER QUESTION LOAD ACTION
        onQuestionLoads?.Invoke();

        //REFRESH WORD SEARCH GRID WITH CURRENT QUESTION ANSWER
        currentQuestion = inputtedTriviaQuestions[level-1];

        if(currentWordSearchContents.Count == 0)
        {
            //LEVEL CHANGE TRANSITION
            CloseWordSearchUI();
            
            List<string> wordSearchContents = GetWordSearchChoices(currentQuestion.answer, answers , wordSearchSettings.maxNumOfChoices);
            WordSearchGridManager.instance.RefreshWordSearchContents(wordSearchContents);

            //OPEN QUESTION BOARD UI
            FunctionTimer.Create(() => {
                WordSearchTextUI.instance.DisplayWordSearchAnswers(wordSearchContents);
                questionBoardAnimator.Play("QuestionBoard_Open");
            },0.8f);
        }


        //REFRESH UI ELEMENTS
        FunctionTimer.Create(() => {
            DisplayAllowableMistakeUI(currentQuestion.difficulty);
            DisplayWordSearchQuestion(currentQuestion);
        }, 0.8f);
    }

    private void DisplayWordSearchQuestion(TriviaQuestion question)
    {
        questionText.text = currentQuestion.question;
    }

    private void DisplayAllowableMistakeUI(string difficulty)
    {
        //DISPLAY ALLOWABLE ANSWER ATTEMPTS DEPENDING ON THE DIFFICULTY OF THE CURRENT QUESTION
        int amount = WordSearchMistakeCounter.instance.GetNumberOfAllowableMistake(currentQuestion.difficulty);
        WordSearchMistakeCounter.instance.CreateMistakeCounterUI(amount);
    }

    public void AnswerWordSearchQuestion(string answer, out bool isCorrect)
    {
        WordSearchTimer.instance.CountTimeTakenToAnswer(false);
        //CHECK IF ANSWER IS CORRECT
        if(currentQuestion.answer == answer)
        {
            isCorrect = true;
            AudioManager.instance.Play("Correct");
            WordSearchTextUI.instance.CrossOutWord(answer);
        }
        else
        {
            isCorrect = false;
            WordSearchMistakeCounter.instance.DecreaseRemainingAllowableMistake();
            CinemachineCameraShake.instance.ShakeCamera(.5f,.25f);
            AudioManager.instance.Play("Thud");
        }

        //PLAYER MADE A MISTAKE BUT STILL HAS REMAINING ATTEMPTS
        if(WordSearchMistakeCounter.instance.GetRemainingAllowableMistake() > 0 && !isCorrect)
        {
            Debug.Log("Lozlz");
            WordSearchTimer.instance.CountTimeTakenToAnswer(true);
            return;
        }

        //REMOVE FROM CURRENT WORD SEARCH CONTENTS
        currentWordSearchContents.Remove(currentQuestion.answer);

        //ADD ANSWER TO TRIVIA DATA LIST
        AddTriviaDataToList(currentQuestion, answer, WordSearchTimer.instance.GetTimeTakenToAnswer(), isCorrect);

        //LOAD NEXT QUESTION
        level++;
        LoadWordSearchQuestion(level);
    }

    private void AddTriviaDataToList(TriviaQuestion currentQuestion, string inputtedAnswer, float timeToAnswer, bool isCorrect)
    {
        triviaDataList.Add(new TriviaData(currentQuestion.id, timeToAnswer, isCorrect, currentQuestion.question, currentQuestion.answer, inputtedAnswer));
    }

    private void ForceEndWordSearchGame()
    {
        //ADD THE CURRENT TRIVIA DATA INTO THE TRIVIA DATA LIST
        AddTriviaDataToList(currentQuestion, "", WordSearchTimer.instance.GetTimeTakenToAnswer(), false);

        //ADD THE REMAINING QUESTIONS TO TRIVIA DATA
        int startingIndex = triviaDataList.Count;
        for (int i = startingIndex; i < inputtedTriviaQuestions.Length; i++)
        {
            AddTriviaDataToList(inputtedTriviaQuestions[i], "", 0f, false);
        }
        
        //LOAD GAME CREDITS
        LoadGameCredits();
    }

    private List<string> GetWordSearchChoices(string correctAnswer, List<string> choices, int maxNumOfChoices)
    {
        //GET INDEX OF CURRENT CORRECT ANSWER
        int currentIndex = choices.IndexOf(correctAnswer);
        //Debug.Log("CurrentIndex: " + currentIndex + ", NumOfChoices: " + choices.Count);
        List<string> wordSearchContents = new List<string>();
        //wordSearchContents.Add(correctAnswer);

        for (int i = 0; i < maxNumOfChoices; i++)
        {
            if(i+currentIndex >= choices.Count) break;
            wordSearchContents.Add(choices[i + currentIndex]);
        }

        Debug.Log(wordSearchContents.Count);
        //COPY CONTENTS AS CURRENT CONTENTS TO KEEP TRACK OF THE CURRENT NUMBER OF CONTENTS
        currentWordSearchContents = new List<string>();
        foreach (string word in wordSearchContents)
        {
            currentWordSearchContents.Add(word);
        }

        return wordSearchContents;
    }

    private List<string>GetTriviaAnswersList(TriviaQuestion[] triviaQuestions)
    {
        List<string> answers = new List<string>();

        foreach (TriviaQuestion triviaQuestion in triviaQuestions)
        {
            answers.Add(triviaQuestion.answer);
        }

        return answers;
    }

    //GET LONGEST ANSWER CHARACTER LENGTH
    private int GetLongestAnswerLength(List<string> words)
    {
        int longestLength = 0;

        for (int i = 0; i < words.Count; i++)
        {
            int currentAnswerLength = words[i].Replace(" ","").Length;
            if(currentAnswerLength >= longestLength) longestLength = currentAnswerLength;
        }

        return longestLength;
    }
    public TriviaQuestion GetCurrentQuestion()
    {
        return currentQuestion;
    }
    public List<string> GetCurrentWordSearchContents()
    {
        return currentWordSearchContents;
    }
    private void LoadGameCredits()
    {
        TriviaGameResultManager.instance.SetTriviaGameResults(triviaDataList.ToArray(), wordSearchSettings.basedGrading);
        GameManager.instance.LoadLevel("TriviaGameCredits");
    }
    private void CloseWordSearchUI()
    {
        FunctionTimer.Create(() => {
            questionBoardAnimator.Play("QuestionBoard_Close");
        }, 0f);
    }
}

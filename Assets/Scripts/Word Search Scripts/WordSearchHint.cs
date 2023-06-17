using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class WordSearchHint : MonoBehaviour
{
    public static WordSearchHint instance;
    [SerializeField] private Transform hintButtonTransform;
    private Animator hintButtonAnimator;
    public float timeToShowHint;
    private bool isClickable;
    private bool buttonWasClicked = false;

    private void Awake()
    {
        if(instance == null) instance = this;
        hintButtonTransform.gameObject.SetActive(false);    
    }

    private void Start()
    {
        WordSearchManager.instance.onQuestionLoads += () => {
            if(hintButtonTransform.gameObject.activeInHierarchy) HideButton();
            buttonWasClicked = false;
        };
        hintButtonAnimator = hintButtonTransform.GetComponent<Animator>();    
    }

    private void Update()
    {
        if(timeToShowHint <= WordSearchTimer.instance.GetTimeTakenToAnswer() && WordSearchManager.instance.GetCurrentWordSearchContents().Count > 1 && !hintButtonTransform.gameObject.activeInHierarchy && !buttonWasClicked && WordSearchManager.instance.enableHints())
        {
            ShowButton();
        }
    }

    public void HideButton()
    {
        hintButtonAnimator.Play("HintButton_Close");
        FunctionTimer.Create(() => {
            hintButtonTransform.gameObject.SetActive(false);
        }, 0.3f);
    }

    public void ShowButton()
    {
        hintButtonTransform.gameObject.SetActive(true);
        hintButtonAnimator.Play("HintButton_Enter");
        FunctionTimer.Create(() => isClickable = true, 0.3f);
    }

    public void onHintButtonClick()
    {
        if(isClickable)
        {
            ShowHint(WordSearchManager.instance.GetCurrentQuestion(), WordSearchManager.instance.GetCurrentWordSearchContents());
            isClickable = false;
            buttonWasClicked = true;
        }
    }

    private void ShowHint(TriviaQuestion currentQuestion, List<string> answerList)
    {
        List<string> hintedWords = new List<string> { currentQuestion.answer };
        List<string> availableWords = answerList.Except(hintedWords).ToList();
        int numberOfWordsToShow = Mathf.Max(Mathf.CeilToInt(answerList.Count/2), 2);

        if(answerList.Count > 1)
        {
            // System.Random random = new System.Random();

            // while(hintedWords.Count < numberOfWordsToShow && availableWords.Count > 0)
            // {
            //     int randomIndex = random.Next(availableWords.Count);
            //     hintedWords.Add(availableWords[randomIndex]);
            //     availableWords.Remove(availableWords[randomIndex]);
            // }

            WordSearchGridManager.instance.HighlightWordsInGrid(answerList, 1f);
        }

        HideButton();
    }
}

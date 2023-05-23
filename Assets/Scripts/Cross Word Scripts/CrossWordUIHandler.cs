using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class CrossWordUIHandler : MonoBehaviour
{
    public static CrossWordUIHandler instance;
    public CrossWordClue activeClue;

    private bool isUIActive = false;

    [Header("UI Ticker")]
    [SerializeField] TickerUI tickerUI;

    [Header("Clue Slider")]
    [SerializeField] private Transform questionItemTextPrefab;
    [SerializeField] private Transform downListTransform;
    [SerializeField] private Transform acrossListTransform;

    [Header("CrossWord End Button")]
    [SerializeField] GameObject confirmationWindow;
    bool confirmationWindowIsActive = false;

    [Header("UI Animator")]
    [SerializeField] private Animator sliderAnimator;
    [SerializeField] private Animator overlayAnimator;
    private Dictionary<int,TextMeshProUGUI> questionItemTextDictionary = new Dictionary<int, TextMeshProUGUI>();

    private void Awake() 
    {
        if(instance == null) instance = this;    
    }

    private void Start()
    {
        CrossWordManager.instance.OnActiveClueChangeAction += ChangeActiveClue;
        CrossWordManager.instance.onCrossWordItemAnswered += UpdateAnsweredText;
    }

    private void ChangeActiveClue(CrossWordClue clue)
    {
        if(activeClue.triviaQuestion == clue.triviaQuestion && activeClue.startNode == clue.startNode) return;
        tickerUI.CreateNewTicker(clue.triviaQuestion.question);
    }

    public void ListAllClues(List<CrossWordGridItem> clues)
    {
        //SORT CLUES BY INDEX
        clues.Sort((a, b) => a.index.CompareTo(b.index));

        //SPAWN THEM IN THE DESIGNATED LAYOUT GROUP
        for (int i = 0; i < clues.Count; i++)
        {
            TextMeshProUGUI questionItemText = Instantiate(questionItemTextPrefab, Vector3.zero, Quaternion.identity).GetComponent<TextMeshProUGUI>();
            questionItemText.text = clues[i].index + ". " + clues[i].triviaQuestion.question;

            if(clues[i].orientation == Orientation.across)
            {
                questionItemText.transform.SetParent(acrossListTransform, false);
            }
            else
            { 
                questionItemText.transform.SetParent(downListTransform, false);
            }

            questionItemTextDictionary.Add(clues[i].index, questionItemText);
        }

    }

    void UpdateAnsweredText(CrossWordGridItem item)
    {
        if(questionItemTextDictionary.ContainsKey(item.index))
        {
            questionItemTextDictionary[item.index].fontStyle = FontStyles.Strikethrough;

            //CHANGE ALPHA TO HALF
            Color currentColor = questionItemTextDictionary[item.index].color;
            currentColor.a = 0.5f;
            questionItemTextDictionary[item.index].color = currentColor;
        }
    }

    public void ShowCLueListUI()
    {
        if(isUIActive) return;

        sliderAnimator.Play("ClueList_Enter");
        ActivateUIOverlay();
    }

    public void HideClueListUI()
    {
        sliderAnimator.Play("ClueList_Exit");
        DisableUIOverlay();
    }


    /*
    * CROSSWORD END BUTTON FUNCTIONS
    */

    public void ClickEndButton()
   {
        if(!confirmationWindowIsActive) ShowConfirmationWindow();
   }

   private void ShowConfirmationWindow()
   {
        if(isUIActive) return;

        confirmationWindowIsActive = true;
        confirmationWindow.SetActive(true);
        ActivateUIOverlay();
   }

    public void HideConfirmationWindow()
    {
        confirmationWindowIsActive = false;
        confirmationWindow.SetActive(false);
        DisableUIOverlay();
    }

    public void EndGame()
    {
        CrossWordManager.instance.CalculateCrossWordResults();
        confirmationWindow.SetActive(false);
    }

    /*
    * UI OVERLAY FUNCTIONS
    */

    private void ActivateUIOverlay()
    {
        overlayAnimator.Play("Overlay_Show");
        isUIActive = true;
        CrossWordManager.instance.SetCanSelectTiles(false);
        CameraDragController.instance.EnableCameraDrag(false);
    }

    private void DisableUIOverlay()
    {
        overlayAnimator.Play("Overlay_Hide");
        isUIActive = false;
        CameraDragController.instance.EnableCameraDrag(true);
        CrossWordManager.instance.SetCanSelectTiles(true);
    }
}

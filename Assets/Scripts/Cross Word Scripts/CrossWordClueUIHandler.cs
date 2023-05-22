using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class CrossWordClueUIHandler : MonoBehaviour
{
    public static CrossWordClueUIHandler instance;
    public CrossWordClue activeClue;

    [Header("UI Ticker")]
    [SerializeField] TickerUI tickerUI;

    [Header("Clue Slider")]
    [SerializeField] private Transform questionItemTextPrefab;
    [SerializeField] private Transform downListTransform;
    [SerializeField] private Transform acrossListTransform;
    [SerializeField] private Animator sliderAnimator;
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
        sliderAnimator.Play("ClueList_Enter");
        CrossWordManager.instance.SetCanSelectTiles(false);
    }

    public void HideClueListUI()
    {
        sliderAnimator.Play("ClueList_Exit");
        CrossWordManager.instance.SetCanSelectTiles(true);
    }
}

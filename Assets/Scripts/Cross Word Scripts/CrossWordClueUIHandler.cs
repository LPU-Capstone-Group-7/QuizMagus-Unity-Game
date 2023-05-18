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

    private void Awake() 
    {
        if(instance == null) instance = this;    
    }

    private void Start()
    {
        CrossWordManager.instance.OnActiveClueChangeAction += ChangeActiveClue;
    }

    private void ChangeActiveClue(CrossWordClue clue)
    {
        if(activeClue.triviaQuestion == clue.triviaQuestion && activeClue.startNode == clue.startNode) return;

        Debug.Log(clue.triviaQuestion.question);
        tickerUI.CreateNewTicker(clue.triviaQuestion.question);
    }

    public void ListAllClues(List<CrossWordGridItem> clues)
    {
        //SORT CLUES BY INDEX
        clues.Sort((a, b) => a.index.CompareTo(b.index));

        for (int i = 0; i < clues.Count; i++)
        {
            TextMeshProUGUI questionItemText = Instantiate(questionItemTextPrefab, Vector3.zero, Quaternion.identity).GetComponent<TextMeshProUGUI>();
            // questionItemText.rectTransform.localScale = Vector3.one;
            questionItemText.text = clues[i].index + ". " + clues[i].triviaQuestion.question;

            if(clues[i].orientation == Orientation.across)
            {
                questionItemText.transform.SetParent(acrossListTransform, false);
            }
            else
            { 
                questionItemText.transform.SetParent(downListTransform, false);
            }
        }

    }
}

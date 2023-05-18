using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossWordClueUIHandler : MonoBehaviour
{
    public static CrossWordClueUIHandler instance;
    public CrossWordClue activeClue;

    [Header("UI Ticker")]
    [SerializeField] TickerUI tickerUI;

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
}

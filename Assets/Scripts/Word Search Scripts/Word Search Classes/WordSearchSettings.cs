using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WordSearchSettings
{
    public TriviaQuestion[] triviaQuestions;
    public int timeLimit;
   public bool allowBackwards;
   public bool allowDiagonals;
   public bool enableHints;
   public bool randomizeQuestion;
   public int maxNumOfChoices;
   public string basedGrading;


}
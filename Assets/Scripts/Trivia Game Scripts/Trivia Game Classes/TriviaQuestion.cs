using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TriviaQuestion
{
  [TextArea(3,5)] public string question;
  public string answer;
  public string difficulty;
  public string id;
}
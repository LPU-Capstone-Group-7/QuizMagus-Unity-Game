using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CrossWordObject
{
   public int x;
   public int y;
    public Grid<CrossWordObject> grid;

   public char letter;
   public char inputtedLetter;
   public Dictionary<Orientation, CrossWordClue> crossWordClues = new Dictionary<Orientation, CrossWordClue>();
   public bool isAnswered;
   public bool isHighlighted = false;
   public bool isSelected = false;

   public CrossWordObject(Grid<CrossWordObject> grid, int x, int y)
   {
        this.grid = grid;
        this.x = x;
        this.y = y;
   }

   public HashSet<int> getCrossWordCluesIndex()
   {
      HashSet<int> indexSet = new HashSet<int>();

      foreach ( KeyValuePair<Orientation, CrossWordClue> clue in crossWordClues)
      {
         indexSet.Add(clue.Value.itemNumber);
      }

      return indexSet;
   }

   public void AssignPlacedWord(char letter, CrossWordClue clue)
   {      
      this.letter = letter;
      crossWordClues.Add(clue.orientation, clue);
   }
}

[System.Serializable]
public struct CrossWordClue
{
   public int itemNumber;
   public Vector2Int startNode;
   public TriviaQuestion triviaQuestion;
   public Orientation orientation;

   public CrossWordClue(int itemNumber, Vector2Int startNode, TriviaQuestion triviaQuestion, Orientation orientation)
   {
      this.itemNumber = itemNumber;
      this.startNode = startNode;
      this.triviaQuestion = triviaQuestion;
      this.orientation = orientation;
   }
}

[System.Serializable]
public class CrossWordGridItem
{
   public int index;
   public TriviaQuestion triviaQuestion;
   public HashSet<CrossWordObject> itemNodes;
   public Orientation orientation;
   public bool isAnswered = false;
   public float timeTakenToAnswer;

   public CrossWordGridItem(int index, TriviaQuestion triviaQuestion, HashSet<CrossWordObject> itemNodes, Orientation orientation, bool isAnswered)
   {
      this.index = index;
      this.triviaQuestion = triviaQuestion;
      this.itemNodes = itemNodes;
      this.orientation = orientation;
      this.isAnswered = isAnswered;
   }
}

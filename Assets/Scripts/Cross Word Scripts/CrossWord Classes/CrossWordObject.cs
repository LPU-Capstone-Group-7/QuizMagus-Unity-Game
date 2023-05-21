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

   public void AssignPlacedWord(char letter, CrossWordClue clue)
   {      
      this.letter = letter;
      crossWordClues.Add(clue.orientation, clue);
   }
}

public class CrossWordGridItem
{
   public int index;
   public TriviaQuestion triviaQuestion;
   public HashSet<CrossWordObject> itemNodes;
   public Orientation orientation;
   public bool isAnswered = false;


   public CrossWordGridItem(int index, TriviaQuestion triviaQuestion, HashSet<CrossWordObject> itemNodes, Orientation orientation, bool isAnswered)
   {
      this.index = index;
      this.triviaQuestion = triviaQuestion;
      this.itemNodes = itemNodes;
      this.orientation = orientation;
      this.isAnswered = isAnswered;
   }
}

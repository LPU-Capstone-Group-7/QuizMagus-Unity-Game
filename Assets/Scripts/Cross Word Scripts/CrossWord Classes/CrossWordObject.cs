using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossWordObject
{
   public int x;
   public int y;
    public Grid<CrossWordObject> grid;

   public char letter;
   public HashSet<CrossWordClue> crossWordClues;
   public bool isAnswered;

   public CrossWordObject(Grid<CrossWordObject> grid, int x, int y)
   {
        this.grid = grid;
        this.x = x;
        this.y = y;
   }

   public void AssignPlacedWord(char letter, CrossWordClue clue)
   {
      this.letter = letter;
      crossWordClues.Add(clue);
   }
}

public struct CrossWordClue
{
   public TriviaQuestion triviaQuestion;
   public Orientation orientation;

   public CrossWordClue(TriviaQuestion triviaQuestion, Orientation orientation)
   {
      this.triviaQuestion = triviaQuestion;
      this.orientation = orientation;
   }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossWordObject
{
   public int x;
   public int y;
    public Grid<CrossWordObject> grid;

   public char letter;
   public bool isStartingLetter = false;
   public HashSet<CrossWordClue> crossWordClues = new HashSet<CrossWordClue>();
   public bool isAnswered;

   public CrossWordObject(Grid<CrossWordObject> grid, int x, int y)
   {
        this.grid = grid;
        this.x = x;
        this.y = y;
   }

   public void AssignPlacedWord(bool isStartingLetter, char letter, CrossWordClue clue)
   {
      if(!this.isStartingLetter) this.isStartingLetter = isStartingLetter;
      
      this.letter = letter;
      crossWordClues.Add(clue);
   }
}

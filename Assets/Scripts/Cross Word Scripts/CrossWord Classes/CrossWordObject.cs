using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossWordObject
{
   public int x;
   public int y;
    public Grid<CrossWordObject> grid;

   public char letter;
   public List<string> assignedWords = new List<string>();
   public HashSet<Orientation> orientations = new HashSet<Orientation>();
   public bool isAnswered;

   public CrossWordObject(Grid<CrossWordObject> grid, int x, int y)
   {
        this.grid = grid;
        this.x = x;
        this.y = y;
   }

   public void AssignPlacedWord(string word, char letter, Orientation orientation)
   {
      this.letter = letter;

      assignedWords.Add(word);
      orientations.Add(orientation);
   }
}

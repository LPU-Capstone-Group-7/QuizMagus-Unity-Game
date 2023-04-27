using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LetterGridObject
{
   public int x;
   public int y;
   public Grid<LetterGridObject> grid;

   public char letter;
   public List<string> assignedWords = new List<string>();

   public LetterGridObject(Grid<LetterGridObject> grid, int x, int y)
   {
        this.grid = grid;
        this.x = x;
        this.y = y;
   }

   public bool isOccupied()
   {
     return assignedWords.Count > 0;
   }

   public void setNodeWordLetter(string assignedWord, char letter)
   {
     this.letter = letter;
     if(assignedWord != null) assignedWords.Add(assignedWord);
   }

   public void ClearNodeWordLetter()
   {
     letter = '\0';
     assignedWords = new List<string>();

   }
}

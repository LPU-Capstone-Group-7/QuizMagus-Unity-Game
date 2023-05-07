using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
* THIS PART OF CODE GENERATES THE POSSIBLE LAYOUT OF THE CROSSWORD GAME IN THE DENSEST WAY POSSIBLE
* USED THIS CODE AS A REFERENCE FOR MAKING THIS https://colab.research.google.com/drive/1z9tQ61NFKufKMhALj-AQi9nINnqMwiZk#scrollTo=ZyoCZzgaxeiL
*/

public class CrossWordGeneration : MonoBehaviour
{
    //REPRESENTS THE BEST GRID GENERATION SO FAR
    Dictionary<(int, int), char> bestSolution = new Dictionary<(int, int), char>();
    int maxIntersections = 0;


    void getPossibleCrossWordLayout(List<string> words) //CHANGE TO RETURN CHAR[][] LATER
    {
        //CREATE EMPTY DICTIONARY TO REPRESENT GRID
        Dictionary<(int,int), char> gridDictionary = new Dictionary<(int, int), char>();

        //CREATE EMPTY DICTIONARY TO MAP EACH LETTER TO ITS LOCATION ON THE GRID
        Dictionary<char, HashSet<(int,int)>> letters = new Dictionary<char, HashSet<(int, int)>>();

        //SORT LIST FROM LONGEST WORD TO SHORTEST WORD
        words.Sort((s1, s2) => s1.Length.CompareTo(s2.Length));
        words.Reverse();

        //ADD FIRST LETTER IN THE DICTIONARY GRID
        string firstWord = words[0];

        for (int i = 0; i < firstWord.Length; i++)
        {
        //ADD EACH LETTER OF THE FIRST WORD INSIDE THE GRID
        char letter = firstWord[i];
        gridDictionary[(i, 0)] = letter;

        //IF LETTER IS NOT ALREADY INSIDE THE LETTER DICTIONARY, ADD IT AND CREATE EMPTY SETS OF LOCATION
        if(!letters.ContainsKey(letter))
        {
            letters[letter] = new HashSet<(int, int)>();
        }

        //ADD CURRENT LOCATION OF THE LETTER TO ITS SET OF LOCATIONS IN THE LETTER DICTIONARY
        letters[letter].Add((i,0));
        }

        //GRID BORDER
        gridDictionary[(-1,0)] = gridDictionary[(firstWord.Length + 1, 0)] = '#';

        PlaceAllWordInGrid(gridDictionary, letters, words.Skip(1).ToList(), 0);
    }

    //ATTEMPTS TO PLACE THE SELECTED WORD IN EVERY POSSIBLE LOCATION IN THE GRID WHILE MAKING SURE THAT THE WORD WOULD INTERSECT WITH OTHER WORDS
    private void PlaceAllWordInGrid(Dictionary<(int, int), char> gridDictionary, Dictionary<char, HashSet<(int, int)>> letters, List<string> words, int numOfIntersections)
    {
        PlacedWord placedWord = new PlacedWord(gridDictionary, letters, numOfIntersections);
        if(words.Count > 0)
        {
            string word = words[0];

            foreach(PlacedWord newPlacedWord in PlaceOneWord(placedWord, word))
            {
                PlaceAllWordInGrid(gridDictionary, letters, words.Skip(1).ToList(), numOfIntersections); //RECURSION CHUCHU

                if(numOfIntersections > maxIntersections)
                {
                    maxIntersections = numOfIntersections;
                    bestSolution = gridDictionary;

                }
            }
        }
    }

    //TRIES TO PLACE A WORD SOMEWHERE IN THE GRID DICTIONARY
    private List<PlacedWord> PlaceOneWord(PlacedWord placedWord, string word)
    {
        List<PlacedWord> solutions = new List<PlacedWord>();

        for (int i = 0; i < word.Length; i++)
        {
            if(placedWord.letters.ContainsKey(word[i]))
            {
                foreach ((int x, int y) in placedWord.letters[word[i]])
                {
                    //TRY TO PLACE WORD HORIZONTALLY AND VERTICALLY
                    foreach (bool canPlaceHorizontally in new bool[] {true, false})
                    {
                        //CREATE NEW LETTER AND GRID DICTIONARY AND ALSO NUMBER OF INTERSECTIONS COUNTER
                        Dictionary<(int, int), char> newGridDictionary = new Dictionary<(int, int), char>();
                        Dictionary<char, HashSet<(int, int)>> newLetters = new Dictionary<char, HashSet<(int, int)>>();
                        int[] newIntersections = new int[] { placedWord.numOfIntersections };

                        //CHECK IF THE WORD CAN BE PLACED INSIDE THE GRID
                        if(canPlaceWordInGrid(i, word, placedWord, newGridDictionary, newLetters, (x, y), canPlaceHorizontally, ref newIntersections))
                        {
                            //MERGE THE OLD DICTIONARIES AND THE NEW ONES
                            newGridDictionary = newGridDictionary.Union(placedWord.gridDictionary).ToDictionary(x => x.Key, x => x.Value);
                            newLetters = newLetters.Union(placedWord.letters).ToDictionary(x => x.Key, x => x.Value);

                            //FILL OUT ADJACENT CELLS WITH # TO REPRESENT AS UNFILLABLE CELLS
                            for (int j = 0; j < i; j++)
                            {
                                FillCross(newGridDictionary, Add(x, y, j - i, canPlaceHorizontally));
                            }

                            for (int j = 0; j < word.Length - i; j++)
                            {
                                FillCross(newGridDictionary, Add(x, y, j, canPlaceHorizontally));   
                            }

                            PlacedWord newPlacedWord = new PlacedWord(newGridDictionary, newLetters, newIntersections[0]);
                            solutions.Add(newPlacedWord);

                        }
                    }
                }
            }
        }

        return solutions;
    }

    private void FillCross(Dictionary<(int, int), char> newGridDictionary, (int x, int y) gridPos)
    {
        //CHECK THE ADJACENT CELLS OF THE CURRENT POSITION
        if(newGridDictionary.ContainsKey((gridPos.x + 1, gridPos.y)) && newGridDictionary[(gridPos.x + 1, gridPos.y)] != '#')
        {
            //IF THERE ARE CELLSS TO THE RIGHT, CHECK CELLS ABOVE AND BELOW
            if(newGridDictionary.ContainsKey((gridPos.x, gridPos.y + 1)) && newGridDictionary[(gridPos.x, gridPos.y + 1)] != '#')
            {
                newGridDictionary[(gridPos.x + 1, gridPos.y + 1)] = '#'; //MARK THE TOP RIGHT CELL AS NOT FILLABLE
            }

            if (newGridDictionary.ContainsKey((gridPos.x, gridPos.y - 1)) && newGridDictionary[(gridPos.x, gridPos.y - 1)] != '#')
            {
                newGridDictionary[(gridPos.x + 1, gridPos.y - 1)] = '#'; //MARK THE BOTTOM RIGHT CELL AS NOT FILLABLE
            }
        }

        if (newGridDictionary.ContainsKey((gridPos.x - 1, gridPos.y)) && newGridDictionary[(gridPos.x - 1, gridPos.y)] != '#')
        {
            //IF THERE ARE CELLS TO THE LEFT, CHECK CELLS ABOVE AND BELOW
            if (newGridDictionary.ContainsKey((gridPos.x, gridPos.y + 1)) && newGridDictionary[(gridPos.x, gridPos.y + 1)] != '#')
            {
                newGridDictionary[(gridPos.x - 1, gridPos.y + 1)] = '#'; //MARK THE TOP LEFT CELL AS NOT FILLABLE
            }
            if (newGridDictionary.ContainsKey((gridPos.x, gridPos.y - 1)) && newGridDictionary[(gridPos.x, gridPos.y - 1)] != '#')
            {
                newGridDictionary[(gridPos.x - 1, gridPos.y - 1)] = '#'; //MARK THE BOTTOM LEFT CELL AS NOT FILLABLE
            }
        }
    }

    private bool canPlaceWordInGrid(int letterIndex, string word, PlacedWord placedWord, Dictionary<(int, int), char> newGridDictionary, Dictionary<char, HashSet<(int, int)>> newLetters, (int x, int y) gridPos, bool canPlaceHorizontally, ref int[] newIntersections)
    {
        for (int i = 0; i < letterIndex; i++)
        {
            (int, int) newGridPos = Add(gridPos.x, gridPos.y, i - letterIndex, canPlaceHorizontally);
            newGridDictionary[newGridPos] = word[i];

            //CHECK IF POSITION ALREADY CONTAINS A LETTER
            if(placedWord.gridDictionary.ContainsKey(newGridPos))
            {
                //IF THERE IS, CHECK IF THE LETTERS ARE THE SAME
                if(placedWord.gridDictionary[newGridPos] != word[i]) return false;

                //IF ITS THE SAME, INCREASE THE COUNT OF INTERSECTIONS
                newIntersections[0]++;
            }
            
            //ADD POSITION TO THE NEW LETTER DICTIONARY
            if(!newLetters.ContainsKey(word[i]))
            {
                newLetters[word[i]] = new HashSet<(int, int)>();
            }

            newLetters[word[i]].Add(newGridPos);
        }

        //LOOP OVER THE LETTERS AFTER THE CURRENT LETTER
        for (int i = 0; i < word.Length - letterIndex; i++)
        {
            (int, int) newGridPos = Add(gridPos.x, gridPos.y, i, canPlaceHorizontally);
            newGridDictionary[newGridPos] = word[i + letterIndex];

            if(placedWord.gridDictionary.ContainsKey(newGridPos))
            {
                if(placedWord.gridDictionary[newGridPos] != word[i + letterIndex]) return false;

                newIntersections[0]++;
            }

            if(!newLetters.ContainsKey(word[i + letterIndex]))
            {
                newLetters[word[i + letterIndex]] = new HashSet<(int, int)>();
            }

            newLetters[word[i + letterIndex]].Add(newGridPos);
        }

        (int, int) upperCornerPosition = Add(gridPos.x, gridPos.y,  -letterIndex - 1, canPlaceHorizontally);
        (int, int) lowerCornerPosition = Add(gridPos.x, gridPos.y, word.Length - letterIndex, canPlaceHorizontally);

        if(placedWord.gridDictionary.ContainsKey(upperCornerPosition) && placedWord.gridDictionary[upperCornerPosition] != '#') return false;

        newGridDictionary[upperCornerPosition] = '#';

        if(placedWord.gridDictionary.ContainsKey(lowerCornerPosition) && placedWord.gridDictionary[lowerCornerPosition] != '#') return false;

        newGridDictionary[lowerCornerPosition] = '#';

        return true;
    }

    private (int, int) Add(int x, int y, int c, bool canPlaceHorizontally)
    {
      if(canPlaceHorizontally)
      {
        return (x + c, y);
      }
      else
      {
        return(x, y + c);
      }
    }
} 

/*
* DATA STRUCTS FOR BETTER READABILITY IN THE CROSSWORD GENERATION CODES
*/

public struct PlacedWord
{
  public Dictionary<(int, int), char> gridDictionary { get; set; }
  public Dictionary<char, HashSet<(int, int)>> letters { get; set; }
  public int numOfIntersections { get; set; }

  public PlacedWord(Dictionary<(int, int), char> gridDictionary, Dictionary<char, HashSet<(int, int)>> letters, int numOfIntersections)
  {
    this.gridDictionary = gridDictionary;
    this.letters = letters;
    this.numOfIntersections = numOfIntersections;
  }
}
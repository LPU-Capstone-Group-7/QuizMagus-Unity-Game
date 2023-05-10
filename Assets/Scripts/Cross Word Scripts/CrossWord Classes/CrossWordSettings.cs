using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CrossWordSettings
{
    public TriviaQuestion[] triviaQuestions;
    public int timeLimit;
    public bool enableHints;
    public bool randomizeQuestion;
    public string basedGrading;
    public bool showCorrectAnswer;
    public int numOfLettersToShow;
}

public struct WordPlacement
{
    public int startRow;
    public int startCol;
    public bool canPlaceWordHorizontally;

    public WordPlacement(int startRow, int startCol, bool canPlaceWordHorizontally)
    {
        this.startRow = startRow;
        this.startCol = startCol;
        this.canPlaceWordHorizontally = canPlaceWordHorizontally;
    }
}

public enum Orientation
{
    across,
    down
}

public struct CrossWordLayout
{
    public char[,] board;
    public HashSet<CrossWordEntry> crossWordEntries;

    public CrossWordLayout(char[,] board, HashSet<CrossWordEntry> crossWordEntries)
    {
        this.board = board;
        this.crossWordEntries = crossWordEntries;
    }

    public HashSet<CrossWordEntry> FindMatchingEntries(int row, int col, char letter)
    {
        HashSet<CrossWordEntry> matchingEntries = new HashSet<CrossWordEntry>();

        foreach (CrossWordEntry entry in crossWordEntries)
        {
            if(entry.letterPlacements.Contains((row, col, letter)))
            {
                matchingEntries.Add(entry);
            }
        }

        return matchingEntries;
    }

    public float GetAspectDiff()
    {
        float aspectRatio = board.GetLength(1) / board.GetLength(0);
        return Mathf.Abs(aspectRatio - 1.0f);
    }
    public int GetTotalLettersInBoard()
    {
        int ctr = 0;

        for (int row = 0; row < board.GetLength(0); row++)
        {
            for (int col = 0; col < board.GetLength(1); col++)
            {
                if(board[row, col] != '\0') ctr++;
            }
        }

        return ctr; 
    }
}

public struct CrossWordEntry
{
    public string word;
    public Orientation orientation;
    public HashSet<(int row, int col, char letter)> letterPlacements;

    public CrossWordEntry(string word, Orientation orientation, HashSet<(int row, int col, char letter)> letterPlacements)
    {
        this.word = word;
        this.orientation = orientation;
        this.letterPlacements = letterPlacements;
    }
}
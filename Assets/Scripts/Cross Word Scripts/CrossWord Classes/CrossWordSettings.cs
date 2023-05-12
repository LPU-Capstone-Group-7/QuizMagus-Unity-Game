using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CrossWordSettings
{
    public TriviaQuestion[] triviaQuestions;
    public int timeLimit;
    public bool enableHints;
    public string basedGrading;
    public bool showCorrectAnswer;
    public int numOfLettersToShow;
}

public struct WordPlacement
{
    public int startRow;
    public int startCol;
    public Orientation orientation;

    public WordPlacement(int startRow, int startCol, Orientation orientation)
    {
        this.startRow = startRow;
        this.startCol = startCol;
        this.orientation = orientation;
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
    public WordPlacement wordPlacement;
    public HashSet<(int row, int col, char letter)> letterPlacements;

    public CrossWordEntry(string word, WordPlacement wordPlacement, HashSet<(int row, int col, char letter)> letterPlacements)
    {
        this.word = word;
        this.wordPlacement = wordPlacement;
        this.letterPlacements = letterPlacements;
    }
}

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
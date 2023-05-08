using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CrossWordGeneration : MonoBehaviour
{
    private List<string> testWords =  new List<string>{"abracadabra", "abrakadabra", "abrawadabra", "abraxadabra", "abrayadabra", "abrazadabra"};
    //private List<string> testWords =  new List<string>{"Elephants","Kangaroos","Crocodiles","Chimpanzees","Flamingos","Rhinoceroses","Gorillas","Cheetahs","Hippopotamuses","Toucans", "Dog", "Cat", "Bat"};
    private CrossWordLayout bestCrossWordLayout;

    private void Start()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        GenerateCrossWordLayout(testWords);

        stopwatch.Stop();
        Debug.Log ("Time taken: "+(stopwatch.Elapsed));
    }
    void GenerateCrossWordLayout(List<string> words)
    {
        for (int i = 0; i < words.Count; i++)
        {
            words[i] = words[i].ToLower();
        }

        //SORT WORDS BY LENGTH AND CHANGED THEM ALL TO LOWERCASE
        Queue<string> sortedWords = new Queue<string>(words.OrderByDescending(w => w.Length));

        //FIGURE OUT THE SIZE OF THE BOARD USING THE LENGTH OF THE LONGEST WORD INSIDE THE QUEUE, WHICH IS THE FIRST INDEXED WORD
        string firstWord = sortedWords.Peek();

        int boardSize = firstWord.Length + (firstWord.Length / 2);
        char[,] initialBoard = new char[boardSize, boardSize];

        //PLACE THE FIRST WORD HORIZONTALLY IN THE MIDDLE OF THE BOARD
        int middleRow = boardSize / 2;
        int startCol = (boardSize - firstWord.Length) / 2;

        for (int i = 0; i < firstWord.Length; i++)
        {
            initialBoard[middleRow, startCol + i] = firstWord[i];
        }

        //INITIALIZE THE BEST CROSSWORD LAYOUT
        bestCrossWordLayout = new CrossWordLayout(initialBoard, 1);
        DisplayBestLayout(bestCrossWordLayout);

        //TRY TO PLACE OTHER WORDS INSIDE THE BOARD
        sortedWords.Dequeue();
        PlaceAllWords(sortedWords, initialBoard, 1);
    }

    void PlaceAllWords(Queue<string> words, char[,] board, int wordCount)
    {
        //LOOP BREAKER
       if(words.Count <= 0) return;

        //TAKE THE NEXT WORD, SEARCH THROUGH ALL THE LETTERS ALREADY IN THE BOARD, AND SEE IF THERE ARE ANY POSSIBLE INTERSECTIONS
        //GET ALL POSSIBLE INTERSECTIONS INSIDE THE LIST
        string word = words.Dequeue();
        List<WordPlacement> intersections = FindIntersections(board, word);

        //CREATE NEW UPDATED BOARD THAT WILL BE RETURNED
        char[,] updatedBoard = null;

        if(intersections.Count > 0)
        {
            foreach (WordPlacement wordPlacement in intersections)
            {
                //CREATE NEW UPDATED BOARD WITH THIS WORD PLACEMENT
                if(wordPlacement.canPlaceWordHorizontally)
                {
                    updatedBoard = PlaceWordHorizontally(board, wordPlacement, word);
                }
                else
                {
                    updatedBoard = PlaceWordVertically(board, wordPlacement, word);
                }

                //INITIALIZE NEW CROSSWORD LAYOUT
                CrossWordLayout crossWordLayout = new CrossWordLayout(updatedBoard, wordCount + 1);

                //COMPARE IT TO THE CURRENT BEST CROSSWORD LAYOUT
                if(crossWordLayout.numOfPlacedWords > bestCrossWordLayout.numOfPlacedWords)
                {
                    bestCrossWordLayout = crossWordLayout;
                    DisplayBestLayout(bestCrossWordLayout);
                }
                else if(crossWordLayout.numOfPlacedWords == bestCrossWordLayout.numOfPlacedWords && crossWordLayout.GetTotalLettersInBoard() < bestCrossWordLayout.GetTotalLettersInBoard())
                {
                    bestCrossWordLayout = crossWordLayout;
                    DisplayBestLayout(bestCrossWordLayout);
                }

                //PLACE NEXT WORD IN GRID
                PlaceAllWords(new Queue<string>(words), updatedBoard, wordCount + 1);
            }
        }
    }

    private List<WordPlacement> FindIntersections(char[,] board, string word)
    {
        //INITITALIZE LIST
        List<WordPlacement> intersectionList = new List<WordPlacement>();
        
        for (int row = 0; row < board.GetLength(0); row++)
        {
            for (int col = 0; col < board.GetLength(1); col++)
            {
                char boardLetter = board[row,col];

                //SEE IF THERE IS A LETTER IN THIS CELL AND WORD HAS SIMILARITIES WITH IT
                for (int i = 0; i < word.Length; i++)
                {
                    if(boardLetter == word[i])
                    {
                        if(CanPlaceWordHorizontally(board, word, i, row, col))
                        {
                            int startCol = col - i;
                            WordPlacement wordPlacement = new WordPlacement(row, startCol, true);
                            intersectionList.Add(wordPlacement);
                        }

                        if(CanPlaceWordVertically(board, word, i, row, col))
                        {
                            int startRow = row - i;
                            WordPlacement wordPlacement = new WordPlacement(startRow, col, false);
                            intersectionList.Add(wordPlacement);
                        }
                    }
                }
            }
        }

        return intersectionList;
    }

    private bool CanPlaceWordHorizontally(char[,] board, string word, int letterIndex, int row, int col)
    {
       //DETERMINE THE STARTING CELL OF THE WORD USING THE LETTER INDEX
       int startingCol = col - letterIndex;

       //SEE IF STARTING CELL WILL NOT GO OUT OF BOUNDS THE WIDTH OF THE BOARD
        if (startingCol < 0 || startingCol + word.Length > board.GetLength(1)) return false;

       //CHECK LEFT OF STARTING LETTER IF THERE IS AN EXISTING LETTER (THERE SHOULD BE NONE)
       if (startingCol > 0 && board[row, startingCol - 1] != '\0') return false;

       //CHECK RIGHT OF ENDING LETTER IF THERE IS AN EXISTING LETTER (THERE SHOULD BE NONE)
       if (startingCol + word.Length < board.GetLength(1) && board[row, startingCol + word.Length] != '\0') return false;

       //FOREACH LETTER CELL THAT IS EMPTY, CHECK IF THERE ARE EXISTING LETTER ABOVE AND BELOW IT (THERE SHOULD BE NONE)
       for (int i = 0; i < word.Length; i++)
       {
            char letter = word[i];
            int cellRow = row;
            int cellCol = startingCol + i;

            //SKIP ALREADY PLACED LETTERS
            if(board[cellRow, cellCol] != '\0')
            {
                if(board[cellRow, cellCol] != letter) return false;

                continue;
            }

            //CHECK BELOW CELL
            if(cellRow > 0 && board[cellRow - 1, cellCol] != '\0') return false;

            //CHECK ABOVE CELL
            if(cellRow < board.GetLength(0) - 1 && board[cellRow + 1, cellCol] != '\0' ) return false;
       }

       return true;
    }

    private bool CanPlaceWordVertically(char[,] board, string word, int letterIndex, int row, int col)
    {
        //DETERMINE THE START Y OF THE LETTER
        int startingRow = row - letterIndex;

        //CHECK IF THIS STARTING POSITION WOULD CAUSE AN OUT OF BOUNDS
        if(startingRow < 0 || startingRow + word.Length > board.GetLength(0)) return false;

        //CHECK IF THERE IS AN EXISTING LETTER ABOVE THE STARTING LETTER (THERE SHOULD BE NONE)
        if(startingRow > 0 && board[startingRow - 1, col] != '\0') return false;

        //CHECK IF THERE IS AN EXISTING LETTER BELOW THE ENDING LETTER (THERE SHOULD BE NONE)
        if(startingRow + word.Length < board.GetLength(0) && board[startingRow + word.Length, col] != '\0') return false;

        //FOR EACH LETTER CELL, CHECK THE LEFT AND RIGHT OF EACH LETTER CELL IF THERE IS AN EXISTING LETTER (THERE SHOULD BE NONE)
        for (int i = 0; i < word.Length; i++)
        {
            char letter = word[i];
            int cellRow = startingRow + i;
            int cellCol = col;

            //SKIPPED ALREADY PLACED LETTERS
            if(board[cellRow, cellCol] != '\0')
            {
                if(board[cellRow, cellCol] != letter) return false;

                continue;
            }

            //CHECK LEFT SIDE 
            if (cellCol > 0 && board[cellRow, cellCol - 1] != '\0')
            {
                return false;
            }

            //CHECK RIGHT SIDE
            if (cellCol < board.GetLength(1) - 1 && board[cellRow, cellCol + 1] != '\0')
            {
                return false;
            }
        }
        
        return true;
    }

    private char[,] PlaceWordHorizontally(char[,] board, WordPlacement wordPlacement, string word)
    {
        for (int i = 0; i < word.Length; i++)
        {
            board[wordPlacement.startRow, wordPlacement.startCol + i] = word[i];
        }

        return board;
    }

    private char[,] PlaceWordVertically(char[,] board, WordPlacement wordPlacement, string word)
    {
        for (int i = 0; i < word.Length; i++)
        {
            board[wordPlacement.startRow + i, wordPlacement.startCol] = word[i];
        }

        return board;
    }

    private void DisplayBestLayout(CrossWordLayout crossWordLayout)
    {
        //Debug.Log("PLACED WORDS: " + crossWordLayout.numOfPlacedWords + " ,NUMBER OF LETTERS: " + crossWordLayout.GetTotalLettersInBoard());

        string gridString = "";
        for (int row = 0; row < crossWordLayout.board.GetLength(0); row++)
        {
            for (int col = 0; col < crossWordLayout.board.GetLength(1); col++)
            {
                if(crossWordLayout.board[row,col] != '\0')
                {
                    gridString += crossWordLayout.board[row,col];
                }
                else
                {
                    gridString += " ";
                }
            }

            gridString += "\n";
        }

        Debug.Log(gridString);
    }


}

struct WordPlacement
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

struct CrossWordLayout
{
    public char[,] board;
    public int numOfPlacedWords;

    public CrossWordLayout(char[,] board, int numOfPlacedWords)
    {
        this.board = board;
        this.numOfPlacedWords = numOfPlacedWords;
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

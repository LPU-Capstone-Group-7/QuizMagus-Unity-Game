using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
* CHANGE LIST SORTING APPROACH TO PRIORITIZE WORDS WITH MORE SIMILAR LETTERS IF LENGTHS ARE  THE SAME -- DONE
* CHANGE CHAR[,] ARRAY  TO A STRUCT CROSSWORD LETTER CONTAINING {WORD[], LETTER, ORIENTATIONS[]} -- DONE
* TRIM UNUSED CORNER ARRAYS -- DONE
* CREATE A WAY TO MOVE  THIS CODE INTO A GENERIC OBJECT GRID -- DONE
*/

public class CrossWordGeneration : MonoBehaviour
{
    public static CrossWordGeneration instance;
    List<string> sortedWordList = new List<string>();
    int numberOfWords = 0;
    private CrossWordLayout bestCrossWordLayout;

    private void Awake()
    {
        if(instance == null) instance = this;    
    }
    public CrossWordLayout GenerateCrossWordLayout(List<string> words)
    {
        for (int i = 0; i < words.Count; i++)
        {
            words[i] = words[i].ToLower();
        }

        //SORT WORDS BY LENGTH AND CHANGED THEM ALL TO LOWERCASE
        sortedWordList = SortWordList(words);
        Queue<string> sortedWords = new Queue<string>(sortedWordList);
        numberOfWords = words.Count;

        //FIGURE OUT THE SIZE OF THE BOARD USING THE LENGTH OF THE LONGEST WORD INSIDE THE QUEUE, WHICH IS THE FIRST INDEXED WORD
        string firstWord = sortedWords.Peek();

        int boardSize = firstWord.Length > words.Count? firstWord.Length * 2 : words.Count * 2;
        char[,] initialBoard = new char[boardSize, boardSize];

        //PLACE THE FIRST WORD HORIZONTALLY IN THE MIDDLE OF THE BOARD
        int startCol = boardSize/2;
        int startRow = (boardSize/2) - Mathf.CeilToInt((float)firstWord.Length /2);
        WordPlacement firstWordPlacement = new WordPlacement(startRow, startCol, Orientation.across); 

        HashSet<(int row, int col, char letter)> initialLetterPlacements = new HashSet<(int row, int col, char letter)>();
        PlaceWordHorizontally(initialBoard, firstWordPlacement, firstWord, out CrossWordEntry crossWordEntry);

        //INITIALIZE THE BEST CROSSWORD LAYOUT
        HashSet<CrossWordEntry> initialCrossWordEntries = new HashSet<CrossWordEntry>{crossWordEntry};
        (char[,] trimmedBoard, HashSet<CrossWordEntry> updatedEntries) = TrimCrossWordBoard(initialBoard, initialCrossWordEntries);
        bestCrossWordLayout = new CrossWordLayout(trimmedBoard, updatedEntries);
        DisplayBestLayout(bestCrossWordLayout, firstWord);

        //TRY TO PLACE OTHER WORDS INSIDE THE BOARD
        sortedWords.Dequeue();
        PlaceAllWords(sortedWords, initialBoard, initialCrossWordEntries);

        return bestCrossWordLayout;
    }

    void PlaceAllWords(Queue<string> words, char[,] board, HashSet<CrossWordEntry> crossWordEntries)
    {
        //LOOP BREAKER
       if(words.Count <= 0) return;

        //TAKE THE NEXT WORD, SEARCH THROUGH ALL THE LETTERS ALREADY IN THE BOARD, AND SEE IF THERE ARE ANY POSSIBLE INTERSECTIONS
        //GET ALL POSSIBLE INTERSECTIONS INSIDE THE LIST
        string word = words.Dequeue();
        List<WordPlacement> intersections = FindIntersections(board, word);

        if(intersections.Count > 0)
        {
            foreach (WordPlacement wordPlacement in intersections)
            {
                //CREATE NEW UPDATED BOARD WITH THIS WORD PLACEMENT
                CrossWordEntry crossWordEntry;
                HashSet<CrossWordEntry> updatedCrossWordEntries = new HashSet<CrossWordEntry>(crossWordEntries);
                char[,] updatedBoard = wordPlacement.orientation == Orientation.across? PlaceWordHorizontally(board, wordPlacement, word, out crossWordEntry) : PlaceWordVertically(board, wordPlacement, word, out crossWordEntry);

                //INITIALIZE NEW CROSSWORD LAYOUT
                updatedCrossWordEntries.Add(crossWordEntry);
                (char[,] trimmedBoard, HashSet<CrossWordEntry> updatedEntries) = TrimCrossWordBoard(updatedBoard, updatedCrossWordEntries);
                CrossWordLayout crossWordLayout = new CrossWordLayout(trimmedBoard, updatedEntries);

                //COMPARE IT TO THE CURRENT BEST CROSSWORD LAYOUT
                if(CompareToBestLayout(crossWordLayout))
                {
                    bestCrossWordLayout = crossWordLayout;
                    DisplayBestLayout(bestCrossWordLayout,word);
                }

                //PLACE NEXT WORD IN GRID
                PlaceAllWords(words, updatedBoard, updatedCrossWordEntries);
            }
        }
        else{Debug.Log("Skipped: " + word);}
    }

    private List<WordPlacement> FindIntersections(char[,] board, string word)
    {
        //INITITALIZE LIST
        List<WordPlacement> intersectionList = new List<WordPlacement>();

        //Create HASHSET OF LETTERS IN THE WORD
        HashSet<char> wordLetters = new HashSet<char>(word);
        
        for (int row = 0; row < board.GetLength(0); row++)
        {
            for (int col = 0; col < board.GetLength(1); col++)
            {
                char boardLetter = board[row,col];

                if(wordLetters.Contains(boardLetter))
                {
                    //SEE IF THERE IS A LETTER IN THIS CELL AND WORD HAS SIMILARITIES WITH IT
                    for (int i = 0; i < word.Length; i++)
                    {
                        if(boardLetter == word[i])
                        {
                            if(CanPlaceWordHorizontally(board, word, i, row, col))
                            {
                                int startCol = col - i;
                                WordPlacement wordPlacement = new WordPlacement(row, startCol, Orientation.across);
                                intersectionList.Add(wordPlacement);
                            }

                            if(CanPlaceWordVertically(board, word, i, row, col))
                            {
                                int startRow = row - i;
                                WordPlacement wordPlacement = new WordPlacement(startRow, col, Orientation.down);
                                intersectionList.Add(wordPlacement);
                            }
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

       //CHECK IF THERE IS ATLEAST ONE EMPTY CELL TO THE CELLS THAT IT WOULD OCCUPY
       bool hasOneEmptyCell = false;
       for (int i = 0; i < word.Length; i++)
       {
            char letter = word[i];
            int cellRow = row;
            int cellCol = startingCol + i;

            if(board[cellRow, cellCol] == '\0') hasOneEmptyCell = true;
       }

       if(!hasOneEmptyCell) return false;

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
            if (cellCol > 0 && board[cellRow, cellCol - 1] != '\0') return false;

            //CHECK RIGHT SIDE
            if (cellCol < board.GetLength(1) - 1 && board[cellRow, cellCol + 1] != '\0') return false;
        }

        //CHECK IF THERE IS ATLEAST ONE EMPTY CELL TO THE CELLS THAT IT WOULD OCCUPY
       bool hasOneEmptyCell = false;
       for (int i = 0; i < word.Length; i++)
       {
            char letter = word[i];
            int cellRow = startingRow + i;
            int cellCol = col;

            if(board[cellRow, cellCol] == '\0') hasOneEmptyCell = true;
       }

       if(!hasOneEmptyCell) return false;
        
        return true;
    }

    private char[,] PlaceWordHorizontally(char[,] board, WordPlacement wordPlacement, string word, out CrossWordEntry crossWordEntry)
    {
        HashSet<(int row, int col, char letter)> letterPlacements = new HashSet<(int row, int col, char letter)>();

        for (int i = 0; i < word.Length; i++)
        {
            board[wordPlacement.startRow, wordPlacement.startCol + i] = word[i];
            letterPlacements.Add((wordPlacement.startRow, wordPlacement.startCol + i, word[i]));
        }

        crossWordEntry = new CrossWordEntry(word, wordPlacement, letterPlacements);

        return board;
    }

    private char[,] PlaceWordVertically(char[,] board, WordPlacement wordPlacement, string word, out CrossWordEntry crossWordEntry)
    {
        HashSet<(int row, int col, char letter)> letterPlacements = new HashSet<(int row, int col, char letter)>();

        for (int i = 0; i < word.Length; i++)
        {
            board[wordPlacement.startRow + i, wordPlacement.startCol] = word[i];
            letterPlacements.Add((wordPlacement.startRow + i, wordPlacement.startCol, word[i]));

        }

        crossWordEntry = new CrossWordEntry(word, wordPlacement, letterPlacements);

        return board;
    }

    private (char[,] trimmedBoard, HashSet<CrossWordEntry> updatedCrossWordEntries) TrimCrossWordBoard(char[,] board, HashSet<CrossWordEntry> crossWordEntries)
    {
        int boardRow = board.GetLength(0);
        int boardCol = board.GetLength(1);

        //FIND THE MIN AND MAX ROW AND COLUMNS INDICES THAT HAVE A LETER
        int minRow = boardRow - 1, maxRow = 0;
        int minCol = boardCol -1, maxCol = 0;

        for (int row = 0; row < boardRow; row++)
        {
            for (int col = 0; col < boardCol; col++)
            {
                if(board[row, col] != '\0')
                {
                    minRow = Mathf.Min(minRow, row);
                    maxRow = Mathf.Max(maxRow, row);
                    minCol = Mathf.Min(minCol, col);
                    maxCol = Mathf.Max(maxCol, col);
                }
            }
        }
        
        //CREATE NEW ARRAY WITH THE TRIMMED DIMENSIONS
        int trimmedRows = maxRow - minRow + 1;
        int trimmedCols = maxCol - minCol + 1;
        char[,] trimmedBoard = new char[trimmedRows, trimmedCols];

        //COPY CONTENTS FROM THE ORIGINAL BOARD
        for (int row = 0; row < trimmedRows; row++)
        {
            for (int col = 0; col < trimmedCols; col++)
            {
                trimmedBoard[row, col] = board[row + minRow, col + minCol];
            }
        }


        //UPDATE CROSSWORD ENTRIES BASED ON THE NEWLY TRIMED BOARD
        HashSet<CrossWordEntry> updatedEntries = new HashSet<CrossWordEntry>();

        foreach (CrossWordEntry entry in crossWordEntries)
        {
            HashSet<(int row, int col, char letter)> updatedLetterPlacements = new HashSet<(int row, int col, char letter)>();

            //UPDATE WORD PLACEMENT BASED ON THE NEWLY TRIMMED BOARD
            int updatedStartRow = entry.wordPlacement.startRow - minRow;
            int updatedStartCol = entry.wordPlacement.startCol - minCol;

            WordPlacement updatedWordPlacement = new WordPlacement(updatedStartRow, updatedStartCol, entry.wordPlacement.orientation);
            
            foreach ((int row, int col, char letter) placement in entry.letterPlacements)
            {
                int updatedRow = placement.row - minRow;
                int updatedCol = placement.col - minCol;
                
                if (updatedRow >= 0 && updatedRow < trimmedRows && updatedCol >= 0 && updatedCol < trimmedCols)
                {
                    updatedLetterPlacements.Add((updatedRow, updatedCol, placement.letter));
                }
            }
            if (updatedLetterPlacements.Count > 0)
            {
                CrossWordEntry updatedEntry = new CrossWordEntry(entry.word, updatedWordPlacement, updatedLetterPlacements);
                updatedEntries.Add(updatedEntry);
            }
        }

        return (trimmedBoard, updatedEntries);

    }
    
    private void DisplayBestLayout(CrossWordLayout crossWordLayout, string newWord)
    {
        Debug.Log("NEW WORD: " + newWord+ " ,PLACED WORDS: " + crossWordLayout.crossWordEntries.Count + "/" + numberOfWords + " ,NUMBER OF LETTERS: " + crossWordLayout.GetTotalLettersInBoard() + " ,SIZE: [" + crossWordLayout.board.GetLength(0) + "," + crossWordLayout.board.GetLength(1) + "]" );
    }

    //SORTING FUNCTION ALGORITHM
    private List<string> SortWordList(List<string> words)
    {
        words.Sort((a, b) => 
        {
            int commonLettersA = CountCommonLetters(a, words);
            int commonLettersB = CountCommonLetters(b, words);
                
            if(commonLettersA == commonLettersB) return 0;

            return commonLettersB - commonLettersA;
        });

        return words;
    }

    private int CountCommonLetters(string selectedWord, List<string> words)
    {
        int count = 0;

        foreach (string word in words)
        {
            if(word != selectedWord)
            {
                foreach (char letter in word)
                {
                    if(selectedWord.Contains(letter)) count++;
                }
            }
        }

        return count;
    }

    //COMPARE BEST CROSSWORD LAYOUT
    private bool CompareToBestLayout(CrossWordLayout crossWordLayout)
    {
        if(crossWordLayout.crossWordEntries.Count > bestCrossWordLayout.crossWordEntries.Count) return true;

        if(crossWordLayout.crossWordEntries.Count == bestCrossWordLayout.crossWordEntries.Count)
        {
            if(crossWordLayout.GetTotalLettersInBoard() > bestCrossWordLayout.GetTotalLettersInBoard()) return false;

            //if(crossWordLayout.GetAspectDiff() < bestCrossWordLayout.GetAspectDiff()) return true;

            if(crossWordLayout.board.GetLength(1) > bestCrossWordLayout.board.GetLength(1)) return true;
        }

        return false;
    }
}
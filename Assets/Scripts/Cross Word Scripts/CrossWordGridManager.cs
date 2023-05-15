using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
public class CrossWordGridManager : MonoBehaviour
{
  public static CrossWordGridManager instance;

  [Header("Grid Component")]
  [SerializeField] private Vector2Int maxGridSize;
  [SerializeField] private float cellSize = 1f;
  private List<string> testWords =  new List<string>{"Elephants","Kangaroos","Crocodiles","Chimpanzees","Flamingos","Rhinoceroses","Gorillas","Cheetahs","Hippopotamuses","Toucans", "Dog", "Cat", "Bat"};
  List<string> testWords50 = new List<string>
  {
    "lion",
    "tiger",
    "bear",
    "wolf",
    "leopard",
    "cheetah",
    "lynx",
    "jaguar",
    "panther",
    "cougar",
    "bobcat",
    "fox",
    "coyote",
    "hyena",
    "mongoose",
    "badger",
    "raccoon",
    "opossum",
    "weasel",
    "ferret",
    "skunk",
    "platypus",
    "otter",
    "seal",
    "walrus",
    "dolphin",
    "whale",
    "shark",
    "swordfish",
    "salmon",
    "trout",
    "bass",
    "crab",
    "lobster",
    "shrimp",
    "clam",
    "oyster",
    "octopus",
    "squid",
    "snail",
    "slug",
    "worm",
    "ant",
    "bee",
    "caterpillar",
    "mosquito",
    "spider",
    "scorpion",
    "snail",
  };

  List<string> testWords01 = new List<string>{"dog", "duck"};
  Grid<CrossWordObject> grid;

  [Header("CrossWord Components")]
  [SerializeField] Transform letterTransformParent;
  [SerializeField] Transform letterTransformPrefab;
  TriviaQuestion[] triviaQuestions;
  CrossWordLayout crossWordLayout;

  private void Awake()
  {
      if(instance == null) instance = this;
  }

  private void Start()
  {
    // System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    // stopwatch.Start();

    //CreateCrossWordGrid(testWords50);

    // stopwatch.Stop();
    // Debug.Log ("Time taken: "+(stopwatch.Elapsed));
  }

  public void CreateCrossWordGrid(TriviaQuestion[] triviaQuestions)
  {
    //GENERATE LIST OF WORDS USING THE TRIVIA QUESTIONS
    this.triviaQuestions = triviaQuestions;

    List<string> words = new List<string>();
    foreach (TriviaQuestion triviaQuestion in triviaQuestions)
    {
      string word = triviaQuestion.answer.ToLower().Replace(" ","");
      words.Add(word);
    }

    //GENERATE LAYOUT BASED ON LIST OF WORDS
    crossWordLayout = CrossWordGeneration.instance.GenerateCrossWordLayout(words);

    //GENERATE GRID USING THE SIZE OF THE CROSSWORD LAYOUT
    GenerateGrid(crossWordLayout.board.GetLength(1) , crossWordLayout.board.GetLength(0), transform.position);
  }
  
  private void GenerateGrid(int width, int height, Vector3 position)
  {
    if(width > maxGridSize.x && width > height)
    {
      cellSize = (float)maxGridSize.x / width;
    }
    else if(height > maxGridSize.y && height > width)
    {
      cellSize = (float)maxGridSize.y / height;
    }

    Vector3 boardPosition = GetBoardPosition(width, height, cellSize, position);

    grid = new Grid<CrossWordObject>(width, height, cellSize, boardPosition, (Grid<CrossWordObject> g, int x, int y) => new CrossWordObject(g, x, y));

    //GENERATE TRIVIA QUESTION LIST WITH ITEM NUNMBER
    List<(TriviaQuestion triviaQuestion, int index)> triviaQuestionList = new List<(TriviaQuestion triviaQuestion, int index)>();
    for (int i = 0; i < triviaQuestions.Length; i++)
    {
      triviaQuestionList.Add((triviaQuestions[i],i + 1));
    }

    //PLACE EACH CROSSWORD ENTRIES TO THE CORRECT GRID POSITION
    foreach (CrossWordEntry entry in crossWordLayout.crossWordEntries)
    {

      //FIND CROSSWORD ENTRY'S DESIGNATED CLUE
      (TriviaQuestion triviaQuestion, int index) selectedTriviaQuestion = FindTriviaQuestionClue(entry.word, triviaQuestionList);
      triviaQuestionList.Remove(selectedTriviaQuestion);

      //GET CROSSWORD ENTRY'S START X AND START Y
      int startX = entry.wordPlacement.startCol;
      int startY = crossWordLayout.board.GetLength(0) - entry.wordPlacement.startRow -1;

      foreach ((int row, int col, char letter) placedLetter in entry.letterPlacements)
      {
        //ASSIGN THE FUCKING SHIT TO THIS GRID NODE
        CrossWordObject node = grid.GetGridObject(placedLetter.col, crossWordLayout.board.GetLength(0) - placedLetter.row - 1);
        char letter = crossWordLayout.board[placedLetter.row, placedLetter.col];

        //GENERATE CROSSWORD CLUE AND ASSIGN IT TO THE DESIRED NODE
        CrossWordClue crossWordClues = new CrossWordClue(selectedTriviaQuestion.index,new Vector2Int(startX, startY), selectedTriviaQuestion.triviaQuestion, entry.wordPlacement.orientation); 
        node.AssignPlacedWord(letter, crossWordClues);

        grid.TriggerGridObjectChanged(placedLetter.col, crossWordLayout.board.GetLength(0) - placedLetter.row - 1);

      }
    }

    
    //INSTANTIATE LETTERS INSIDE THE GRID
    SpawnCrossWordLetters();

  }

    private Vector3 GetBoardPosition(int width, int height, float cellSize, Vector3 centerPosition)
    {
        float adjustedX = (width * cellSize) / 2f;
        float adjustedY = (height * cellSize) /2f;

        return new Vector3(centerPosition.x - adjustedX, centerPosition.y - adjustedY, centerPosition.z);
    }

    (TriviaQuestion triviaQuestion, int index) FindTriviaQuestionClue(string word, List<(TriviaQuestion triviaQuestion, int index)> triviaQuestions)
  {
    (TriviaQuestion triviaQuestion, int index) selectedTriviaQuestion = (new TriviaQuestion(), -1);

    foreach ((TriviaQuestion triviaQuestion, int index) triviaQuestion in triviaQuestions)
    {
      if(word == triviaQuestion.triviaQuestion.answer.ToLower().Replace(" ",""))
      {
        selectedTriviaQuestion = triviaQuestion;
        break;
      }
    }

    return selectedTriviaQuestion;
  }

  void SpawnCrossWordLetters()
  {

    foreach (CrossWordObject node in grid.GetAllGridObject())
    {
      if(node.letter != '\0')
      {
        Vector3 gridPosition = grid.GetCenterWorldPosition(node.x, node.y);
        Transform crossWordBox = Instantiate(letterTransformPrefab, gridPosition, Quaternion.identity, letterTransformParent);

        //ASSIGN GRID OBJECT TO CROSSWORD BOX GAMEOBJECT
        crossWordBox.GetComponent<CrossWordLetter>().crossWordObject = node;

        //ADJUST LETTER NODE FONT SIZE
        //float fontSize = cellSize * 10f;
        //crossWordBox.GetComponent<TextMeshPro>().fontSize = fontSize;

      }
    }
  }

  private void OnDrawGizmos() 
  {
    Vector3 maxGridPosition = GetBoardPosition(maxGridSize.x, maxGridSize.y, 1, transform.position);
    Gizmos.color = Color.red;

    Gizmos.DrawLine(maxGridPosition, new Vector3(maxGridPosition.x, maxGridPosition.y + maxGridSize.y));
    Gizmos.DrawLine(maxGridPosition, new Vector3(maxGridPosition.x + maxGridSize.x, maxGridPosition.y));
    
    Vector3 upperRightCorner = new Vector3(maxGridPosition.x + maxGridSize.x,  maxGridPosition.y + maxGridSize.y);
    Gizmos.DrawLine(upperRightCorner, new Vector3(maxGridPosition.x, maxGridPosition.y + maxGridSize.y));
    Gizmos.DrawLine(upperRightCorner, new Vector3(maxGridPosition.x + maxGridSize.x, maxGridPosition.y));
  }
    
}

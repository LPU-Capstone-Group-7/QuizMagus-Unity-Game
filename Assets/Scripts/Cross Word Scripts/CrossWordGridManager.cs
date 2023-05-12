using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
public class CrossWordGridManager : MonoBehaviour
{
  public static CrossWordGridManager instance;

  [Header("Grid Component")]
  private float cellSize;
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
  [SerializeField] Transform letterTransformPrefab;
  CrossWordLayout crossWordLayout;

  private void Awake()
  {
      if(instance == null) instance = this;
  }

  private void Start()
  {
    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    stopwatch.Start();

    CreateCrossWordGrid(testWords50);

    stopwatch.Stop();
    Debug.Log ("Time taken: "+(stopwatch.Elapsed));
  }

  public void CreateCrossWordGrid(List<string> words)
  {
    //GENERATE LAYOUT BASED ON LIST OF WORDS
    crossWordLayout = CrossWordGeneration.instance.GenerateCrossWordLayout(words);

    //GENERATE GRID USING THE SIZE OF THE CROSSWORD LAYOUT
    GenerateGrid(crossWordLayout.board.GetLength(1) , crossWordLayout.board.GetLength(0), 0.8f, transform.position);
  }
  
  public void GenerateGrid(int width, int height, float cellSize, Vector3 position)
  {
    this.cellSize = cellSize;

    grid = new Grid<CrossWordObject>(width, height, cellSize, position, (Grid<CrossWordObject> g, int x, int y) => new CrossWordObject(g, x, y));

    //ASSIGN CROSSWORD ENTRIES TO THE CORRECT GRID POSITION
    for (int row = 0; row < crossWordLayout.board.GetLength(0); row++)
    {
      for (int col = 0; col < crossWordLayout.board.GetLength(1); col++)
      {
        char letter =  crossWordLayout.board[row, col];

        if(letter != '\0')
        {          
          //FIND CROSSWORD ENTRY OF THIS COORDINATE
          HashSet<CrossWordEntry> matchingEntries = crossWordLayout.FindMatchingEntries(row, col,letter);

          //ASSIGN TO GRID
          CrossWordObject node = grid.GetGridObject(col, crossWordLayout.board.GetLength(0) -1 - row);
          
          //if(matchingEntries.Count == 0) Debug.Log("No Matching Entries");

          foreach (CrossWordEntry entry in matchingEntries)
          {
            node.AssignPlacedWord(entry.word, letter, entry.orientation);
          }

          grid.TriggerGridObjectChanged(col, crossWordLayout.board.GetLength(0) -1 - row);
        }

      }
    }

    //INSTANTIATE LETTERS INSIDE THE GRID
    SpawnCrossWordLetters();

  }

  void SpawnCrossWordLetters()
  {

    foreach (CrossWordObject node in grid.GetAllGridObject())
    {
      if(node.letter != '\0')
      {
        Vector3 gridPosition = grid.GetCenterWorldPosition(node.x, node.y);
        Transform letterTransform = Instantiate(letterTransformPrefab, gridPosition, Quaternion.identity);

        //CHANGE THE LETTER TO THE CORRESPONDING NODE'S LETTER
        letterTransform.GetComponent<TextMeshPro>().text = node.letter.ToString();

        //ADJUST LETTER NODE FONT SIZE
        float fontSize = cellSize * 10f;
        letterTransform.GetComponent<TextMeshPro>().fontSize = fontSize;

      }
    }
  }
    
}

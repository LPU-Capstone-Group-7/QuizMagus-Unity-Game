using System;
using System.Collections.Generic;
using UnityEngine;

public class CrossWordManager : MonoBehaviour
{
  public static CrossWordManager instance;
  private CrossWordGridManager gridManager;
  private CrossWordSettings crossWordSettings;

  [Header("CrossWordObject")]
  private CrossWordObject selectedCrossWordObject;
  private CrossWordClue activeClue;
  private bool canSelectTiles = true;

  public Action<CrossWordClue> OnActiveClueChangeAction;
  public Action onNodeSelected;
  public Action onNodeAnswered;
  
  private void Awake()
  {
    if(instance == null) instance = this;  
  }
  private void Start()
  {
    crossWordSettings = DataManager.instance.GetGameSettings<CrossWordSettings>();

    //CREATE CROSSWORD GRID
    gridManager = CrossWordGridManager.instance;
    gridManager.CreateCrossWordGrid(crossWordSettings.triviaQuestions);

    //START TIMER
    CrossWordTimer.instance.StartTimer(crossWordSettings.timeLimit);
    
  }

  public void SelectCrossWordObject(CrossWordObject node, Orientation orientation = Orientation.across)
  {
    if(node.crossWordClues.Count == 0) return;
    
    node.isSelected = true;

    if(selectedCrossWordObject == node && selectedCrossWordObject.crossWordClues.Count > 1) //RE-CLICKING THE SAME SELECTED NODE
    {
      activeClue = activeClue.orientation == Orientation.across? node.crossWordClues[Orientation.down] : node.crossWordClues[Orientation.across];
    }
    else //SELECTING A NEW NODE
    {
      selectedCrossWordObject = node;

      if(node.crossWordClues.ContainsKey(orientation)) //DEFAULT ORIENTATION ACROSS
      {
        activeClue = node.crossWordClues[orientation];
      }
      else
      {
        activeClue = node.crossWordClues[Orientation.down];
      }

    }

    //RESET HIGHLIGHTED AND SELECTED ON ALL OTHER NODES
    foreach (CrossWordObject tiles in gridManager.GetGrid().GetAllGridObject())
    {
      if(tiles.letter == '\0') continue;

      if(tiles != node) tiles.isSelected =false;
      tiles.isHighlighted = false;
    }

    //HIGHLIGHT SELECTED ITEM
    HighlightNeighbourTiles(node, activeClue.orientation, true);

    //INVOKE EVENTS
    onNodeSelected?.Invoke();
    OnActiveClueChangeAction?.Invoke(activeClue);


  }

  public void HighlightNeighbourTiles(CrossWordObject crossWordObject, Orientation orientation, bool state)
  {
    int x = crossWordObject.x;
    int y = crossWordObject.y;

    HashSet<CrossWordObject> highlightedTiles = new HashSet<CrossWordObject>();
    highlightedTiles.Add(crossWordObject);

    if (orientation == Orientation.across) //HIGHLIGHT NEIGHBOURS ACROSS
    {
        highlightedTiles.UnionWith(GetNeighboursInDirection(x, y, new Vector2Int(-1, 0)));
        highlightedTiles.UnionWith(GetNeighboursInDirection(x, y, new Vector2Int(1,0)));
    }
    else //HIGHLIGHT NEIGHBOURS DOWNWARDS
    {
        highlightedTiles.UnionWith(GetNeighboursInDirection(x, y, new Vector2Int(0, -1)));
        highlightedTiles.UnionWith(GetNeighboursInDirection(x, y, new Vector2Int(0,1)));
    }

    foreach (CrossWordObject node in highlightedTiles)
    {
        //EXCLUDE ANSWERED TILES
        if(!node.isAnswered) node.isHighlighted = state;
    }
  }

  private void CheckAllNodesForCorrectAnswers()
  {
    foreach (CrossWordGridItem item in gridManager.GetCrossWordGridItems())
    {
      foreach (CrossWordObject node in item.itemNodes)
      {
        //TODO
      }
    }
  }

  public void CheckForCorrectAnswers(CrossWordObject selectedNode)
  {
    //SHOW CORRECT ANSWER ONLY AT THE END OF THE GAME
    if(!crossWordSettings.showCorrectAnswer) return;

    //CHECK IF CROSSWORD ITEM TILES ARE ALREADY ANSWERED
    List<CrossWordGridItem> relatedItems = gridManager.GetRelatedCrossWordGridItems(selectedNode);
    
    foreach(CrossWordGridItem item in relatedItems)
    {
      bool answerIsCorrect = true;
      bool noEmptyTiles = true;

      //VALIDATE IF ANSWER IS CORRECT AND IF THERE ARE NO EMPTY TILES
      foreach (CrossWordObject node in item.itemNodes)
      {
          if(node.inputtedLetter == '\0') noEmptyTiles = false;
          if(node.inputtedLetter != node.letter) answerIsCorrect = false; 
      }

      //UPDATE TIME TAKEN TO ANSWER ON THIS GRID 
      if(noEmptyTiles) item.timeTakenToAnswer = CrossWordTimer.instance.GetTimeTakenToAnswer();

      if(answerIsCorrect)
      {
        item.isAnswered = true;
        item.timeTakenToAnswer = CrossWordTimer.instance.GetTimeTakenToAnswer();

        //DESELECT TILES
        foreach (CrossWordObject node in item.itemNodes)
        {
          node.isAnswered = true;
          node.isHighlighted = false;
          node.isSelected = false;    
        }

        onNodeSelected?.Invoke();
        onNodeAnswered?.Invoke();
      }
    }
        
  }

  private HashSet<CrossWordObject> GetNeighboursInDirection(int startX, int startY, Vector2Int direction)
  {
    int x = startX + direction.x;
    int y = startY + direction.y;

    HashSet<CrossWordObject> crossWordNeighbours = new HashSet<CrossWordObject>();
    CrossWordObject neighbourTile;

    do
    {
      neighbourTile = gridManager.GetGrid().GetGridObject(x, y);

      if (neighbourTile == null || neighbourTile.letter == '\0')
      {
          break; // Reached an invalid neighbour
      }

      crossWordNeighbours.Add(neighbourTile);

      x += direction.x;
      y += direction.y;
    }
    while(neighbourTile != null);

    return crossWordNeighbours;
  }

//CAN HIGHLIGHT TILES BOOLEAN
public bool CanSelectTiles()
{
  return canSelectTiles;
}

public void SetCanSelectTiles(bool state)
{
  canSelectTiles = state;
}

public CrossWordClue GetActiveCLue()
{
  return activeClue;
}
  
  
}
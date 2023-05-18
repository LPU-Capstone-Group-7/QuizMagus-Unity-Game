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

  public Action<CrossWordClue> OnActiveClueChangeAction;
  public Action onNodeSelected;
  
  private void Awake()
  {
    if(instance == null) instance = this;  
  }
  private void Start()
  {
    crossWordSettings = DataManager.instance.GetGameSettings<CrossWordSettings>();

    gridManager = CrossWordGridManager.instance;
    gridManager.CreateCrossWordGrid(crossWordSettings.triviaQuestions);
    
  }

  public void SelectCrossWordObject(CrossWordObject node)
  {
    if(selectedCrossWordObject == node && node.crossWordClues.Count > 1) //RE-CLICKING THE SAME SELECTED NODE
    {
      activeClue = activeClue.orientation == Orientation.across? node.crossWordClues[Orientation.down] : node.crossWordClues[Orientation.across];
    }
    else //SELECTING A NEW NODE
    {
      selectedCrossWordObject = node;
      activeClue = node.crossWordClues.ContainsKey(Orientation.across)? node.crossWordClues[Orientation.across] : node.crossWordClues[Orientation.down];

    }

    //RESET ISHIGHLIGHTED ON ALL NODES
    foreach (CrossWordObject tiles in gridManager.GetGrid().GetAllGridObject())
    {
      if(tiles.letter == '\0') continue;
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
        node.isHighlighted = state;
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
      neighbourTile = CrossWordGridManager.instance.GetGrid().GetGridObject(x, y);

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

  
  
}
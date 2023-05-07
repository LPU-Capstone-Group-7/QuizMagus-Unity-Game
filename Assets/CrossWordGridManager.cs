using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CrossWordGridManager : MonoBehaviour
{
  public static CrossWordGridManager instance;

  [Header("Grid Component")]
  [SerializeField] private int gridSize;
  [SerializeField] private float gridLength;
  private float cellSize;
  Grid<CrossWordObject> grid;

  private void Awake()
  {
      if(instance == null) instance = this;
  }

  public void CreateCrossWordGrid(List<string> words, int gridSize, float cellSize)
  {
    this.gridSize = gridSize;
    
      GenerateGrid(gridSize, gridSize, 0.8f, transform.position);
  }
  
  public void GenerateGrid(int width, int height, float cellSize, Vector3 position)
  {
      grid = new Grid<CrossWordObject>(width, height, cellSize, position, (Grid<CrossWordObject> g, int x, int y) => new CrossWordObject(g, x, y));
  }
   
    private Tuple<int,int,int,int> GetCornerSize(Dictionary<Tuple<int, int>, char> gridDictionary)
  {
    int min_x = 0, max_x = 0;
    int min_y = 0, max_y = 0;

    foreach(var (x,y) in gridDictionary.Keys)
    {
      min_x = Mathf.Min(min_x, x);
      max_x = Mathf.Max(max_x, x);
      min_y = Mathf.Min(min_y, y);
      max_y = Mathf.Max(max_y, y);
    }

    return new Tuple<int, int, int, int>(min_x, min_y, max_x, max_y);
  }
    
}

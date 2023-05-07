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
  private List<string> testWords =  new List<string>{"Elephants","Kangaroos","Crocodiles","Chimpanzees","Flamingos","Rhinoceroses","Gorillas","Cheetahs","Hippopotamuses","Toucans", "Dog", "Cat", "Bat"};

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
    
}

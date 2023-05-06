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
        
        //PLACE FIRST LETTER INSIDE THE GRID IN THE MIDDLE
        PlaceFirstLetter(words[0]);
    }
    
    public void GenerateGrid(int width, int height, float cellSize, Vector3 position)
    {
        grid = new Grid<CrossWordObject>(width, height, cellSize, position, (Grid<CrossWordObject> g, int x, int y) => new CrossWordObject(g, x, y));
    }
    
    private void PlaceFirstLetter(string word)
    {
      //PLACE IT IN THE FUCKING MIDDLE
      int startX = Mathf.RoundToInt(gridSize/2);
      int startY = gridSize - 1;
      
      PlaceWordVertically();
    }
    
    private void PlaceWordVertically(int startX, int startY, string word)
    {
      for(int i = 0; i < word.length; i++)
      {
        CrossWordObject node = grid.getGridObject(startX, startY+ i);
        
        //ASSIGN CORRESPONDING WORD AND LETTER
        
        
      }
    }
    
    
}

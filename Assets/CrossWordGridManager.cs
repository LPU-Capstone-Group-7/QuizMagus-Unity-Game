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

    public void CreateCrossWordGrid()
    {
        
    }
    
    public void GenerateGrid(int width, int height, float cellSize, Vector3 position)
    {
        grid = new Grid<CrossWordObject>(width, height, cellSize, position, (Grid<CrossWordObject> g, int x, int y) => new CrossWordObject(g, x, y));
    }
}

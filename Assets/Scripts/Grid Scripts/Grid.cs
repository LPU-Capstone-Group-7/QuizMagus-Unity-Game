using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Grid<TGridObject>
{ 
    private int width;
    private int height;
    private Vector3 originPos;
    private float cellSize;
    private TGridObject[,] gridArray;
   private TextMesh[,] debugTextArray;

    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    public class OnGridObjectChangedEventArgs : EventArgs
    {
        public int x;
        public int y;
    }
    //Grid Struct
    public Grid(int width, int height, float cellSize,Vector3 originPos, Func<Grid<TGridObject>,int,int,TGridObject> CreateGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPos = originPos;

        gridArray = new TGridObject[width,height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
               gridArray[x, y] = CreateGridObject(this,x,y);
            }
        }
        debugTextArray = new TextMesh[width, height];
        bool showDebug = true;
        if (showDebug)
        {
            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    Debug.DrawLine(GetGridWorldPosition(x, y), GetGridWorldPosition(x, y + 1), Color.white, 100f);
                    Debug.DrawLine(GetGridWorldPosition(x, y), GetGridWorldPosition(x + 1, y), Color.white, 100f);
                    //debugTextArray[x, y] = CreateWorldText(x.ToString() + "," + y.ToString(), null, GetCenterWorldPosition(x, y) + new Vector3(cellSize, cellSize) * .5f, 1, Color.white, TextAnchor.MiddleCenter);

                }
            }

            Debug.DrawLine(GetGridWorldPosition(0, height), GetGridWorldPosition(width, height), Color.white, 100f);
            Debug.DrawLine(GetGridWorldPosition(width, 0), GetGridWorldPosition(width, height), Color.white, 100f);

            OnGridObjectChanged += (object sender, OnGridObjectChangedEventArgs eventArgs) =>
            {
                //debugTextArray[eventArgs.x, eventArgs.y].text = gridArray[eventArgs.x, eventArgs.y]?.ToString();
            };
        }
    }
    //Get the world position given the x and y coords
    public Vector3 GetGridWorldPosition(int x, int y)
    {
        return new Vector3(x,y) * cellSize + originPos;
    }

    //Get the grid world position given a world position
    public Vector3 GetGridWorldPosition(Vector3 worldPosition)
    {
        GetXY(worldPosition, out int x, out int y);
        return GetGridWorldPosition(x, y);
    }


    //Get the center world position given the x and y coords
    public Vector3 GetCenterWorldPosition(int x, int y)
    {
        return GetGridWorldPosition(x, y) + new Vector3(cellSize, cellSize) * .5f;
    }

    //Get the centered world position using a vector3 world position
    public Vector3 GetCenterWorldPosition(Vector3 worldPosition)
    {
        GetXY(worldPosition, out int x, out int y);
        return GetCenterWorldPosition(x, y);
    }

    // Get the x and y coords given the world position
    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - originPos).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPos).y / cellSize);
    }
    //Set the value of the grid given the x and y
    public void SetGridObject(int x, int y, TGridObject value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
            debugTextArray[x, y].text = gridArray[x,y].ToString();
            TriggerGridObjectChanged(x, y);
        }
    }

    public void TriggerGridObjectChanged(int x, int y)
    {
        OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, y = y });
    }
    //Set the value of the grid given the x and y
    public void SetGridObject(Vector3 worldPosition, TGridObject value)
    {
        GetXY(worldPosition,out int x,out int y);
        SetGridObject(x, y, value);
    }
    //Get the value of the x and y coords of the grid
    public TGridObject GetGridObject(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            return default(TGridObject);
        }
    }
    //Get the value of x and y given the worldPosition
    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        GetXY(worldPosition, out int x, out int y);
        return GetGridObject(x, y);
    }

    public int GetWidth()
    { 
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public float GetCellSize()
    {
        return cellSize;
    }

    public Vector3 GetOriginPos()
    {
        return originPos;
    }

    private TextMesh CreateWorldText(string text, Transform parent, Vector3 localPosition, int fontSize, Color color, TextAnchor textAnchor)
    {
        GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.anchor = textAnchor;
        //  textMesh.alignment = textAlignment;
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        //  textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        return textMesh;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System;

public class WordSearchGridManager : MonoBehaviour
{
    public static WordSearchGridManager instance;
    private bool isDebugging = false;

    [Header("Grid Component")]
    [SerializeField] private int gridSize;
    [SerializeField] private float gridLength;
    private float cellSize;
    Grid<LetterGridObject> grid;

    [Header("Word Search Letter Components")]
    [SerializeField] Transform letterTransformPrefab;
    private List<Transform> letterTransformList = new List<Transform>();

    [Header("Word Search Grid Settings")]
    private bool allowBackwards = false;
    private bool allowDiagonals = false;

    [Header("Word Search Grid Action")]
    public Action onGridRefresh;
    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        // //FOR DEBUGGING AND SHOWING THE ANSWERS INSIDE THE GRID
        // if(Input.GetKey(KeyCode.Space))
        // {
        //     foreach (Transform letterTransform in letterTransformList)
        //     {
        //         LetterGridObject node = GetLetterGridObject(letterTransform.position);
        //         if(node.isOccupied()) letterTransform.GetComponent<TextMeshPro>().color = Color.green;
        //     }

        //     isDebugging = true;
        // }

        // else if(isDebugging && Input.GetKeyUp(KeyCode.Space))
        // {
        //     foreach (Transform letterTransform in letterTransformList)
        //     {
        //         LetterGridObject node = GetLetterGridObject(letterTransform.position);
        //         if(node.isOccupied()) letterTransform.GetComponent<TextMeshPro>().color = Color.white;
        //     }

        //     isDebugging = false;
        // }
    }

    public void HighlightWordsInGrid(List<string> words, float duration)
    {
        foreach (Transform letterTransform in letterTransformList)
        {
            LetterGridObject node = GetLetterGridObject(letterTransform.position);
            List<string> commonWords = node.assignedWords.Intersect<string>(words).ToList<string>();

            if(node.isOccupied() && commonWords.Count > 0) letterTransform.GetComponent<TextMeshPro>().color = Color.green;
        }

        FunctionTimer.Create(() => {
            foreach (Transform letterTransform in letterTransformList)
            {
                LetterGridObject node = GetLetterGridObject(letterTransform.position);
                if(node.isOccupied()) letterTransform.GetComponent<TextMeshPro>().color = Color.white;
            }
        },duration);
    }

    public void CreateWordSearchGrid(int gridSize, float gridLength, List<string> words, bool allowBackwards, bool allowDiagonals)
    {
        //GLOBALIZE WORD SEARCH GRID PARAMETERS
        this.gridSize = gridSize;
        this.gridLength = gridLength;
        this.cellSize = gridLength / gridSize;
        this.allowBackwards = allowBackwards;
        this.allowDiagonals =  allowDiagonals;

        //GENERATE WORD SEARCH GRID
        GenerateGrid(gridSize, gridSize, cellSize, transform.position);
        InsertWordSearchContents(words);

        //INSTANTIATE LETTERS INSIDE THE GRID
        SpawnLetterTransformsInGrid();
    }

    private void InsertWordSearchContents(List<string> words)
    {
        //SORT THE WORD BY LENGTH
        words.Sort((s1, s2) => s1.Length.CompareTo(s2.Length));
        words.Reverse();

        Debug.Log(words[0]);

        //PLACE THE FIRST WORD INTO THE GRID
        for (int i = 0; i < words.Count; i++)
        {
            bool hasformattedWord = TryToPlaceWordInGrid(words[i]);

        }

        FillEmptyCellsWithRandomLetters();
    }

    public void SpawnLetterTransformsInGrid()
    {
        //SPAWN LETTER PARENTS TRANSFORM
        GameObject parentObject = new GameObject("Grid Letter Parent");
        parentObject.transform.position = Vector3.zero;

        foreach (LetterGridObject node in GetAllLetterGridObject())
        {
            //SPAWN LETTERS
            Vector3 nodePosition = GetGrid().GetCenterWorldPosition(node.x,node.y);
            Transform letterTransform = Instantiate(letterTransformPrefab, nodePosition, Quaternion.identity, parentObject.transform);
            letterTransform.GetComponent<GridLetter>().letterGridObject = node;

            //CHANGE THE LETTER TO THE CORRESPONDING NODE'S LETTER
            letterTransform.GetComponent<TextMeshPro>().text = node.letter.ToString();

            //ADJUST LETTER NODE FONT SIZE
            float fontSize = cellSize * 10f;
            letterTransform.GetComponent<TextMeshPro>().fontSize = fontSize;

            letterTransformList.Add(letterTransform);
            
        }
    }
    
    public void RefreshWordSearchContents(List<string> words)
    {
        onGridRefresh?.Invoke();
        //HIDE LETTER TRANSFORM
        foreach (Transform letterTransform in letterTransformList)
        {
            letterTransform.GetComponent<Animator>().Play("Letter_Exit");
        }

        //DELAY BEFORE GENERATING NEW WORD SEARCH GRID
        FunctionTimer.Create(() => {
            //CLEAR CURRENT WORD SEARCH GRID
            ClearAllLetterGridObjects();

            //INSERT NEW WORD SEARCH CONTENTS
            InsertWordSearchContents(words);
            
            //REASSIGN LETTER GAME OBJECTS TO NEW LETTERS DEPENDING ON THEIR GRID POSITION
            foreach (Transform letterTransform in letterTransformList)
            {
                LetterGridObject node = GetGrid().GetGridObject(letterTransform.position);
                letterTransform.GetComponent<TextMeshPro>().text = node.letter.ToString();
                letterTransform.GetComponent<GridLetter>().letterGridObject = node;
            }
            foreach (Transform letterTransform in letterTransformList)
            {
                letterTransform.GetComponent<Animator>().Play("Letter_Enter");
            }
        }, .75f);
    }

    void FillEmptyCellsWithRandomLetters()
    {
        foreach (LetterGridObject node in GetAllLetterGridObject())
        {
            if(!node.isOccupied())
            {
                node.setNodeWordLetter(null, (char)('A' + UnityEngine.Random.Range(0, 26)));
            }
        }
    }

     private bool TryToPlaceWordInGrid(string word)
    {
        //TRY TO PLACE WORD HORIZONTALLY OR VERTICALLY RANDOMLY
        System.Random random = new System.Random();
        bool placed = false;

        //OPEN LIST AND CLOSE LIST OF LETTER GRID NODES
        List<LetterGridObject> openList = new List<LetterGridObject>(GetAllLetterGridObject());
        List<LetterGridObject> closeList = new List<LetterGridObject>();


        while(!placed && openList.Count > 0)
        {
            //CHOOSE RANDOM INDEX INSIDE THE OPEN LIST
            int randomIndex = random.Next(openList.Count);
            LetterGridObject node = openList[randomIndex];

            int orientation = random.Next(0,3); // 0 = HORIZONTAL 1 = VERTICAL 2 = DIAGONAL
            if(orientation == 2 && word.Length >= gridSize / 2){orientation = random.Next(0,2);}

            //CHANGES THE FORMAT OF THE WORD TO BE PLACED INTO THE GRID
            bool placeWordBackwards = random.Next(0,3) == 0 && allowBackwards;
            string formattedWord = placeWordBackwards? new string(word.Reverse().ToArray()) : word;
            formattedWord = formattedWord.Replace(" ","");

            //REMOVE NODE FROM OPEN LIST AND ADD IT TO CLOSE LIST IF ORIENTATION IS HORIZONTAL OR VERTICAL
            if(orientation != 2)
            {
                openList.RemoveAt(randomIndex);
                closeList.Add(node);
            }

            switch (orientation)
            {
                case 0:
                     if(CanPlaceWordHorizontally(node.x, node.y, formattedWord))
                    {
                        PlaceWordHorizontally(node.x, node.y, word, formattedWord);
                        placed = true;
                    }
                    else if(CanPlaceWordVertically(node.x, node.y, formattedWord))
                    {
                        PlaceWordVertically(node.x, node.y, word, formattedWord);
                        placed = true;
                    }
                    break;
                case 1:
                     if(CanPlaceWordVertically(node.x, node.y, formattedWord))
                    {
                        PlaceWordVertically(node.x, node.y, word, formattedWord);
                        placed = true;
                    }
                    else if(CanPlaceWordHorizontally(node.x, node.y, word))
                    {
                        PlaceWordHorizontally(node.x, node.y, word, formattedWord);
                        placed = true;
                    }
                    break;
                case 2:
                    if(CanPlaceWordDiagonally(node.x, node.y, word))
                    {
                        PlaceWordDiagonally(node.x, node.y, word, formattedWord);
                        placed = true;

                        //ONLY RMOVES THE NODE FROM THE LIST IF IT WAS ADDED DIAGONALLY SUCCESFULLY
                        openList.RemoveAt(randomIndex);
                        closeList.Add(node);
                    }
                    break;

                default:
                    break;
            }

        }

        if(!placed) Debug.Log(word + " Cannot be added in the grid");
        return placed;
    }

    private bool CanPlaceWordHorizontally(int startX, int startY, string formattedWord)
    {
        //CHECK IF THE WORD CAN BE PLACED HORIZONTALLY
        if(startX + formattedWord.Length > gridSize) return false;

        for (int i = 0; i < formattedWord.Length; i++)
        {
            LetterGridObject node = GetLetterGridObject(startX + i, startY);
            if(node.isOccupied() && node.letter != formattedWord[i]) return false;
        }

        return true;
    }

    void PlaceWordHorizontally(int startX, int startY, string word, string formattedWord)
    {
        //PLACE THE WORD HORIZONTALLY STARTING AT STARTX,STARTY
        for (int i = 0; i < formattedWord.Length; i++)
        {
            LetterGridObject node = GetLetterGridObject(startX + i, startY);
            node.setNodeWordLetter(word, formattedWord[i]);
            GetGrid().TriggerGridObjectChanged(startX + i, startY);
        }
    }

    private bool CanPlaceWordVertically(int startX, int startY, string formattedWord)
    {
        //CHECK IF IT CAN PLACE WORD DOWNWARD VERTICALLY
        if(startY - formattedWord.Length < 0) return false;

        for (int i = 0; i < formattedWord.Length; i++)
        {
            LetterGridObject node = GetLetterGridObject(startX, startY - i);
            if(node.isOccupied() && node.letter != formattedWord[i]) return false;
        }

        return true;
    }

    void PlaceWordVertically(int startX, int startY, string word, string formattedWord)
    {
        //PLACE THE WORD HORIZONTALLY STARTING AT STARTX,STARTY
        for (int i = 0; i < formattedWord.Length; i++)
        {
            LetterGridObject node = GetLetterGridObject(startX, startY - i);
            node.setNodeWordLetter(word, formattedWord[i]);
            GetGrid().TriggerGridObjectChanged(startX, startY - i);
        }
    }

    private bool CanPlaceWordDiagonally(int startX, int startY, string formattedWord)
    {
        //CHECK IF IT CAN BE PLACED DIAGONALLY TO THE RIGHT DOWNWARDS
        if(startX + formattedWord.Length > gridSize || startY - formattedWord.Length < 0) return false;

        for (int i = 0; i < formattedWord.Length; i++)
        {
            LetterGridObject node = GetLetterGridObject(startX + i, startY - i);
            if(node.isOccupied() && node.letter != formattedWord[i]) return false;
        }

        return true;
    }

    void PlaceWordDiagonally(int startX, int startY, string word, string formattedWord)
    {
        //PLACE THE WORD DIAGONALLY STARTING AT START X, START Y
        for (int i = 0; i < formattedWord.Length; i++)
        {
            LetterGridObject node = GetLetterGridObject(startX + i, startY - i);
            node.setNodeWordLetter(word, formattedWord[i]);
            GetGrid().TriggerGridObjectChanged(startX + i, startY - i);
        }

    }

    public void GenerateGrid(int width, int height, float cellSize, Vector3 position)
    {
        grid = new Grid<LetterGridObject>(width, height, cellSize, position, (Grid<LetterGridObject> g, int x, int y) => new LetterGridObject(g, x, y));
    }

    public LetterGridObject GetLetterGridObject(int x, int y)
    {
        return grid.GetGridObject(x,y);
    }

    public LetterGridObject GetLetterGridObject(Vector3 position)
    {
        return grid.GetGridObject(position);
    }

    public List<LetterGridObject> GetAllLetterGridObject()
    {
        List<LetterGridObject> letterGridObjectList = new List<LetterGridObject>();

        for (int x = 0; x < grid.GetWidth(); x++)
        {   
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                letterGridObjectList.Add(GetLetterGridObject(x,y));
            }
        }

        return letterGridObjectList;
    }

    public void ClearAllLetterGridObjects()
    {
        foreach (LetterGridObject node in GetAllLetterGridObject())
        {
            node.ClearNodeWordLetter();
        }
    }

    public Grid<LetterGridObject> GetGrid()
    {
        return grid;
    }

    private void OnDrawGizmos()
    {
        float cellSize = gridLength / gridSize;

        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + gridLength, transform.position.y));
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y + gridLength));


        Gizmos.color = Color.red;
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Gizmos.DrawLine(new Vector3(x, y) * cellSize + transform.position, new Vector3(x, y +1) * cellSize + transform.position);
                Gizmos.DrawLine(new Vector3(x, y) * cellSize + transform.position, new Vector3(x + 1, y) * cellSize + transform.position);
            }
        }

        Gizmos.DrawLine(new Vector3(0, gridSize) * cellSize + transform.position, new Vector3(gridSize, gridSize) * cellSize + transform.position);
        Gizmos.DrawLine(new Vector3(gridSize, 0) * cellSize + transform.position, new Vector3(gridSize, gridSize) * cellSize + transform.position);
    }
}
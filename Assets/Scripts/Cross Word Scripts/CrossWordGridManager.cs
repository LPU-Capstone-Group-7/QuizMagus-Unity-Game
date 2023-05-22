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


    List<string> testWords01 = new List<string>{"dog", "duck"};
    Grid<CrossWordObject> grid;

    [Header("CrossWord Components")]
    [SerializeField] Transform letterTransformParent;
    [SerializeField] Transform letterTransformPrefab;
    TriviaQuestion[] triviaQuestions;
    CrossWordLayout crossWordLayout;
    [SerializeField] List<CrossWordGridItem> crossWordGridItems = new List<CrossWordGridItem>();

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

        //CENTER CROSSWORD BOARD AND SET CAMERA BOUNDING BOX DEPENDING ON THE SIZE OF THE GRID
        Vector3 boardPosition = GetBoardPosition(width, height, cellSize, position);
        GameObject.FindObjectOfType<CameraDragController>().SetCameraBoundingBox(new Vector2(width * cellSize, height * cellSize) /2, position, 1.5f);

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
            //INITIALIZE NEW NODE LIST
            HashSet<CrossWordObject> nodeList = new HashSet<CrossWordObject>();

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
                nodeList.Add(node);

                grid.TriggerGridObjectChanged(placedLetter.col, crossWordLayout.board.GetLength(0) - placedLetter.row - 1);

            }

            //CREATE CROSSWORD GRID ITEM TO KEEP TRACK OF ANSWERED AND UNANSWERED QUESTIONS AND EACH ITEM'S STATES
            CrossWordGridItem item = new CrossWordGridItem(selectedTriviaQuestion.index, selectedTriviaQuestion.triviaQuestion, nodeList, entry.wordPlacement.orientation, false);
            crossWordGridItems.Add(item);
        }

        //INSTANTIATE LETTERS INSIDE THE GRID
        SpawnCrossWordLetters();

        //GENERATE LIST OF QUESTIONS
        CrossWordClueUIHandler.instance.ListAllClues(crossWordGridItems);

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
                crossWordBox.GetComponent<CrossWordTile>().SpawnCrossWordTile(node, cellSize);

            }
        }
    }

    public Grid<CrossWordObject> GetGrid()
    {
        return grid;
    }

    //CROSSWORD GRID ITEM FUNCTIONS
    public List<CrossWordGridItem> GetRelatedCrossWordGridItems(CrossWordObject selectedNode)
    {
        List<CrossWordGridItem> relatedItems = new List<CrossWordGridItem>();

        foreach (CrossWordGridItem item in crossWordGridItems)
        {
            if(!item.isAnswered && item.itemNodes.Contains(selectedNode) && selectedNode.getCrossWordCluesIndex().Contains(item.index))
            {
                relatedItems.Add(item);
            }
        }

        return relatedItems;
    }

    public string GetCrossWordGridItemInputtedAnswer(CrossWordGridItem item)
    {
        string inputtedAnswer = "";

        foreach (CrossWordObject node in item.itemNodes)
        {
            inputtedAnswer += node.inputtedLetter;
        }

        return inputtedAnswer.ToLower();
    }

    public List<CrossWordGridItem> GetCrossWordGridItems()
    {
        return crossWordGridItems;
    }

}

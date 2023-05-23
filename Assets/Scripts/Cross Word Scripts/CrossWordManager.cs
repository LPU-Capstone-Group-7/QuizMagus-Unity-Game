using System;
using System.Collections.Generic;
using UnityEngine;

public class CrossWordManager : MonoBehaviour
{
    public static CrossWordManager instance;
    private CrossWordGridManager gridManager;
    private CrossWordSettings crossWordSettings;

    [Header("Trivia Question")]
    TriviaQuestion[] inputtedTriviaQuestions;

    [Header("CrossWordObject")]
    private CrossWordObject selectedCrossWordObject;
    private CrossWordClue activeClue;
    private bool canSelectTiles = true;

    [Header("Event Handlers")]
    public Action<CrossWordClue> OnActiveClueChangeAction;
    public Action onNodeSelected;
    public Action onNodeAnswered;
    public Action<CrossWordGridItem> onCrossWordItemAnswered;
    
    private void Awake()
    {
        if(instance == null) instance = this;  
    }
    private void Start()
    {
        crossWordSettings = DataManager.instance.GetGameSettings<CrossWordSettings>();

        //RANDOMIZES ORDER WHILE ALSO RETAINING THE ORGINAL ORDER OF THE ARRAY
        inputtedTriviaQuestions = new TriviaQuestion[crossWordSettings.triviaQuestions.Length];
        System.Array.Copy(crossWordSettings.triviaQuestions, inputtedTriviaQuestions, crossWordSettings.triviaQuestions.Length);
        if(crossWordSettings.randomizeQuestions) inputtedTriviaQuestions = GameManager.instance.ShuffleTriviaQuestions(inputtedTriviaQuestions);

        //CREATE CROSSWORD GRID
        gridManager = CrossWordGridManager.instance;
        gridManager.CreateCrossWordGrid(inputtedTriviaQuestions);

        //START TIMER
        CrossWordTimer.instance.StartTimer(crossWordSettings.timeLimit);
        CrossWordTimer.instance.onTimeFinished += CheckAllNodesForCorrectAnswers;

        //ANSWER SOME TILES DEPENDING ON THE UNIQUE SETTINGS
        if(crossWordSettings.numOfLettersToShow > 0) AnswerRandomTiles(crossWordSettings.numOfLettersToShow);
    }
    
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            CheckAllNodesForCorrectAnswers();
        }
    }

    private void AnswerRandomTiles(int numOfLettersToShow)
    {
        //GENERATE LIST OF OCCUPIED NODES
        List<CrossWordObject> crossWordObjectList = gridManager.GetGrid().GetAllGridObject();
        crossWordObjectList.RemoveAll(node => node.letter == '\0');

        for (int i = 0; i < numOfLettersToShow; i++)
        {
            //GET RANDOM NODE FROM LIST
            int randomIndex = UnityEngine.Random.Range(0, crossWordObjectList.Count);
            CrossWordObject node = crossWordObjectList[randomIndex];

            node.isAnswered = true;
            node.inputtedLetter = node.letter;
        }

        onNodeAnswered?.Invoke();
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

    public void SelectCrossWordObject(CrossWordObject node, Orientation orientation = Orientation.across)
    {
        if(node.crossWordClues.Count == 0) return;
        
        node.isSelected = true;
        StartCoroutine(CameraDragController.instance.FocusCameraOnThisPosition(gridManager.GetGrid().GetCenterWorldPosition(node.x, node.y)));

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

    //FUNCTIONS FOR CHECKING IF ANSWER IS CORRECT
    private void CheckAllNodesForCorrectAnswers()
    {
        //PAUSE CROSSWORD TIMER
        CrossWordTimer.instance.PauseTimer();

        //CHECK ALL NODE FOR CORRRECT ANSWERS
        foreach (CrossWordGridItem item in gridManager.GetCrossWordGridItems())
        {
            CheckCrossWordGridItem(item);

            //UPDATE TIME TAKEN TO ANSWER ON ITEMS WITH ZERO VALUES
            if(item.timeTakenToAnswer == 0f)
            {
                item.timeTakenToAnswer = CrossWordTimer.instance.GetTimeTakenToAnswer();
            }
        }

        //END GAME
        CalculateCrossWordResults();
    }

    public void CheckForCorrectAnswers(CrossWordObject selectedNode)
    {
        //SHOW CORRECT ANSWER ONLY AT THE END OF THE GAME
        if(!crossWordSettings.showCorrectAnswer) return;

        //CHECK IF CROSSWORD ITEM TILES ARE ALREADY ANSWERED
        List<CrossWordGridItem> relatedItems = gridManager.GetRelatedCrossWordGridItems(selectedNode);
        
        foreach (CrossWordGridItem item in relatedItems)
        {
            CheckCrossWordGridItem(item);   
        }

        bool allItemsAreAnswered = true;

        //CHECK IF ALL NODES ARE ANSWERED, IF THEY ARE THEN END THE GAME
        foreach (CrossWordGridItem item in gridManager.GetCrossWordGridItems())
        {
            if(!item.isAnswered)
            {
                allItemsAreAnswered = false;
                break;
            }
        }

        if(allItemsAreAnswered) CalculateCrossWordResults();
            
    }

    public void CheckCrossWordGridItem(CrossWordGridItem item)
    {
        bool answerIsCorrect = true;

        //VALIDATE IF ANSWER IS CORRECT AND IF THERE ARE NO EMPTY TILES
        foreach (CrossWordObject node in item.itemNodes)
        {
            if(node.inputtedLetter != node.letter) answerIsCorrect = false; 
        }

        if(answerIsCorrect)
        {
            item.isAnswered = true;

            //DESELECT TILES
            foreach (CrossWordObject node in item.itemNodes)
            {
                node.isAnswered = true;
                node.isHighlighted = false;
                node.isSelected = false;    
            }

            onCrossWordItemAnswered?.Invoke(item);
            onNodeSelected?.Invoke();
            onNodeAnswered?.Invoke();
        }
    }    

    //FUNCTIONS FOR CROSSWORD FINAL RESULTS
    public void CalculateCrossWordResults()
    {
        //INITIALIZE REQUIRED LIST NEEDED
        List<CrossWordGridItem> crossWordGridItems = gridManager.GetCrossWordGridItems();
        List<TriviaData> triviaDataList = new List<TriviaData>();

        foreach (CrossWordGridItem item in crossWordGridItems)
        {
            TriviaQuestion triviaQuestion = item.triviaQuestion;

            //GENERATE TRIVIA DATA DEPENDING ON THE RESULT
            if(item.isAnswered)
            {
                TriviaData triviaData = new TriviaData(triviaQuestion.id, item.timeTakenToAnswer, true, triviaQuestion.question, triviaQuestion.answer, triviaQuestion.answer);
                triviaDataList.Add(triviaData);
            }
            else
            {
                string inputtedAnswer = gridManager.GetCrossWordGridItemInputtedAnswer(item);
                TriviaData triviaData = new TriviaData(triviaQuestion.id, item.timeTakenToAnswer, false, triviaQuestion.question, triviaQuestion.answer, inputtedAnswer);
                triviaDataList.Add(triviaData);
            }            
        }

        //SEND RESULTS TO TRIVIA GAME RESULT MANAGER SCRIPT
        TriviaGameResultManager.instance.SetTriviaGameResults(triviaDataList.ToArray(), crossWordSettings.basedGrading, CrossWordTimer.instance.GetTimeTakenToAnswer());
        GameManager.instance.LoadLevel("TriviaGameCredits");

    }

    //GET ANS SET FUNCTIONS
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
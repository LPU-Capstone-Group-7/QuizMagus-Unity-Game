using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WordSearchLineController : MonoBehaviour
{
    [SerializeField] private Transform lineTransformPrefab;
    private Transform activeLineTransform;
    private List<Transform> lineTransformList = new List<Transform>();
    private LetterGridObject startNode;
    private LetterGridObject endNode;
    bool lastAnswerIsCorrect = true;
    private List<HighlightedWord> highlightedWordList = new List<HighlightedWord>();
    private LetterGridObject hoveredNode;

    // Start is called before the first frame update
    void Start()
    {
        WordSearchGridManager.instance.onGridRefresh += RemoveAllLineTransforms;
        WordSearchManager.instance.onQuestionLoads += () => {if(lastAnswerIsCorrect) CreateNewActiveLineTransform();};
        WordSearchGridManager.instance.onGridGenerates += CreateNewActiveLineTransform;
    
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //SET THE VALID NODE AS THE NEW STARTING NODE
        if(Input.GetMouseButtonDown(0) && PositionIsWithinTheGrid(mouseWorldPosition))
        {
            startNode = WordSearchGridManager.instance.GetLetterGridObject(mouseWorldPosition);
        }

        //MOUSE IS HELD DOWN SET THE CURRENT MOUSE POSITION AS THE END NODE
        if(startNode != null && Input.GetMouseButton(0))
        {
            LetterGridObject potentialEndNode = WordSearchGridManager.instance.GetLetterGridObject(mouseWorldPosition);

            if(potentialEndNode != null)
            {
                endNode = potentialEndNode; 
                PlayPopSFX(endNode);
            }
        }

        //DRAW LINE
        if(startNode != null && endNode != null)
        {
            if(activeLineTransform == null) CreateNewActiveLineTransform();
            activeLineTransform?.gameObject.SetActive(true);

            LineRenderer lineRenderer = activeLineTransform.GetComponent<LineRenderer>();

            //SET LINE'S ENDPOINTS
            lineRenderer.SetPosition(0, WordSearchGridManager.instance.GetGrid().GetCenterWorldPosition(startNode.x, startNode.y));
            lineRenderer.SetPosition(1, WordSearchGridManager.instance.GetGrid().GetCenterWorldPosition(endNode.x, endNode.y));
            
        }

        //RELEASES MOUSE HOLD SET THE STARTING NODE AS NULL
        if(Input.GetMouseButtonUp(0) && activeLineTransform != null && activeLineTransform.gameObject.activeInHierarchy)
        {
            string highlightedAnswer = "";
            if(startNode != null && endNode != null && HighlightedPossibleAnswer(startNode, endNode, out highlightedAnswer))
            {
                WordSearchManager.instance.AnswerWordSearchQuestion(highlightedAnswer, out lastAnswerIsCorrect);
                if(lastAnswerIsCorrect) highlightedWordList.Add(new HighlightedWord(highlightedAnswer, startNode, endNode));
            }
        
            startNode = null;
            endNode = null;
            activeLineTransform?.gameObject.SetActive(false);
        }
    }

    void PlayPopSFX(LetterGridObject endNode)
    {
        if(endNode != hoveredNode)
        {
            if(!AudioManager.instance.isPlaying("Bubble Pop"))AudioManager.instance.Play("Bubble Pop");
            hoveredNode = endNode;
        }
    }

    private void CreateNewActiveLineTransform()
    {
        //INSTANTIATE LINE TRANSFORM FOR LINE CONTROLLER
        activeLineTransform = Instantiate(lineTransformPrefab, transform.position, Quaternion.identity);

        //CHANGE WIDTH OF LINE TRANSFORM
        float lineWidth = Mathf.Min(1, WordSearchGridManager.instance.GetGrid().GetCellSize());
        activeLineTransform.GetComponent<LineRenderer>().startWidth = lineWidth;


        lineTransformList.Add(activeLineTransform);
        activeLineTransform.gameObject.SetActive(false);
    }

    private void RemoveAllLineTransforms()
    {
        foreach (Transform lineTransform in lineTransformList)
        {
            Destroy(lineTransform.gameObject);
        }

        lineTransformList.Clear();
        highlightedWordList.Clear();
    }

    private bool HighlightedPossibleAnswer(LetterGridObject startNode, LetterGridObject endNode, out string highlightedAnswer)
    {
        List<LetterGridObject> highlightedNodes = new List<LetterGridObject>();
        highlightedAnswer = "";

        bool horizontallyAdjacent = startNode.x == endNode.x;
        bool verticallyAdjacent = startNode.y == endNode.y;
        bool diagonallyAdjacent = Mathf.Abs(startNode.x - endNode.x) == Mathf.Abs(startNode.y - endNode.y);

        if(!horizontallyAdjacent && !verticallyAdjacent && !diagonallyAdjacent) return  false;

        int startX = startNode.x < endNode.x? startNode.x : endNode.x;
        int endX = startNode.x > endNode.x? startNode.x : endNode.x;

        int startY = startNode.y < endNode.y? startNode.y : endNode.y;
        int endY = startNode.y > endNode.y? startNode.y : endNode.y;

        //CYCLE THROUGH THE HORIZONTALLY HIGHLIGHTED NODES AND THEN CHECK IF THEY ALL HAVE SIMILAR ASSIGNED WORD
        if(horizontallyAdjacent)
        {
            for (int y = startY; y <= endY; y++)
            {
                highlightedNodes.Add(WordSearchGridManager.instance.GetLetterGridObject(startNode.x, y));        
            }
        }

        //CYCLE THROUGH THE VERTICALLY HIGHLIGHTED NODES AND THEN CHECK IF THEY ALL HAVE SIMILAR ASSIGNED WORD
        if(verticallyAdjacent)
        {
            for (int x = startX; x <= endX; x++)
            {
                highlightedNodes.Add(WordSearchGridManager.instance.GetLetterGridObject(x, startNode.y));
            }
        }

        //CYCLE THROUGH THE DIAGONALLY HIGHLIGHTED NODES AND THEN CHECK IF THEY ALL HAVE SIMILAR ASSIGNED WORD
        if(diagonallyAdjacent)
        {
            for (int x = startX, y = endY; x <= endX; x++, y--)
            {
                highlightedNodes.Add(WordSearchGridManager.instance.GetLetterGridObject(x, y));
            }
        }

        //CHECK IF ALL HIGHLIGHTED NODES ARE OCCUPIED
        foreach (LetterGridObject node in highlightedNodes)
        {
            if(!node.isOccupied()) return false;
        }

        //FIND HIGHLIGHTED WORD'S COMMON ASSIGNED WORD
        string highlightedWord = "";
        IEnumerable<string> commonWords = highlightedNodes.First().assignedWords;

        foreach (LetterGridObject node in highlightedNodes)
        {
            highlightedWord += node.letter;
            commonWords = commonWords.Intersect(node.assignedWords);
        }

        bool wordIsHiglighted = isWordAlreadyHighlighted(new HighlightedWord(commonWords.ToList()[0], startNode, endNode)) ;

        //CHECK IF THE NUMBER OF HIGHLIGHTED NODES IS SIMILAR TO THE LENGHT OF THE COMMON WORD, THAT MEANS ALL THE LETTERS WERE HIGHLIGHTED
        string trimmedCommonWord = commonWords.ToList()[0].Replace(" ", "");
        if(commonWords.Count<string>() > 0 && highlightedNodes.Count == trimmedCommonWord.Length && !wordIsHiglighted)
        {
            highlightedAnswer = commonWords.ToList()[0];
            return true;
        }

        Debug.Log("Not Highlighted Word: " + highlightedWord);
        return false;
    }

    private bool PositionIsWithinTheGrid(Vector3 mousePos)
    {
        LetterGridObject node = WordSearchGridManager.instance.GetLetterGridObject(mousePos);
        return node != null;
    }

    private bool isWordAlreadyHighlighted(HighlightedWord highlightedWord)
    {
        bool isHighlighted = false;
        foreach (HighlightedWord word in highlightedWordList)
        {
            if(word.Equals(highlightedWord))isHighlighted = true;
        }

        return isHighlighted;
    }
}

public class HighlightedWord
{
    string word;    
    LetterGridObject startNode;
    LetterGridObject endNode;
    private string highlightedAnswer;

    public HighlightedWord(string highlightedAnswer, LetterGridObject startNode, LetterGridObject endNode)
    {
        this.highlightedAnswer = highlightedAnswer;
        this.startNode = startNode;
        this.endNode = endNode;
    }

    public bool Equals(HighlightedWord highlightedWord)
    {
        return (startNode == highlightedWord.startNode && endNode == highlightedWord.endNode) || (endNode == highlightedWord.startNode && startNode == highlightedWord.endNode);
    }
}
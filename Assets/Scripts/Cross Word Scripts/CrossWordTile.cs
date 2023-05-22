using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class CrossWordTile : MonoBehaviour
{
    public CrossWordObject crossWordObject;
    bool hasLetterBelow;

    [Header("UI Components")]
    [SerializeField] TextMeshPro numberText;
    [SerializeField] TextMeshPro letterText;
    [SerializeField] TMP_InputField inputField;

    [Header("Sprite Components")]
    [SerializeField] Transform selectSpriteTransform;
    [SerializeField] Sprite[] crossWordTileSprites;
    [SerializeField] Sprite[] crossWordTileHighlightSprites;
    Sprite baseSprite;

    // Start is called before the first frame update
    void Start()
    {
        CrossWordManager.instance.onNodeSelected += HighlightCrossWordTile;
        inputField.onValueChanged.AddListener(HandleInputValueChanged);
        inputField.onValidateInput += RestrictSpecialChar;
    }

    void Update()
    {
        if(crossWordObject.isSelected && !inputField.isFocused) inputField.Select();
    }

    public void SpawnCrossWordTile(CrossWordObject node, float cellSize)
    {
        if(node != null)
        {
            //ADJUST FONT SIZE DEPENDING ON CELL SIZE
            letterText.fontSize = cellSize * 6f;
            numberText.fontSize = cellSize * 2f;

            //ADJUST SCALE DEPENDING ON THE CELLSIZE
            transform.localScale = new Vector3(cellSize, cellSize, 1);

            //ADD UI TEXT DESIGNATED CHARACTERS/STRINGS
            numberText.text = "";

            foreach(KeyValuePair<Orientation, CrossWordClue> clue in node.crossWordClues)
            {
                if(node.x == clue.Value.startNode.x && node.y == clue.Value.startNode.y)
                {
                    numberText.text = clue.Value.itemNumber.ToString();
                    break;
                }
            }

            letterText.text = '\0'.ToString();

            //CHANGE TILE IMAGE DEPENDING ON NODE'S NEIGHBOUR
            Grid<CrossWordObject> grid = CrossWordGridManager.instance.GetGrid();
            SpriteRenderer sr = GetComponent<SpriteRenderer>();

            hasLetterBelow = node.y > 0 && grid.GetGridObject(node.x, node.y - 1).letter != '\0';

            if(hasLetterBelow)
            {
                sr.sprite = crossWordTileSprites[0];
            }
            else
            {
                sr.sprite = crossWordTileSprites[1];
            }

            baseSprite = sr.sprite;
            crossWordObject = node;

            inputField.interactable = false;
        }
    }

    private void OnMouseDown()
    {
        //CAN ONLY SELECT TILES IF THERE ARE NO UI ELEMENT ACTIVE AND THE TILE IS NOT YET ANSWERED
        if(CrossWordManager.instance.CanSelectTiles() && !crossWordObject.isAnswered)
        {
            //GET NEIGHBOUR TILES AND HIGHLIGHT IT
            inputField.interactable = true;
            CrossWordManager.instance.SelectCrossWordObject(crossWordObject);
        }

    }

    public void HighlightCrossWordTile()
    {
        if(crossWordObject == null)
        {
            Debug.Log("CrossWord Object Does not exist");
            return;
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        inputField.interactable = crossWordObject.isSelected;
        if(crossWordObject.isSelected && !inputField.isFocused) inputField.Select();
        
        selectSpriteTransform.gameObject.SetActive(crossWordObject.isSelected);
        if(crossWordObject.isHighlighted)
        {
            sr.sprite = hasLetterBelow? crossWordTileHighlightSprites[0] : crossWordTileHighlightSprites[1];    
        }
        else
        {
            sr.sprite = baseSprite;
        }
    }

    //EVENT HANDLERS
    private void HandleInputValueChanged(string newValue)
    {
        //CHANGE EXISTING TEXT IF THERE ARE ANY
        if(newValue.Length > 1) inputField.text = newValue[1].ToString();
        
        //CHANGE INPUTTED LETTER FIELD AND CHECK CURRENT ORIENTATION
        crossWordObject.inputtedLetter = inputField.text == "" ? '\0' : inputField.text.ToLower()[0];
        CrossWordClue activeClue = CrossWordManager.instance.GetActiveCLue();

        //REVERSE DIRECTION AS DEFAULT DIRECTION
        Vector2Int direction = activeClue.orientation == Orientation.across? Vector2Int.left : Vector2Int.up;        

        if(newValue != "")
        {
            //CHECK IF ANSWER IS CORRECT AND CHANGE DIRECTION
            CrossWordManager.instance.CheckForCorrectAnswers(crossWordObject);
            direction = activeClue.orientation == Orientation.across? Vector2Int.right : Vector2Int.down;
        }

        MoveToNextTile(crossWordObject.x, crossWordObject.y, direction, activeClue.orientation);
    }

    private CrossWordObject MoveToNextTile(int x, int y, Vector2Int direction, Orientation orientation)
    {
        //UPDATE TILE LOCATION DEPENDING ON DIRECTION
        x += direction.x;
        y += direction.y;

        CrossWordObject nextNode = CrossWordGridManager.instance.GetGrid().GetGridObject(x, y);
        
        if(nextNode == null || nextNode.letter == '\0') //NODE IS INVALID, RETURN NULL
        {
            return nextNode;
        }
        else if(nextNode.isAnswered) //RE-RUN THIS FUNCTION WITH NEW COORDINATES
        {
            MoveToNextTile(x, y, direction, orientation);
        }
        else
        {
            crossWordObject.isSelected = false;
            CrossWordManager.instance.SelectCrossWordObject(nextNode, orientation);
        }

        return nextNode;
    }

    private char RestrictSpecialChar(string text, int charIndex, char addedChar)
    {
        //IF INPUTTED IS NOT A LETTER
        if(!char.IsLetter(addedChar))
        {
            return '\0';
        }

        return addedChar;
    }
}

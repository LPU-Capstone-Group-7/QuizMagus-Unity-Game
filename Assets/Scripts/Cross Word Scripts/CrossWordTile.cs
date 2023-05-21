using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

/*
    TODO
    HIGHLIGHT SELECTED CROSSWORD ITEM AND ALSO RELATED TILES -- DONE
    WINDOW THAT SHOWS ALL CLUES ACROSS AND DOWN -- DONE
    DISPLAY WINDOW FOR SHOWING CLUES ON THAT SPECIFIC TILES -- DONE
    INPUT FIELD FOR ANSWERING TILES -- DONE
    INPUT FIELD HIGHLIGHT SELECTED TILES -- DONE
    DISABLE SELECT WHEN CLUE LIST IS ACTIVE -- DONE
    ONLY ALLOW A-Z CHAR WHEN TYPING LETTERS -- DONE
    MOVE BACKWARDS WHEN BACKSPACE -- DONE
    CHECK WHEN CROSSWORD ENTRY IS FINALLY CORRECT AND ALSO INCLUDE INDEX NUMBER FOR COMPARISON
    TIMER
    FUNCTION FOR CALCULATING THE TIME IT TOOK TO FIND THE CORRECT ANSWER
    FUNCTION THAT UPDATES THE TIME IT TOOK TO FIND THE CORRECT ANSWER EVERYTIME NODE IS UPDATED (DO THIS ONLY WHEN NO NODES ARE EMPTY)
    FUNCTION FOR CHECKING ALL THE CROSSWORD GRID ITEM IF THEY ARE CORRECT
    CREATE CLUE IS ANSWERED EVENT HANDLER
    STRIKETHROUGH CLUES IN CLUES LIST WHEN IT IS ANSWERED ALREADY
    CREATE ART THAT SHOWS THAT TILE IS ALREADY ANSWERED
    DO NOT HIGHLIGHT OR SELECT TILES THAT ARE ALREADY ANSWERED
    SKIP TILES WHEN IT IS ALREADY ANSWERED
    END GAME WHEN ALL CLUES ARE ANSWERED
    BUTTON THAT ALLOWS PLAYER TO END THE GAME
    FORCE END GAME WHEN TIMER ELAPSED
    FUNCTION FOR CALCULATING SCORE
    RANDOMIZE TRIVIA QUESTION
    STARTING HINT FOR SHOWING LETTERS BASED ON THE UNIQUE SETTINGS
    DO NOT REFRESH CLUE TICKER WHEN MOVING TO A NEW TILE IF THE CROSSWORD CLUE ARE THE SAME
    UPLOADING CROSSWORD GAME RESULT TO FIREBASE
*/
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
        if(CrossWordManager.instance.CanSelectTiles())
        {
            inputField.interactable = true;

            //GET NEIGHBOUR TILES AND HIGHLIGHT IT
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

        //CHECK IF ANSWER IS CORRECT
        CrossWordManager.instance.CheckForCorrectAnswers(crossWordObject);

        int x = crossWordObject.x;
        int y = crossWordObject.y;

        if(crossWordObject.crossWordClues.ContainsKey(activeClue.orientation))
        {
            //MOVE TO NEXT TILE AND SELECT THAT TILE
            if(activeClue.orientation == Orientation.across)
            {
                x = newValue != "" ? x + 1 : x - 1;
            }
            else
            {
                y = newValue != "" ? y - 1 : y + 1;
            }
        }

        MoveToNextTile(x, y, activeClue.orientation);
    }

    private CrossWordObject MoveToNextTile(int x, int y, Orientation orientation)
    {
        CrossWordObject nextNode = CrossWordGridManager.instance.GetGrid().GetGridObject(x, y);

        if(nextNode != null && nextNode.letter != '\0')
        {
            crossWordObject.isSelected = false;
            CrossWordManager.instance.SelectCrossWordObject(nextNode, orientation);
        }

        return nextNode;
    }

    private char RestrictSpecialChar(string text, int charIndex, char addedChar)
    {
        Debug.Log("text: " + text + "CharIndex: " + charIndex);
        if(addedChar == '\0')
        {
            CrossWordClue activeClue = CrossWordManager.instance.GetActiveCLue();

            if(crossWordObject.crossWordClues.ContainsKey(activeClue.orientation))
            {
                //CHECK IF ALL TILE RELATED TO THIS HAS A VISIBLE TILES

                //MOVE TO NEXT TILE AND SELECT THAT TILE
                int x = activeClue.orientation == Orientation.across? crossWordObject.x - 1 : crossWordObject.x;
                int y = activeClue.orientation == Orientation.down? crossWordObject.y + 1 : crossWordObject.y;

                CrossWordObject nextNode = MoveToNextTile(x, y, activeClue.orientation);
            }

            Debug.Log("This is running");

            return '\0';
        }

        //IF INPUTTED IS NOT A LETTER
        if(!char.IsLetter(addedChar))
        {
            return '\0';
        }

        return addedChar;
    }
}

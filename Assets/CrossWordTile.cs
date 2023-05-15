using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class CrossWordTile : MonoBehaviour
{
    public CrossWordObject crossWordObject;

    [Header("UI Components")]
    [SerializeField] TextMeshPro numberText;
    [SerializeField] TextMeshPro letterText;

    [Header("Sprite Components")]
    [SerializeField] Sprite[] crossWordTileSprites;

    // Start is called before the first frame update
    void Start()
    {
        
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

            foreach(CrossWordClue clue in node.crossWordClues)
            {
                if(node.x == clue.startNode.x && node.y == clue.startNode.y)
                {
                    numberText.text = clue.itemNumber.ToString();
                    break;
                }
            }

            letterText.text = node.letter.ToString();

            //CHANGE TILE IMAGE DEPENDING ON NODE'S NEIGHBOUR
            Grid<CrossWordObject> grid = CrossWordGridManager.instance.GetGrid();
            SpriteRenderer sr = GetComponent<SpriteRenderer>();

            bool hasLetterBelow = node.y > 0 && grid.GetGridObject(node.x, node.y - 1).letter != '\0';

            if(!hasLetterBelow)
            {
                sr.sprite = crossWordTileSprites[1];
            }
            else
            {
                sr.sprite = crossWordTileSprites[0];
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}

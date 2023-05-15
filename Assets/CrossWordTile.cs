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

    // Start is called before the first frame update
    void Start()
    {
        if(crossWordObject != null)
        {
            numberText.text = "";

            foreach(CrossWordClue clue in crossWordObject.crossWordClues)
            {
                if(crossWordObject.x == clue.startNode.x && crossWordObject.y == clue.startNode.y)
                {
                    numberText.text = clue.itemNumber.ToString();
                    break;
                }
            }

            letterText.text = crossWordObject.letter.ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}

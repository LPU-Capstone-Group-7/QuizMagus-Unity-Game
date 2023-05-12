using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class CrossWordLetter : MonoBehaviour
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
            if(crossWordObject.isStartingLetter) 
            {
                foreach (CrossWordClue clue in crossWordObject.crossWordClues)
                {
                    if(crossWordObject.letter.ToString().ToLower() == clue.triviaQuestion.answer[0].ToString().ToLower())
                    {
                        numberText.text = clue.itemNumber.ToString();
                        break;
                    }
                }
            }
            else
            {
                numberText.text = "";
            }

            letterText.text = crossWordObject.letter.ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}

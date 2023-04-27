using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    private TextMeshProUGUI hintText;
    private string currentAnswer;

    [Header("Text Color")]
    [SerializeField] private Color correctAnswer;
    [SerializeField] private Color incorrectAnswer;
    private Color baseColor;

    private void Awake() 
    {
        hintText = GetComponent<TextMeshProUGUI>();    
    }

    private void Start()
    {
        baseColor = hintText.color;
    }

    public void HideAnswer(string answer) //HIDES ANSWER
    {
        currentAnswer = answer;
        foreach (char letter in answer)
        {
            if(letter != ' ')
            {
                answer = answer.Replace(letter,'_');
            }
        }
        if(hintText == null){Debug.Log("hint text is missing");}
        hintText.text = answer;
    }
        /*  === HINT LOGIC EXPLAINED ===
            CURRENT WORD: YELLOW        CURRENT HINT: Y___O_
            COUNT THE AMOUNT OF LETTERS NEED TO SHOW BASED ON THE DESIRED PERCENTAGE -----> TOTAL LETTER / PERCENTAGE = 6 * .5 =  3
            CHECK THE NUMBER OF LETTERS THAT ARE ALREADY SHOWING -----> LETTERS NEEDED - CURRENT LETTERS = REMAINING LETTERS TO SHOW
        */
    public void ShowPercentageOfLetters(float percentage)
    {
        int amountToShow = Mathf.CeilToInt(currentAnswer.ToCharArray().Length * (percentage/100f));//COMPUTE HOW MANY LETTERS TO SHOW FOR EACH PERCENTAGE
        int count = 0;
        foreach (char letter in hintText.text.ToCharArray()) 
        {
            if(letter != ' ' && letter != '_'){ count++; } //COUNT HOW MANY LETTERS ARE SHOWING
        }
        amountToShow -= count;
        if(amountToShow > 0 && hintText.text.Contains("_")) //CHECKS IF THERE ARE STILL LETTERS THAT ARE HIDDEN
        {
            ShowLetters(amountToShow);
        }
    }

    public void ShowLetters(int amountToShow)
    {
        char[] hintArray = hintText.text.ToCharArray();
        for (int i = 0; i < amountToShow; i++) //ADD THE REMAINING LETTERS NEEDED TO SHOW
        {
            bool hasShownLetter = false;
            do
            {
                int randomIndex = Random.Range(0,hintArray.Length);
                if(hintArray[randomIndex] == '_') //CHECKS IF THAT RANDOM INDEX HAS NOT YET SHOWN ITS LETTER
                {
                    hintArray[randomIndex] = currentAnswer.ToCharArray()[randomIndex]; //REPLACE IT WITH THE LETTER FROM THE CURRENT ANSWER
                    hasShownLetter = true;
                }
            } while (!hasShownLetter);
        }

        hintText.text = hintArray.ArrayToString();
    }

    public void ShowAnswer(string answerResult = "")  //SHOW COMPLETE ANSWER
    {
        if(answerResult == "incorrect"){
            hintText.color = incorrectAnswer;
        }

        hintText.text = currentAnswer;
    }

    public void EnableHintText( bool state)
    {
        hintText.gameObject.SetActive(state);
        hintText.color = baseColor;
    }

  
}

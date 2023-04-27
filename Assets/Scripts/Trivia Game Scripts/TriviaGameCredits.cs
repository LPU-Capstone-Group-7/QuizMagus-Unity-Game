using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class TriviaGameCredits : MonoBehaviour
{
    [SerializeField] private float showDelay;
    
    [Header("Trivia Game Result Texts")]
    [SerializeField] private Transform accuracyText;
    [SerializeField] private Transform timeTakenText;
    [SerializeField] private Transform averageTimeText;
    [SerializeField] private Transform overallGradeText;

    private void Awake()
    {
        AudioManager.instance.StopPlaying("BGM");    
    }
    void Start()
    {
        AudioManager.instance.Play("Chain Rattle");

        TriviaGameResultManager.instance.TriggerSendTriviaGameResult();
        GenerateScoreboardValues();
        
        //SET EACH TRANSFORM TO INACTIVE TO HIDE THEM
        accuracyText.gameObject.SetActive(false);
        timeTakenText.gameObject.SetActive(false);
        averageTimeText.gameObject.SetActive(false);
        overallGradeText.gameObject.SetActive(false);

        //SEQUENTIALLY SHOW EACH TEXT FIELDS AFTER A SPECIFIC AMOUNT OF TIME
        FunctionTimer.Create(() => {
            FunctionTimer.Create(() => ShowResultText(accuracyText.gameObject), showDelay);
            FunctionTimer.Create(() => ShowResultText(timeTakenText.gameObject), showDelay * 2);
            FunctionTimer.Create(() => ShowResultText(averageTimeText.gameObject), showDelay * 3);
            FunctionTimer.Create(() => ShowResultText(overallGradeText.gameObject), showDelay * 4.5f);
        },showDelay);
    }

    void ShowResultText(GameObject textObject)
    {
        textObject.SetActive(true);
        AudioManager.instance.Play("Thud");
    }

    void GenerateScoreboardValues(){
        //TOTAL SCORE OF ASSESSMENT IN PERCENTAGE
        accuracyText.Find("Field Value").GetComponent<TextMeshProUGUI>().text = TriviaGameResultManager.instance.GetScorePercentage().ToString() + "%";

        //TOTAL TIME TAKEN
        int totalSeconds = (int)TriviaGameResultManager.instance.GetTotalTimeTaken();
        int minutes = totalSeconds/60;
        int seconds = totalSeconds%60;
        timeTakenText.Find("Field Value").GetComponent<TextMeshProUGUI>().text = minutes.ToString() + " m " + seconds.ToString() + " s";
        
        //AVEREAGE TIME PER QUESTIONS
        averageTimeText.Find("Field Value").GetComponent<TextMeshProUGUI>().text = TriviaGameResultManager.instance.GetAverageTimePerQuestions().ToString() + " s";

        //OVERALL GRADESW
        overallGradeText.Find("Field Value").GetComponent<TextMeshProUGUI>().text = TriviaGameResultManager.instance.GetTotalGrade().ToString() + "%";
    }
}

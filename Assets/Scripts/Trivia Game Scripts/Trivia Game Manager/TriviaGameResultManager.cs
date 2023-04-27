using UnityEngine;
using System.Runtime.InteropServices;
using TMPro;

public class TriviaGameResultManager : MonoBehaviour
{
    private TriviaGameResults triviaGameResults;
    public static TriviaGameResultManager instance;
    [DllImport("__Internal")] private static extern void SendTriviaGameResult (string triviaGameResultJSON);

    private void Awake() {

        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public void TriggerSendTriviaGameResult()
    {
        string jsonResult = JsonUtility.ToJson(triviaGameResults);

        //ONLY RETURNS THE TRIVIA GAME RESULT IF ITS HIGHER THAN THE OLD RESULT
        #if UNITY_WEBGL == true && UNITY_EDITOR == false
                SendTriviaGameResult(jsonResult);
        #endif
    }

    public void SetTriviaGameResults(TriviaData[] triviaDatas, string basedGrading){

        float totalTimeTaken = 0;
        for (int i = 0; i < triviaDatas.Length; i++){
            totalTimeTaken += triviaDatas[i].timeToAnswer;
        }

        float totalGrade = ComputeTotalGrade(getTotalScore(triviaDatas), triviaDatas.Length,basedGrading);
        triviaGameResults = new TriviaGameResults(triviaDatas, Mathf.Round(totalGrade * 100f) / 100f, totalTimeTaken);
    }

    //GET PERCENTAGE OF CORRECT ANSWERS OF USER
    public float GetScorePercentage(){
       
        TriviaData[] triviaDatas = triviaGameResults.triviaDatas;
        return Mathf.Round((((float)getTotalScore(triviaDatas)/triviaDatas.Length) * 100));
    }

    public float ComputeTotalGrade(int currentScore, int totalScore, string basedGrading)
    {
        switch (basedGrading)
        {
            case "based 60": return ((float)currentScore/totalScore) * 60 + 40;
            case "based 50": return ((float)currentScore/totalScore) * 50 + 50;
            default: return 0f;
        }

    }

    public int getTotalScore(TriviaData[] triviaDatas)
    {
        int correctAnswers = 0;
        //GET NUMBER OF CORRECT ANSWERS
        for (int i = 0; i < triviaDatas.Length; i++)
        {
            if(triviaDatas[i].correct){
                correctAnswers++;
            }
        }

        return correctAnswers;
    }

    //GET TOTAL TIME TAKEN
    public float GetTotalTimeTaken(){

        return triviaGameResults.totalTimeTaken;
    }

    public float GetTotalGrade()
    {
        return triviaGameResults.totalGrade;
    }

    //GET AVERAGE TIME OF TIME PER QUESTIONS FOR EACH ITEMS
    public float GetAverageTimePerQuestions(){

        return (float)System.Math.Round((decimal)GetTotalTimeTaken()/triviaGameResults.triviaDatas.Length, 1);
    }
}
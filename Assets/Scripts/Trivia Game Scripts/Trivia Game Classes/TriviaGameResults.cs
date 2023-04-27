using UnityEngine;

[System.Serializable]
public class TriviaGameResults
{
    public TriviaData[] triviaDatas;
    public float totalGrade;
    public float totalTimeTaken;

    public TriviaGameResults(TriviaData[] triviaDatas, float gradePercentage, float totalTimeTaken){
        this.triviaDatas = triviaDatas;
        this.totalGrade = gradePercentage;
        this.totalTimeTaken = totalTimeTaken;
    }

}

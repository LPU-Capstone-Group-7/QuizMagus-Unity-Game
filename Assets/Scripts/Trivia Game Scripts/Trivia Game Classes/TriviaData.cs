[System.Serializable]
public class TriviaData
{
    public string triviaQuestionId;
    public float timeToAnswer;
    public bool correct;
    public string question;
    public string correctAnswer;
    public string answer;

    public TriviaData(string triviaQuestionId, float timeToAnswer, bool correct, string question, string correctAnswer, string answer){
        this.triviaQuestionId = triviaQuestionId;
        this.timeToAnswer = timeToAnswer;
        this.correct = correct;
        this.question = question;
        this.correctAnswer = correctAnswer;
        this.answer = answer;
    }
}

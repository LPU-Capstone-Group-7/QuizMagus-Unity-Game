using UnityEngine;
using TMPro;
using System;
public class WordSearchTimer : MonoBehaviour
{
    public static WordSearchTimer instance;
    private bool isTimeRunning = false;

    [Header("Word Search Timer UI")]
    [SerializeField] private TextMeshProUGUI timerUI;

    [Header("Word Search Timer Counters")]
    private float totalSeconds;
    private float timeTakenToAnswer;
    private bool trackTimeTakenToAnswer = true;

    public Action onTimeFinished;
    public Action onTimeChanged;

    private void Awake()
    {
        if(instance == null) { instance = this; }    
    }

    private void Start()
    {
        //RESET TIMER EVERYTIME A NEW QUESTION IS LOADED
        WordSearchManager.instance.onQuestionLoads += () => {        
            ResetTimeTakenToAnswer();
            CountTimeTakenToAnswer(true);
        };    
    }

    private void Update()
    {
        if(isTimeRunning)
        {
            totalSeconds -= Time.deltaTime;
            DisplayWordSearchCurrentTime(totalSeconds);
            onTimeChanged?.Invoke();
        }

        if(totalSeconds <= 0 && isTimeRunning)
        {
            onTimeFinished?.Invoke();
            isTimeRunning = false;
        }

        if(trackTimeTakenToAnswer)
        {
            timeTakenToAnswer += Time.deltaTime;
        }
    }
    
    public void StartWordSearchTimer(int startingMinute)
    {
        totalSeconds = startingMinute * 60;
        isTimeRunning = true;
    }

    public void DisplayWordSearchCurrentTime(float totalSeconds)
    {
        //GET MINUTES AND SECONDS FROM THE REMAINING TIME IN SECONDS
        int minutes = (int)totalSeconds / 60;
        int seconds = (int)totalSeconds % 60;

        // DISPLAY TIME IN THIS FORMAT MM : SS
        timerUI.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void CountTimeTakenToAnswer(bool state)
    {
        trackTimeTakenToAnswer = state;
    }

    public float GetTimeTakenToAnswer()
    {
        float roundedValue = Mathf.Round(timeTakenToAnswer * 100.0f) * 0.01f;
        return roundedValue;
    }

    public void ResetTimeTakenToAnswer()
    {
        timeTakenToAnswer = 0f;
    }
}

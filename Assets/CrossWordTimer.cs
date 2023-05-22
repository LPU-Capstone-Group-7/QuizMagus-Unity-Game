using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class CrossWordTimer : MonoBehaviour
{
    public static CrossWordTimer instance;

    [Header("Timer Settings")]
    private float totalSeconds;
    private float timeTakenToAnswer;
    private bool isTimeRunning = false;
    private bool trackTimeTakenToAnswer = true;

    [Header("Timer UI Components")]
    [SerializeField] TextMeshProUGUI timerUI;

    public Action onTimeFinished;
    public Action onTimeChanged;
    
    private void Awake()
    {
        if(instance == null) instance = this;    
    }
    private void Update() 
    {
        if(isTimeRunning)
        {
            totalSeconds -= Time.deltaTime;
            timerUI.text = TimerUtils.DisplayCurrentTime(totalSeconds);
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

    public void StartTimer(int startingMinutes)
    {
        totalSeconds = startingMinutes * 60;
        isTimeRunning = true;
    }

    public float GetTimeTakenToAnswer()
    {
        float roundedValue = Mathf.Round(timeTakenToAnswer * 100.0f) * 0.01f;
        return roundedValue;
    }

}

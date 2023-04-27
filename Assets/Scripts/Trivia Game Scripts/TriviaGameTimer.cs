using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class TriviaGameTimer : MonoBehaviour
{
    private Slider slider;
    private float timePerQuestion;
    private float currentTime;
    private bool pauseTimerUI = true;

    public Action OnTimeStart;
    public Action OnTimeChange;
    public Action OnTimeFinish;

    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!pauseTimerUI)
        {
           UpdateSlider();

           if(currentTime <= 0)
           {
                OnTimeFinish?.Invoke();
                PauseTimerUI(true);
           }
        }

    }

    public void UpdateSlider()
    {
        OnTimeChange();
        currentTime -= Time.deltaTime;
        float sliderVale = currentTime / timePerQuestion;
        slider.value = sliderVale;
    }

    public void IncreaseTimerValue(float timeToIncrease)
    {
        currentTime += timeToIncrease;
    }

    public float GetCurrentTimePercentage()
    {
        return (currentTime / timePerQuestion) * 100f;
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }

    public float GetTimePerQuestion()
    {
        return timePerQuestion;
    }


    public void StartTimer(float timePerQuestion)
    {
        this.timePerQuestion = timePerQuestion;
        currentTime = timePerQuestion;
        pauseTimerUI = false;
    }

    public void SetTimer(float timePerQuestion)
    {
        this.timePerQuestion = timePerQuestion;
    }

    public void PauseTimerUI(bool state)
    {
        pauseTimerUI = state;
    }

    public bool IsTimerUIPaused()
    {
        return pauseTimerUI;
    }

    public void ResetTimer()
    {
        currentTime = timePerQuestion;
        Debug.Log("Timer resetted");
        OnTimeStart?.Invoke();
    }
}

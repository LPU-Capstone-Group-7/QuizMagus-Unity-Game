using System.Collections.Generic;
using UnityEngine;
using System;

public class FunctionTimer
{
    private static List<FunctionTimer> activeTimerList;
    private static GameObject initGameObject;
    private static void InitIfNeeded()
    {
        if (initGameObject == null)
        {
            initGameObject = new GameObject("FunctionTimer_InitGameObject");
            activeTimerList = new List<FunctionTimer>();
        }
    }
    public static FunctionTimer Create(Action action, float timer, string timerName = null)
    {
        InitIfNeeded();
        GameObject obj = new GameObject("Function Timer", typeof(MonoBehaviourHook));

        FunctionTimer functionTimer = new FunctionTimer(action, timer,timerName, obj);
     
        obj.GetComponent<MonoBehaviourHook>().onUpdate = functionTimer.Update;

        activeTimerList.Add(functionTimer);

        return functionTimer;
    }

    private static void RemoveTimer(FunctionTimer functionTimer)
    {
        InitIfNeeded();
        activeTimerList.Remove(functionTimer);
    }

    private static void StopTimer(string timerName)
    {
        for (int i = 0; i < activeTimerList.Count; i++)
        {
            if (activeTimerList[i].timerName == timerName)
            {
                //STOP THIS TIMER
                activeTimerList[i].DestroySelf();
                i--;
            }
        }
    }

    public class MonoBehaviourHook : MonoBehaviour //CLASS THAT HAVE ACCESS TO MONOBEHAVIOUR
    {
        public Action onUpdate;
        private void Update()
        {
            if (onUpdate != null) { onUpdate(); }
        }
    }

    private Action action;
    private float timer;
    private string timerName;
    private GameObject obj;
    private bool isDestroyed;
    private FunctionTimer(Action action, float timer, string timerName, GameObject obj)
    {
        this.action = action;
        this.timer = timer;
        this.timerName = timerName;
        this.obj = obj;
        this.isDestroyed = false;
    }

    public void Update()
    {
        if (!isDestroyed)
        {
            timer -= Time.deltaTime;

            if (timer < 0)
            {
                action();
                DestroySelf();
            }
        }
    }

    private void DestroySelf()
    {
        isDestroyed = true;
        UnityEngine.Object.Destroy(obj);
        RemoveTimer(this);
    }
}
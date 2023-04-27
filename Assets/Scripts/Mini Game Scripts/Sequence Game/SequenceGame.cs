using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
public class SequenceGame : MonoBehaviour
{
    public static SequenceGame instance;

    [Header("Sequence Component")]
    [SerializeField] private Transform[] sequenceButtonArray;
    [SerializeField] private int numberOfSequence;
    private Queue<int> sequenceQueue;
    private Queue<int> inputtedSequenceQueue;
    private bool isGeneratingSequence = false;

    [Header("Sequence Game UI Component")]
    [SerializeField] private TextMeshProUGUI sequenceLevelText;

    [Header("Sequence Game Messages")]
    [SerializeField] private string[] levelCompletedMessages;
    [SerializeField] private string[] gameOverMessages;


    private void Awake() 
    {
        if(instance == null)
        {
            instance = this; 
        }
        else
        {
            Destroy(this.gameObject);
        }    
    }

    private void Start()
    {
        //SETS MAIN CAMERA INTO CANVAS WORLD SPACE
        GetComponentInChildren<Canvas>().worldCamera = Camera.main;

        MiniGameManager.instance.onStartPlaying += StartSequenceGame;
    }

    void StartSequenceGame()
    {
        sequenceQueue = new Queue<int>();
        inputtedSequenceQueue = new Queue<int>();

        for (int i = 0; i < sequenceButtonArray.Length; i++)
        {
            int index = i;
            sequenceButtonArray[i].GetComponentInChildren<SequenceButton>().onButtonClicked += () => { ValidateClickSequence(index);};
        }
        StartCoroutine(GenerateSequence());
    }
    IEnumerator GenerateSequence()
    {
        isGeneratingSequence = true;
        inputtedSequenceQueue.Clear();

        //ENQUEUE SEQUENCE DEPENDING ON THE NUMBER OF SEQUENCE
        for (int i = 0; i < numberOfSequence; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0,sequenceButtonArray.Length);
            sequenceQueue.Enqueue(randomIndex);
        }
        
        //DO SEQUENCE GLOW ANIMATION
        int[] currentSequence = sequenceQueue.ToArray();

        for (int i = 0; i < currentSequence.Length; i++)
        {
            sequenceButtonArray[currentSequence[i]].GetComponentInChildren<SequenceButton>().PlayButtonGlowAnimation();
            AudioManager.instance.Play("Sequence " + currentSequence[i].ToString());
            Debug.Log(currentSequence[i]);
            yield return new WaitForSeconds(1.5f);
        }

        //SHOWING SEQUENCE IS FINISH
        isGeneratingSequence = false;
        sequenceLevelText.text = "Go";

    }

    void ValidateClickSequence(int buttonIndex)
    {
        inputtedSequenceQueue.Enqueue(buttonIndex);
        int[] expectedSequence = new int[inputtedSequenceQueue.Count];
        Array.Copy(sequenceQueue.ToArray(), expectedSequence, inputtedSequenceQueue.Count);

        if(inputtedSequenceQueue.ToArray().SequenceEqual(expectedSequence))
        {
            sequenceButtonArray[buttonIndex].GetComponentInChildren<SequenceButton>().PlayButtonGlowAnimation();
            AudioManager.instance.Play("Sequence " + buttonIndex.ToString());

            sequenceLevelText.text = levelCompletedMessages[UnityEngine.Random.Range(0, levelCompletedMessages.Length)];

            if(inputtedSequenceQueue.Count == numberOfSequence)
            {
                //GAME IS FINISH CONGRATULATIONS AND RESUME QUIZ GAME AND INCREASE TIMER
                EndMiniGame();
                MiniGameManager.instance.IncreasePlayingTime();
            }
            

        }
        else //INCORRECT SEQUENCE DO CLEAN UP
        {
            sequenceLevelText.text = gameOverMessages[UnityEngine.Random.Range(0, gameOverMessages.Length)];
            AudioManager.instance.Play("Thud");
            FunctionTimer.Create(EndMiniGame, 0.5f);
        }

    }

    void EndMiniGame()
    {
        MiniGameManager.instance.StartExitMiniGameCoroutine();
    }

    public bool GetIsGeneratingSequence()
    {
        return isGeneratingSequence;
    }
    string DebugArray(int[] array)
    {
        string arrayString = string.Join(",", array);
        return string.Format("[{0}]", arrayString);
    }

    private void OnDestroy()
    {
        MiniGameManager.instance.onStartPlaying -= StartSequenceGame;    
    }

}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager instance;
    [SerializeField] private GameObject[] miniGameArray;
    private Transform currentMiniGame;
    private Animator animator;
    bool startPlaying = false;
    int levelPlayedMiniGame = 0;
    public Action onStartPlaying;
    public Action onMiniGameLoads;
    public Action onMiniGameUnloads;

    [Header("Mini Game Book Components")]
    [SerializeField] private Transform miniGameBookPrefab;
    [SerializeField] private Transform miniGameBookSpawnPoint;
    private Transform activeMiniGameBookTransform;
    private int spawnRoll;
    private HashSet<int> spawnedMiniGame = new HashSet<int>();

    [Header("Timer Components")]
    [SerializeField] private float timePercentageBeforeSpawning;
    private TriviaGameTimer triviaGameTimer;
    private TriviaGameManager triviaGameManager;


    private void Awake()
    {
        animator = GetComponent<Animator>();

        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        triviaGameTimer = GameObject.FindObjectOfType<TriviaGameTimer>();
        triviaGameManager = GameObject.FindObjectOfType<TriviaGameManager>();

        if(triviaGameManager != null) triviaGameManager.onLevelFinished += HideMiniGameBook;

        if(triviaGameTimer != null) triviaGameTimer.OnTimeChange += SpawnMiniGameBook;

        else{
            Debug.Log("Trivia Game Timer is missing");
        }
        
    }

    void SpawnMiniGameBook()
    {
        if(!startPlaying && triviaGameTimer.GetCurrentTimePercentage() <= timePercentageBeforeSpawning && activeMiniGameBookTransform == null && triviaGameManager.GetCurrentLevel() != levelPlayedMiniGame && !triviaGameManager.IsGameFinished())
        {
            activeMiniGameBookTransform = Instantiate(miniGameBookPrefab, miniGameBookSpawnPoint.transform.position, Quaternion.identity);
            levelPlayedMiniGame = triviaGameManager.GetCurrentLevel();
        }
    }

    void HideMiniGameBook()
    {
        if(activeMiniGameBookTransform != null)
        {
            activeMiniGameBookTransform.GetComponent<MiniGameBook>().ExitMiniGameBook();
        }
    }

    void RollSpawnChance()
    {
        int randomRoll = UnityEngine.Random.Range(0,2);
    }

    public void StartGenerateMiniGameCoroutine()
    {
        if(!startPlaying)
        {
            StartCoroutine(GenerateMiniGame());
            activeMiniGameBookTransform.GetComponent<MiniGameBook>().ExitMiniGameBook();
        }
    }

    IEnumerator GenerateMiniGame()
    {
        startPlaying = true;
        //PAUSE TRIVIA GAME
        onMiniGameLoads?.Invoke();

        //GET NEW RANDOM MINI GAME
        int randomIndex = 0;
        if (spawnedMiniGame.Count < miniGameArray.Length)
        {
            //GENERATE RANDOM INDEX THAT IS NOT INCLUDED IN SPAWNED MINI GAME HASHSET
            do
            {
                randomIndex = UnityEngine.Random.Range(0, miniGameArray.Length);
            }
            while (spawnedMiniGame.Contains(randomIndex));
        }
        else
        {
            spawnedMiniGame.Clear();
            randomIndex = UnityEngine.Random.Range(0, miniGameArray.Length);
        }

        spawnedMiniGame.Add(randomIndex);

        Debug.Log(randomIndex);
        currentMiniGame = Instantiate(miniGameArray[randomIndex], transform.GetChild(0).position, Quaternion.identity).transform;
        currentMiniGame.SetParent(transform.GetChild(0));
        animator.Play("MiniGame_Enter");

        yield return new WaitForSeconds(0.5f);

        onStartPlaying?.Invoke();

    }

    public void StartExitMiniGameCoroutine()
    {
            StartCoroutine(ExitMiniGame());
    }

    IEnumerator ExitMiniGame()
    {
        if(currentMiniGame != null)
        {
            animator.Play("MiniGame_Exit");
            Debug.Log("Exits Mini Game");
            yield return new WaitForSeconds(0.5f);

            startPlaying = false;
            Destroy(currentMiniGame.gameObject);
            onMiniGameUnloads?.Invoke();
        }

    }

    public void IncreasePlayingTime()
    {
        float timeToIncrease = triviaGameTimer.GetTimePerQuestion();
        GameObject.FindObjectOfType<TriviaGameTimer>().IncreaseTimerValue(timeToIncrease);
    }
}

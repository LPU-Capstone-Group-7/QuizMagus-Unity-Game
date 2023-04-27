using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FlappyGhost : MonoBehaviour
{
    public static FlappyGhost instance;
    [SerializeField] private Transform ghostTransform;

    [Header("Obstacle Commponent")]
    [SerializeField] private Transform obstacleSpawnPointTransform;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform obstaclePrefab;
    [SerializeField] private float obstacleSpeed;
    [SerializeField] private float startTimeBtwSpawn;
    [SerializeField] private float verticalOffset;
    private float timeBtwSpawn;
    private List<Transform> activeObstacleList;

    [Header("Game Handler")]
    private bool gameInProgress;
    private int score;
    [SerializeField] private int maxScore;

    [Header("UI Components")]
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI instructionText;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        activeObstacleList = new List<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!gameInProgress && Input.GetKeyDown(KeyCode.Space))
        {
            gameInProgress = true;
            scoreText.text = score.ToString() + "/" + maxScore.ToString();
            instructionText.text = "";
            ghostTransform.GetComponent<Rigidbody2D>().gravityScale = 1f;
        }

        if(gameInProgress)
        {
            if(timeBtwSpawn <= 0)
            {
                Transform obstacle = Instantiate(obstaclePrefab, RandomizeVerticalPosition(obstacleSpawnPointTransform.position, verticalOffset), Quaternion.identity, transform);
                obstacle.GetComponent<Obstacle>().InitializeMovement(targetTransform.position, obstacleSpeed);

                activeObstacleList.Add(obstacle);
                obstacle.GetComponent<Obstacle>().onDestroyAction += () => activeObstacleList.Remove(obstacle);

                timeBtwSpawn = startTimeBtwSpawn;
            }
            else
            {
                timeBtwSpawn -= Time.deltaTime;
            }
        }
    }

    public void IncreaseScore()
    {
         score ++;
        scoreText.text = score.ToString() + "/" + maxScore.ToString();

        if(isGameInProgress() && score >= maxScore) //GAME IS FINISHED
        {
            gameInProgress = false;
            ghostTransform.GetComponent<Rigidbody2D>().isKinematic = true;
            FunctionTimer.Create( () => {
                MiniGameManager.instance.StartExitMiniGameCoroutine();
                MiniGameManager.instance.IncreasePlayingTime();
            }, 0.5f);
        }
    }

    public void GameIsOver()
    {
        if(gameInProgress && score < maxScore)
        {
            Debug.Log("Game Over");
            gameInProgress = false;
            foreach (Transform obstacle in activeObstacleList)
            {
                obstacle.GetComponent<Obstacle>().InitializeMovement(targetTransform.position, 0);
            }

            AudioManager.instance.Play("Thud");
            FunctionTimer.Create(() => MiniGameManager.instance.StartExitMiniGameCoroutine(), 1f);
        }
  
    }

    Vector3 RandomizeVerticalPosition(Vector3 position, float verticalOffset)
    {
        float randomY = Random.Range(-verticalOffset, verticalOffset);
        return new Vector3(position.x, position.y + randomY);
    }

    public bool isGameInProgress()
    {
        return gameInProgress;
    }
}

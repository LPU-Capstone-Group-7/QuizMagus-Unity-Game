using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class MatchingGame : MonoBehaviour
{
    public static MatchingGame instance;
    [SerializeField] private Transform cardSpawnPoint;

    [Header("Card Components")]
    [SerializeField] private Transform cardPrefab;
    [SerializeField] private GameCardTypeSO[] gameCardTypeSOArray;
    [SerializeField] private Vector2 positionOffset;
    private List<GameCard> gameCardList;
    private GameCard activeGameCard;
    private int numOfPairsFound;

    [Header("Matching Game UI")]
    [SerializeField] private TextMeshProUGUI helperText;


    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    void Start()
    {
        SpawnCards();
        
        Canvas canvas = transform.GetComponentInChildren<Canvas>();
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            canvas.worldCamera = Camera.main;
        }
    }

    public void SpawnCards()
    {
        gameCardList = new List<GameCard>();

        List<Vector2> cardPositionList = new List<Vector2>();
        int numberOfPairs = gameCardTypeSOArray.Length;

        float cardWidth =  cardPrefab.GetComponent<SpriteRenderer>().bounds.size.x;
        float cardHeight = cardPrefab.GetComponent<SpriteRenderer>().bounds.size.y;

        //SPAWN THE CARDS BASED ON THE POSITION OFFSET AND ORIGIN POSITION
        for (int x = 0; x < numberOfPairs; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                Vector2 cardPosition = new Vector2(x * positionOffset.x, y * positionOffset.y);

                //SHIFTS THE X SO THAT THE CENTER WOULD BE THE ORIGIN, SAME ALSO FOR THE Y
                cardPosition = new Vector2(cardPosition.x - (numberOfPairs * positionOffset.x) / 2, cardPosition.y - positionOffset.y);

                //ADDS THE HEIGHT AND WIDTH OF THE CARD TO CENTER THE SHIT
                cardPosition = new Vector2(cardPosition.x + cardWidth, cardPosition.y + cardHeight/2);
                cardPositionList.Add(cardPosition);
            }
        }

        //SHUFFLE CARD POSITION LIST
        int n = cardPositionList.Count;
        while(n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, cardPositionList.Count);
            Vector2 position = cardPositionList[k];
            cardPositionList[k] = cardPositionList[n];
            cardPositionList[n] = position;
        }

        //SPAWN CARD PAIRS ON THE POSITIONS
        for (int i = 0; i < gameCardTypeSOArray.Length; i++)
        {

            //SPAWN 2 CARDS
            for (int j = 0; j < 2; j++)
            {
                if(cardPositionList.Count > 0)
                {
                    Vector3 cardSpawnPosition = new Vector3(cardPositionList[0].x + cardSpawnPoint.position.x, cardPositionList[0].y + cardSpawnPoint.position.y);
                    Transform cardTransform = Instantiate(cardPrefab, cardSpawnPosition, Quaternion.identity);
                    cardTransform.SetParent(transform);

                    //ASSIGN CARD TYPE SO
                    cardTransform.GetComponent<GameCard>().SetGameCardTypeSO(gameCardTypeSOArray[i]);

                    //REMOVE CARD POSITION FROM THE LIST
                    cardPositionList.Remove(cardPositionList[0]);

                    //ADD GAME CARD INSIDE THE LIST
                    gameCardList.Add(cardTransform.GetComponent<GameCard>());
                }    
            }
        }

        //PLAY CARD INTRO ANIMATIONS
        helperText.text = "";
        FunctionTimer.Create(() => FlipAllCards(gameCardList, CardState.FaceUp), 1f);
        FunctionTimer.Create(() => FlipAllCards(gameCardList, CardState.FaceDown), 3.5f);
        FunctionTimer.Create(() => {
            EnableCardsClickability(gameCardList, true);
            helperText.text = "go";
        }, 4f);
    }

    public void EnableCardsClickability(List<GameCard> gameCardList ,bool state)
    {
        foreach (GameCard gameCard in gameCardList)
        {
            gameCard.SetClickable(state);
        }
    }

    public void FlipAllCards(List<GameCard> gameCardList, CardState cardState)
    {
        Debug.Log(cardState);
        foreach (GameCard gameCard in gameCardList)
        {
            if(cardState == CardState.FaceUp) gameCard.PlayCardAnimation("Flip_To_Front");
            if(cardState == CardState.FaceDown) gameCard.PlayCardAnimation("Flip_To_Back");
        }
    }

    public void CompareCardTypes(GameCard gameCard)
    {
        EnableCardsClickability(gameCardList, false);
        if(activeGameCard)
        {
            //CARDS ARE THE SAME
            if(activeGameCard.GetGameCardTypeSO() == gameCard.GetGameCardTypeSO())
            {
                numOfPairsFound++;

                //GAME IS FINISHED
                if(numOfPairsFound == gameCardTypeSOArray.Length)
                {
                    helperText.text = "Good Job";
                    FunctionTimer.Create( () => {
                        MiniGameManager.instance.StartExitMiniGameCoroutine();
                        MiniGameManager.instance.IncreasePlayingTime();
                    }, 0.5f);

                }
                else
                {
                    FunctionTimer.Create(() => EnableCardsClickability(gameCardList, true), 0.25f);
                }
            }
            else
            {
                helperText.text = "Too Bad";
                FunctionTimer.Create( () => {
                    AudioManager.instance.Play("Thud");
                    CinemachineCameraShake.instance.ShakeCamera(0.25f, .5f);
                    FunctionTimer.Create(() => MiniGameManager.instance.StartExitMiniGameCoroutine(), 1f);

                },.5f);
                
            }

            activeGameCard = null;
        }
        else //THERE ARE NO ACTIVE GAME CARDS, SET THIS CARD AS THE ACTIVE ONE
        {
            activeGameCard = gameCard;
            FunctionTimer.Create(() => EnableCardsClickability(gameCardList, true), .25f);
        }

    }
}

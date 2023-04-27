using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum CardState
{
    FaceUp,
    FaceDown
}
public class GameCard : MonoBehaviour
{
    private GameCardTypeSO gameCardTypeSO;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Sprite cardBackSprite;
    private bool isCardFacedown = true;
    private bool isClickable = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        cardBackSprite = spriteRenderer.sprite;      
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            FlipCardToFront();
        }
    }
    public void SetGameCardTypeSO(GameCardTypeSO gameCardTypeSO)
    {
        this.gameCardTypeSO = gameCardTypeSO;
    }

    public GameCardTypeSO GetGameCardTypeSO()
    {
        return gameCardTypeSO;
    }

    void FlipCardToBack()
    {
        spriteRenderer.sprite = cardBackSprite;
        isCardFacedown = true;
        if(!AudioManager.instance.isPlaying("Card Flip")) AudioManager.instance.Play("Card Flip");
    }

    void FlipCardToFront()
    {
        spriteRenderer.sprite = gameCardTypeSO.cardFrontSprite;
        isCardFacedown = false;
        if(!AudioManager.instance.isPlaying("Card Flip")) AudioManager.instance.Play("Card Flip");

    }

    public void PlayCardAnimation(string stateName)
    {
        animator.Play(stateName);
    }

    public void SetClickable(bool state)
    {
        isClickable = state;
    }

    private void OnMouseDown()
    {
        if(isClickable && isCardFacedown)
        {
            //FLIP CARD
            animator.Play("Flip_To_Front");

            //COMPARE CARDS IF THERE ARE OTHER FACED UP CARDS....
            MatchingGame.instance.CompareCardTypes(this);
            
        }
    }

}

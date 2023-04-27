using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool jumpRequest = false;

    [Header("Jump Components")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float startTimeBtwJumps;
    private float timeBtwJumps;

    [Header("Game Physics")]
    [SerializeField] private float fallMultiplier;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(FlappyGhost.instance.isGameInProgress() && Input.GetKeyDown(KeyCode.Space) && !jumpRequest && timeBtwJumps <= 0)
        {
            jumpRequest = true;
            timeBtwJumps = startTimeBtwJumps;
        }
        else
        {
            timeBtwJumps -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(FlappyGhost.instance != null && FlappyGhost.instance.isGameInProgress())
        {
            if(other.CompareTag("Score")) //INCREASE SCORE
            {
                Debug.Log("SCORE !!!");
                FlappyGhost.instance.IncreaseScore();
            }

            else if(other.CompareTag("Obstacle"))
            {
                FlappyGhost.instance.GameIsOver();
            }
        }   
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(FlappyGhost.instance != null && FlappyGhost.instance.isGameInProgress() && other.CompareTag("Screen"))
        {
            FlappyGhost.instance.GameIsOver();
        }
    }

    void FixedUpdate()
    {

        if(jumpRequest)
        {
            rb.velocity = Vector2.zero;
            rb.AddForce(jumpForce * Vector2.up, ForceMode2D.Impulse);
            jumpRequest = false;
        }

        if(FlappyGhost.instance.isGameInProgress()) GamePhysics();
    }

    void GamePhysics()
    {
        if (rb.velocity.y < 0) //CHARACTER IS FALLING
        {
            rb.gravityScale = fallMultiplier;
        }
        else if (rb.velocity.y > 0)
        {
            rb.gravityScale = fallMultiplier / 1.5f;
        }
        else
        {
            rb.gravityScale = 1;
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MiniGameBook : MonoBehaviour
{
  
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();    
    }
    private void OnMouseDown()
    {
        MiniGameManager.instance.StartGenerateMiniGameCoroutine();
    }

    public void ExitMiniGameBook()
    {
        animator.Play("HintBook_Exit");
    }

    public void DestroyMiniGameBook()
    {
        Destroy(gameObject);
    }
}

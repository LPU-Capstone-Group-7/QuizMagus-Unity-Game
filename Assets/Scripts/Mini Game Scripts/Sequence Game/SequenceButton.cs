using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class SequenceButton : MonoBehaviour
{
    public Action onButtonClicked;
    public int indexNumber;

    [SerializeField] private Animator animator;

    private void Awake() 
    {
        animator = GetComponent<Animator>();
    }

    private void OnMouseDown()
    {
        if(!SequenceGame.instance.GetIsGeneratingSequence())
        {
            onButtonClicked?.Invoke();
        }
        
    }

    public void setIndexNumber(int indexNumber)
    {
        this.indexNumber = indexNumber;
    }

    public void PlayButtonGlowAnimation()
    {
        animator.Play("SequenceButton_Glow");
    }

}

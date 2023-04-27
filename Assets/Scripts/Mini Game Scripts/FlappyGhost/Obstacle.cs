using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Obstacle : MonoBehaviour
{
    private float movementSpeed;
    private Vector3 targetPosition;
    public Action onDestroyAction;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(targetPosition != null)
        {
            if(Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
            }
            else
            {
                transform.position = targetPosition;
                onDestroyAction?.Invoke();
                Destroy(gameObject);
            }

        }
    }

    public void InitializeMovement(Vector3 targetPosition, float movementSpeed)
    {
        this.movementSpeed = movementSpeed;
        this.targetPosition = new Vector3(targetPosition.x, transform.position.y);
    }
}

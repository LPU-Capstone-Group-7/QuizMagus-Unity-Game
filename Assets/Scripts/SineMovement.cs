using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineMovement : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Vector3 targetDirection;
    public float waveFrequency = 1f;
    public float waveAmplitude = 1f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float waveY = Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;
        Vector3 offsetDirection = new Vector3(targetDirection.x, waveY, targetDirection.z);
        transform.position += offsetDirection * Time.deltaTime;
    }
}

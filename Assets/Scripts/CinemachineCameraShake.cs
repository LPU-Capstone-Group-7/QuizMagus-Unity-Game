using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CinemachineCameraShake : MonoBehaviour
{

    
    public static CinemachineCameraShake instance { get; private set; }

    CinemachineVirtualCamera cinemachineVirtualCamera;
    CinemachineBasicMultiChannelPerlin channelPerlin;

    float shakeTimer;
    float shakeTimerTotal;
    float shakeIntensity;

    Canvas[] canvasObjectArray;
    List<Vector3> canvasOriginalPositions = new List<Vector3>();
    bool shakeWithCanvas = true;

    // Start is called before the first frame update

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
        channelPerlin = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        canvasObjectArray = GameObject.FindObjectsOfType<Canvas>();

        foreach (Canvas canvas in canvasObjectArray)
        {
            canvasOriginalPositions.Add(canvas.transform.position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;

            if(shakeWithCanvas) ShakeCanvas();

            if (shakeTimer <= 0f) //Timer Finished
            {
                channelPerlin.m_AmplitudeGain = Mathf.Lerp(shakeIntensity, 0f, 1 - (shakeTimer / shakeTimerTotal));

                 float t = 1 - (shakeTimer / shakeTimerTotal);

                for (int i = 0; i < canvasObjectArray.Length; i++)
                {
                    Canvas canvas = canvasObjectArray[i];

                    Vector3 newPos = Vector3.Lerp(canvas.transform.localPosition, canvasOriginalPositions[i], t);
                    canvas.transform.localPosition = newPos;
                }
            }
        }
    }

    public void ShakeCamera(float intensity, float time)
    {
        shakeIntensity = intensity;
        channelPerlin.m_AmplitudeGain = intensity;
        shakeTimer = time;
        shakeTimerTotal = time;
    }

    public void ShakeCanvas()
    {
        foreach (Canvas canvas in canvasObjectArray)
        {
            canvas.transform.localPosition = Random.insideUnitSphere * shakeIntensity;
        }
    }

    public bool IsCameraShaking()
    {
        return channelPerlin.m_AmplitudeGain != 0;
    }
}

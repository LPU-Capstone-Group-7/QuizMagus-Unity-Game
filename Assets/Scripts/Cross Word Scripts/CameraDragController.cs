using System.Collections;
using UnityEngine;
using Cinemachine;

public class CameraDragController : MonoBehaviour
{
    public static CameraDragController instance;
    [SerializeField] private float cameraSpeed;
    private Vector3 dragOrigin;
    private bool isDragging;
    private bool canDrag = false;

    [Header("Cinemachine Camera")]
    private CinemachineVirtualCamera virtualCamera;

    [Header("Camera Bounding Box")]
    [SerializeField] private Vector2 defaultBoundingBoxSize;
    Vector3 bottomLeftCoords, upperRightCoords;

    void Awake()
    {
        if(instance == null) instance = this;            
    }
    void Start()
    {
        //FIND CINEMACHINE CAMERA GAME OBJECT AND SET THIS AS PARENT
        Transform cameraTransform = GameObject.FindObjectOfType<Cinemachine.CinemachineVirtualCamera>().transform;


        if(cameraTransform != null)
        {
            cameraTransform.SetParent(transform);
        }
        else
        {
            Destroy(gameObject);
        }

        virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();

    }

    void Update() 
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            dragOrigin = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }
    void FixedUpdate()
    {
        if (isDragging && canDrag)
        {
            Vector3 currentPosition = Input.mousePosition;
            Vector3 dragDifference = Camera.main.ScreenToViewportPoint(dragOrigin - currentPosition) * cameraSpeed * Time.deltaTime;
            Vector3 newPosition = Camera.main.transform.position + dragDifference;

            // Update the position of the Cinemachine Virtual Camera
            virtualCamera.transform.position = ClampCameraPosition(newPosition, bottomLeftCoords, upperRightCoords);
        }
    }
    
    public void SetCameraBoundingBox(Vector2 boxSize, Vector3 centerPosition, float margin = 0f)
    {
        //MAKES SURE THAT THE NEW BOUNDING BOX IS NOT SMALLER THAN THE DEFAULT BOUNDING BOX
        if(boxSize.x >= defaultBoundingBoxSize.x || boxSize.y >= defaultBoundingBoxSize.y)
        {
            canDrag = true;
            bottomLeftCoords = new Vector3(centerPosition.x - boxSize.x - margin, centerPosition.y - boxSize.y - margin, centerPosition.z);
            upperRightCoords =  new Vector3(centerPosition.x + boxSize.x + margin,  centerPosition.y + boxSize.y + margin);
        }

    }

    public IEnumerator FocusCameraOnThisPosition(Vector3 position)
    {
        //INITIALIZE VALUES
        Vector3 initialPosition = virtualCamera.transform.position;
        float elapsedTime = 0f;

        //GET TARGET POSITION WHILE TAKING THE CLAMPING INTO CONSIDERATION
        Vector3 targetPosition = ClampCameraPosition(new Vector3(position.x, position.y, virtualCamera.transform.position.z), bottomLeftCoords, upperRightCoords);

        if (targetPosition != position)
        {
            float distanceToTarget = Vector3.Distance(initialPosition, targetPosition);
            float clampedMoveSpeed = distanceToTarget / 20f;

            while (elapsedTime < clampedMoveSpeed)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / clampedMoveSpeed);

                virtualCamera.transform.position = Vector3.Lerp(initialPosition, targetPosition, t);

                yield return null;
            }

            // Ensure the camera reaches the exact target position
            virtualCamera.transform.position = targetPosition;
        }
    }

    private Vector3 ClampCameraPosition(Vector3 position, Vector2 bottomLeftCoords, Vector2 upperRightCoords)
    {
        Vector3 newPosition = position;

        //DETERMINE THE BOUNDING BOX OF THE CAMERA
        float cameraHeight = Camera.main.orthographicSize;
        float cameraWidth = cameraHeight * Camera.main.aspect;

        if (Mathf.Abs(upperRightCoords.x - bottomLeftCoords.x) > cameraWidth * 2)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, bottomLeftCoords.x + cameraWidth, upperRightCoords.x - cameraWidth);
        }
        else
        {
            // Camera width is larger than available horizontal distance, don't allow horizontal movement
            newPosition.x = Camera.main.transform.position.x;
        }

        if (Mathf.Abs(upperRightCoords.y - bottomLeftCoords.y) > cameraHeight * 2)
        {
            newPosition.y = Mathf.Clamp(newPosition.y, bottomLeftCoords.y + cameraHeight, upperRightCoords.y - cameraHeight);
        }
        else
        {
            // Camera height is larger than available vertical distance, don't allow vertical movement
            newPosition.y = Camera.main.transform.position.y;
        }

        return newPosition;
    }

    public void EnableCameraDrag(bool state)
    {
        canDrag = state;
    }

    private void OnDrawGizmos()
    {
        //Vector3 bottomLeftCoords = new Vector3(transform.position.x - defaultBoundingBoxSize.x, transform.position.y - defaultBoundingBoxSize.y, transform.position.z);
        //Vector3 upperRightCoords =  new Vector3(transform.position.x + defaultBoundingBoxSize.x,  transform.position.y + defaultBoundingBoxSize.y);

        Gizmos.color = Color.green;
        //GIZMOS SHIT
        Gizmos.DrawLine(bottomLeftCoords, new Vector3(bottomLeftCoords.x, upperRightCoords.y));
        Gizmos.DrawLine(bottomLeftCoords, new Vector3(upperRightCoords.x, bottomLeftCoords.y));
        
        Gizmos.DrawLine(upperRightCoords, new Vector3(bottomLeftCoords.x, upperRightCoords.y));
        Gizmos.DrawLine(upperRightCoords, new Vector3(upperRightCoords.x, bottomLeftCoords.y));    
    }
}
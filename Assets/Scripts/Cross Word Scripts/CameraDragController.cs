using UnityEngine;

public class CameraDragController : MonoBehaviour
{
    [SerializeField] private float cameraSpeed;
    private Vector3 dragOrigin;
    private bool isDragging;
    private bool canDrag = false;

    [Header("Camera Bounding Box")]
    [SerializeField] private Vector2 defaultBoundingBoxSize;
    Vector3 bottomLeftCoords, upperRightCoords;

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

            //DETERMINE THE BOUNDING BOX OF THE CAMERA
            float cameraHeight = Camera.main.orthographicSize;
            float cameraWidth = cameraHeight * Camera.main.aspect;

            newPosition.x = Mathf.Clamp(newPosition.x, bottomLeftCoords.x + cameraWidth, upperRightCoords.x - cameraWidth);
            newPosition.y = Mathf.Clamp(newPosition.y, bottomLeftCoords.y + cameraHeight, upperRightCoords.y - cameraHeight);

            // Update the position of the Cinemachine Virtual Camera
            Cinemachine.CinemachineVirtualCamera virtualCamera = GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>();
            virtualCamera.transform.position = newPosition;
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

    private void OnDrawGizmos()
    {
        Vector3 bottomLeftCoords = new Vector3(transform.position.x - defaultBoundingBoxSize.x, transform.position.y - defaultBoundingBoxSize.y, transform.position.z);
        Vector3 upperRightCoords =  new Vector3(transform.position.x + defaultBoundingBoxSize.x,  transform.position.y + defaultBoundingBoxSize.y);
        //GIZMOS SHIT
        Gizmos.DrawLine(bottomLeftCoords, new Vector3(bottomLeftCoords.x, upperRightCoords.y));
        Gizmos.DrawLine(bottomLeftCoords, new Vector3(upperRightCoords.x, bottomLeftCoords.y));
        
        Gizmos.DrawLine(upperRightCoords, new Vector3(bottomLeftCoords.x, upperRightCoords.y));
        Gizmos.DrawLine(upperRightCoords, new Vector3(upperRightCoords.x, bottomLeftCoords.y));    
    }
}
using UnityEngine;

public class LolStyleCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public bool isLocked = true;

    [Header("Camera Settings")]
    public float fov = 40f;
    public float tiltAngle = 56f;
    public float moveSpeed = 50f;
    public float edgeSize = 20f;

    [Header("Zoom Settings")]
    public float minDistance = 25f;
    public float maxDistance = 50f;
    public float zoomSpeed = 100f;
    public float zoomSmoothTime = 0.1f;

    private float targetDistance;
    private float currentDistance;
    private float zoomVelocity;
    
    private Vector3 freeCameraPos;
    private Quaternion fixedRotation;

    void Start()
    {
        GetComponent<Camera>().fieldOfView = fov;
        fixedRotation = Quaternion.Euler(tiltAngle, 0f, 0f);
        
        targetDistance = (minDistance + maxDistance) / 2f;
        currentDistance = maxDistance;

        if (target) freeCameraPos = target.position;
    }

    void Update()
    {
        // Alternar bloqueo con la Y
        if (Input.GetKeyDown(KeyCode.Y))
        {
            isLocked = !isLocked;
            if (isLocked) Debug.Log("Cámara Bloqueada");
            else {
                Debug.Log("Cámara Libre");
                freeCameraPos = target.position;
            }
        }

        HandleZoom();
    }

    void LateUpdate()
    {
        if (!target) return;

        currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref zoomVelocity, zoomSmoothTime);

        Vector3 desiredFocusPoint;

        if (isLocked)
        {
            desiredFocusPoint = target.position;
            freeCameraPos = target.position;
        }
        else
        {
            HandleEdgeScrolling();
            desiredFocusPoint = freeCameraPos;
        }

        desiredFocusPoint.y = 0;
        Vector3 offset = fixedRotation * new Vector3(0f, 0f, -currentDistance);
        transform.position = desiredFocusPoint + offset;
        transform.rotation = fixedRotation;
    }

    void HandleEdgeScrolling()
    {
        Vector3 moveInput = Vector3.zero;

        if (Input.mousePosition.x >= Screen.width - edgeSize) moveInput.x += 1;
        if (Input.mousePosition.x <= edgeSize) moveInput.x -= 1;
        if (Input.mousePosition.y >= Screen.height - edgeSize) moveInput.z += 1;
        if (Input.mousePosition.y <= edgeSize) moveInput.z -= 1;

        freeCameraPos += moveInput.normalized * moveSpeed * Time.deltaTime;
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            targetDistance -= scroll * zoomSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }
    }
}
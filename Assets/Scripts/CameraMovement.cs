using UnityEngine;

public class CamerMovement: MonoBehaviour
{
    public float moveSpeed = 5f;
    public float zoomSpeed = 5f;
    public float minZoom = 2f;
    public float maxZoom = 10f;
    public float dragSpeed = 1f;

    public float edgeSize = 10f; // Pixels from screen edge to trigger camera movement
    public bool enableEdgeMovement = true;

    public float momentumDamping = 5f; // Higher = faster momentum falloff
    public float momentumThreshold = 0.05f; // Minimum momentum before stopping

    private Vector3 dragOrigin;
    private bool isDragging = false;
    private Vector3 momentum;

    private Vector3 targetPosition;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        targetPosition = transform.position;
    }

    void Update()
    {
        HandleKeyboardMovement();
        HandleMouseEdgeMovement();
        HandleZoom();
        HandleMouseDragMovement();

        if (momentum.magnitude > momentumThreshold)
        {
            targetPosition += momentum * Time.deltaTime;
            momentum = Vector3.Lerp(momentum, Vector3.zero, momentumDamping * Time.deltaTime);
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, 10f * Time.deltaTime);
    }

    void HandleKeyboardMovement()
    {
        Vector3 move = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) move.y += 1;
        if (Input.GetKey(KeyCode.S)) move.y -= 1;
        if (Input.GetKey(KeyCode.A)) move.x -= 1;
        if (Input.GetKey(KeyCode.D)) move.x += 1;

        move = move.normalized * moveSpeed * Time.deltaTime;
        targetPosition += move;
    }

    void HandleMouseEdgeMovement()
    {
        if (!enableEdgeMovement) return;

        Vector3 move = Vector3.zero;
        Vector3 mousePos = Input.mousePosition;

        if (mousePos.y >= Screen.height - edgeSize) move.y += 1;
        if (mousePos.y <= edgeSize) move.y -= 1;
        if (mousePos.x <= edgeSize) move.x -= 1;
        if (mousePos.x >= Screen.width - edgeSize) move.x += 1;

        move = move.normalized * moveSpeed * Time.deltaTime;
        targetPosition += move;
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }

    void HandleMouseDragMovement()
    {
        if (Input.GetMouseButtonDown(2))
        {
            isDragging = true;
            dragOrigin = Input.mousePosition;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            momentum = Vector3.zero; // reset momentum while dragging
        }

        if (Input.GetMouseButtonUp(2))
        {
            isDragging = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            momentum = new Vector3(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"), 0) * dragSpeed;
        }

        if (isDragging)
        {
            float moveX = -Input.GetAxis("Mouse X") * dragSpeed;
            float moveY = -Input.GetAxis("Mouse Y") * dragSpeed;

            Vector3 move = new Vector3(moveX, moveY, 0);
            targetPosition += move;
        }
    }
}

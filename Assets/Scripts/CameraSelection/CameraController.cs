
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float sprintMultiplier = 3f;

    [Header("Rotation Settings")]
    [SerializeField] private float mouseSensitivity = 0.15f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minFOV = 15f;
    [SerializeField] private float maxFOV = 90f;

    private Camera playerCamera;
    private float yaw;
    private float pitch;

    private void Awake()
    {
        playerCamera = GetComponent<Camera>();

        // Preserve the camera's starting rotation
        yaw = transform.eulerAngles.y;
        pitch = Mathf.DeltaAngle(0f, transform.eulerAngles.x);
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleZoom();
    }

    // First-person keyboard movement
    private void HandleMovement()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        Vector3 movement = Vector3.zero;

        if (keyboard.wKey.isPressed)
            movement += transform.forward;

        if (keyboard.sKey.isPressed)
            movement -= transform.forward;

        if (keyboard.aKey.isPressed)
            movement -= transform.right;

        if (keyboard.dKey.isPressed)
            movement += transform.right;

        if (keyboard.eKey.isPressed)
            movement += Vector3.up;

        if (keyboard.qKey.isPressed)
            movement -= Vector3.up;

        float speed = moveSpeed;

        // Hold Shift to move faster
        if (keyboard.leftShiftKey.isPressed)
            speed *= sprintMultiplier;

        transform.position += movement.normalized
            * speed * Time.unscaledDeltaTime;
    }

    // Rotate camera while holding the right mouse button
    private void HandleRotation()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null || !mouse.rightButton.isPressed)
            return;

        Vector2 mouseDelta = mouse.delta.ReadValue();

        yaw += mouseDelta.x * mouseSensitivity;
        pitch -= mouseDelta.y * mouseSensitivity;

        // Prevent the camera from flipping upside down
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    // Zoom using the mouse scroll wheel
    private void HandleZoom()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        float scroll = mouse.scroll.ReadValue().y;

        if (scroll != 0f)
        {
            playerCamera.fieldOfView -=
                Mathf.Sign(scroll) * zoomSpeed;

            playerCamera.fieldOfView = Mathf.Clamp(
                playerCamera.fieldOfView, minFOV, maxFOV);
        }
    }
}

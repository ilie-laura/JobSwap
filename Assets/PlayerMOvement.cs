using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Mișcare")]
    public float moveSpeed = 5.0f;
    public float jumpHeight = 1.2f;
    public float gravity = -19.62f;

    [Header("Privire (Mouse Look)")]
    public Transform cameraTransform;
    public float mouseSensitivity = 15.0f;
    public float upperLookLimit = 85.0f;
    public float lowerLookLimit = -85.0f;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
    }

    void HandleLook()
    {
        if (cameraTransform == null || Mouse.current == null) return;

        // Citim mișcarea mouse-ului
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;

        // Rotire cameră pe verticală (Sus/Jos)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, lowerLookLimit, upperLookLimit);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotire corp jucător pe orizontală (Stânga/Dreapta)
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Preluare intrări de la tastatură
        Vector2 inputVector = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) inputVector.y += 1f;
            if (Keyboard.current.sKey.isPressed) inputVector.y -= 1f;
            if (Keyboard.current.dKey.isPressed) inputVector.x += 1f;
            if (Keyboard.current.aKey.isPressed) inputVector.x -= 1f;
        }

        // Normalizare vector pentru a preveni mișcarea mai rapidă pe diagonală
        inputVector.Normalize();

        // Direcție bazată STRICT pe orientarea curentă a corpului
        Vector3 moveDirection = transform.right * inputVector.x + transform.forward * inputVector.y;

        // APLICARE MIȘCARE (Fără nicio rotire adăugată aici)
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        // Săritură
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Gravitație
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
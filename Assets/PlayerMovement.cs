using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Camera Settings")]
    public Transform playerCamera;
    public float mouseSensitivity = 2f;

    public float interactDistance = 3f;

    private bool _canMove = true;
    private bool _interviewMode = false; // true = in interview mode

    private CharacterController controller;
    private Vector3 velocity;
    private float cameraPitch = 0f;
    private bool isGrounded;

    public void SetInterviewMode(bool interviewing)
    {
        _interviewMode = interviewing;
        _canMove = !interviewing;

        if (interviewing)
        {
            // Unlock cursor to click UI, but only look around when holding right mouse
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void SetFullLock(bool locked)
    {
        _canMove = !locked;
        _interviewMode = false;
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (_canMove) HandleMovement();
        else ApplyGravityOnly();

        HandleMouseLook();

        if (_canMove && Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(playerCamera.position, playerCamera.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            {
                DoorInteraction door = hit.collider.GetComponentInParent<DoorInteraction>();
                if (door != null) door.ToggleDoor();
            }
        }
    }

    void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        bool sprinting = Input.GetKey(KeyCode.LeftShift);
        float speed = sprinting ? sprintSpeed : walkSpeed;

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void ApplyGravityOnly()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        if (_interviewMode)
        {
            //when in interview mode, only look around when holding right mouse, otherwise free cursor to click UI
            if (Input.GetMouseButton(1)) //holding right mouse
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                ApplyLook();
            }
            else
            {
                // release right mouse, unlock cursor to click/interact
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        else
        {
            ApplyLook();
        }
    }

    void ApplyLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -75f, 75f);
        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
    }
}
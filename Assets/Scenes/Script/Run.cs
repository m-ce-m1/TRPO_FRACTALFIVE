using UnityEngine;

public class Run : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 7f;
    public float gravity = -9.81f;

    [Header("Camera Settings")]
    public Transform cameraTransform;
    public float cameraSmoothness = 10f;
    public Vector3 cameraOffset = new Vector3(0, 2, -5);
    public float mouseSensitivity = 2f;

    [Header("Components")]
    public CharacterController characterController;
    public Animator animator;

    private Vector3 movement;
    private Vector3 velocity;
    private float horizontalInput;
    private float verticalInput;
    private float mouseX;
    private float mouseY;
    private float cameraPitch = 0f;
    private bool isGrounded;
    private bool isRunning;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    void Start()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
        if (animator == null)
            animator = GetComponent<Animator>();
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            gc.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = gc.transform;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        GetInput();
        HandleCameraRotation();
        HandleMovement();
        HandleGravityAndJump();
        UpdateAnimations();
    }

    void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        isRunning = Input.GetKey(KeyCode.LeftShift);
    }

    void HandleCameraRotation()
    {
        // Поворот персонажа по горизонтали
        transform.Rotate(Vector3.up * mouseX);

        // Вертикальный наклон камеры
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);
    }

    void HandleMovement()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        movement = (camForward * verticalInput + camRight * horizontalInput);
        movement = Vector3.ClampMagnitude(movement, 1f);

        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        characterController.Move(movement * currentSpeed * Time.deltaTime);
    }

    void HandleGravityAndJump()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Прыжок только при нажатии Space и на земле
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    void LateUpdate()
    {
        // Плавная камера
        Vector3 targetPos = transform.position + transform.TransformDirection(cameraOffset);
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPos, cameraSmoothness * Time.deltaTime);

        // Поворот камеры
        cameraTransform.rotation = Quaternion.Euler(cameraPitch, transform.eulerAngles.y, 0);
    }

    void UpdateAnimations()
    {
        if (animator != null)
        {
            float speed = movement.magnitude;
            if (isRunning && speed > 0) speed *= 2f;

            animator.SetFloat("Speed", speed);
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetBool("IsRunning", isRunning);
            animator.SetFloat("VerticalVelocity", velocity.y);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}

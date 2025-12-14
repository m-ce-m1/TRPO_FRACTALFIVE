using UnityEngine;

public class Run : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Camera Settings")]
    public Transform cameraTransform;
    public float cameraSmoothness = 10f;
    public Vector3 cameraOffset = new Vector3(0, 2, -5);
    public float mouseSensitivity = 2f;

    [Header("Pickup Settings")]
    public Transform holdPosition; // Позиция где будет держаться предмет
    public float pickupRange = 3f; // Дистанция поднятия
    public float rotationSpeed = 5f; // Скорость вращения предмета
    public LayerMask pickupLayer; // Слой для предметов которые можно поднять

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

    // Переменные для системы поднятия предметов
    private GameObject heldObject;
    private Rigidbody heldObjectRb;
    private bool isHoldingItem = false;
    private float holdDistance = 2f; // Дистанция от камеры до предмета

    void Start()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
        if (animator == null)
            animator = GetComponent<Animator>();
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        // Создаем точку удержания предмета если не задана
        if (holdPosition == null)
        {
            GameObject holdPoint = new GameObject("HoldPosition");
            holdPoint.transform.SetParent(cameraTransform);
            holdPoint.transform.localPosition = new Vector3(0, 0, 2f); // На расстоянии 2 метра перед камерой
            holdPosition = holdPoint.transform;
        }

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
        HandleGravityAndJump();
        HandleMovement();
        UpdateAnimations();
        HandlePickup();

        // Если держим предмет, обрабатываем вращение
        if (isHoldingItem)
        {
            HandleObjectRotation();
        }
    }

    void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        isRunning = Input.GetKey(KeyCode.LeftShift);
    }

    void HandlePickup()
    {
        // Поднять/отпустить предмет по нажатию E
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isHoldingItem)
            {
                TryPickupObject();
            }
            else
            {
                DropObject();
            }
        }
    }

    void TryPickupObject()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, pickupRange))
        {
            // Проверяем есть ли у объекта Rigidbody
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                PickupObject(hit.collider.gameObject);
            }
        }
    }

    void PickupObject(GameObject objToPickup)
    {
        isHoldingItem = true;
        heldObject = objToPickup;
        heldObjectRb = heldObject.GetComponent<Rigidbody>();

        if (heldObjectRb != null)
        {
            heldObjectRb.useGravity = false;
            heldObjectRb.linearDamping = 10f;
            heldObjectRb.angularDamping = 10f;
            heldObjectRb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        // Отключаем коллайдер или делаем его триггером
        Collider collider = heldObject.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }

        // Отключаем возможность двигаться пока держим предмет (опционально)
        // walkSpeed = 0f;
        // runSpeed = 0f;

        Debug.Log("Поднял предмет: " + heldObject.name);
    }

    void DropObject()
    {
        if (heldObject == null) return;

        if (heldObjectRb != null)
        {
            heldObjectRb.useGravity = true;
            heldObjectRb.linearDamping = 1f;
            heldObjectRb.angularDamping = 0.5f;
            heldObjectRb.constraints = RigidbodyConstraints.None;

            // Добавляем небольшую силу вперед при бросании
            heldObjectRb.AddForce(cameraTransform.forward * 2f, ForceMode.Impulse);
        }

        // Восстанавливаем коллайдер
        Collider collider = heldObject.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = false;
        }

        // Восстанавливаем скорость движения
        // walkSpeed = 5f;
        // runSpeed = 8f;

        Debug.Log("Выбросил предмет: " + heldObject.name);

        isHoldingItem = false;
        heldObject = null;
        heldObjectRb = null;
    }

    void HandleObjectRotation()
    {
        if (heldObject == null) return;

        // Двигаем предмет к позиции удержания
        Vector3 targetPosition = holdPosition.position; // Используем позицию holdPosition

        // Плавное движение предмета
        if (heldObjectRb != null)
        {
            Vector3 direction = targetPosition - heldObject.transform.position;
            heldObjectRb.linearVelocity = direction * 10f;

            // Вращение предмета при зажатой правой кнопке мыши
            if (Input.GetMouseButton(1))
            {
                float rotateX = Input.GetAxis("Mouse X") * rotationSpeed * 100f;
                float rotateY = Input.GetAxis("Mouse Y") * rotationSpeed * 100f;

                heldObject.transform.Rotate(cameraTransform.up, -rotateX, Space.World);
                heldObject.transform.Rotate(cameraTransform.right, rotateY, Space.World);
            }
        }

        // Изменение дистанции удержания колесиком мыши
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // Меняем позицию holdPosition
            Vector3 localPos = holdPosition.localPosition;
            localPos.z = Mathf.Clamp(localPos.z + scroll * 3f, 1f, 5f);
            holdPosition.localPosition = localPos;
        }
    }

    void HandleCameraRotation()
    {
        // Если держим предмет и зажата правая кнопка - только предмет вращается, не камера
        if (!isHoldingItem || !Input.GetMouseButton(1))
        {
            // Поворот персонажа по горизонтали
            transform.Rotate(Vector3.up * mouseX);

            // Вертикальный наклон камеры
            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);
        }
    }

    void HandleGravityAndJump()
    {
        // ВАЖНО: Character Controller имеет свою гравитацию
        // Мы используем isGrounded от CharacterController

        isGrounded = characterController.isGrounded;

        // Дополнительная проверка сферой (опционально)
        if (!isGrounded)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }

        // Если на земле и скорость вниз, обнуляем вертикальную скорость
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Прыжок при нажатии пробела и если на земле
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // Формула для прыжка: v = √(h * -2 * g)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Применяем гравитацию К Character Controller
        velocity.y += gravity * Time.deltaTime;
    }

    void HandleMovement()
    {
        // Получаем направление движения относительно камеры
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // Рассчитываем движение
        movement = (camForward * verticalInput + camRight * horizontalInput);
        movement = Vector3.ClampMagnitude(movement, 1f);

        // Применяем скорость
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        Vector3 moveVelocity = movement * currentSpeed;

        // Добавляем вертикальную скорость (для прыжка и гравитации)
        moveVelocity.y = velocity.y;

        // Двигаем CharacterController
        characterController.Move(moveVelocity * Time.deltaTime);
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

            // Анимация для удержания предмета
            animator.SetBool("IsHoldingItem", isHoldingItem);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }

        // Визуализация луча для поднятия предметов
        if (cameraTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * pickupRange);
        }
    }
}
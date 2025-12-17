using UnityEngine;

// ВНИМАНИЕ: Этот скрипт отключён, т.к. управление камерой есть в PlayerController (Run.cs)
// Если вы хотите использовать этот скрипт вместо PlayerController, удалите комментарий ниже
// и отключите PlayerController на объекте игрока

/*
public class Camera_1 : MonoBehaviour
{
    public Transform playerBody;
    public float mouseSensitivity = 150f;

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        playerBody.Rotate(Vector3.up * mouseX);
    }
}
*/

using UnityEngine;

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

        // Вращаем камеру вверх-вниз
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Вращаем тело персонажа влево-вправо
        playerBody.Rotate(Vector3.up * mouseX);
    }
}

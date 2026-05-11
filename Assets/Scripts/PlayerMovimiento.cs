using UnityEngine;
using UnityEngine.InputSystem;

public partial class PlayerMovimiento : MonoBehaviour
{
    public float speed = 10f;
    public float rotationSpeed = 100f; // Ahora controla el giro del cuerpo

    private CharacterController controller;
    private float gravity = -9.81f;
    private float verticalVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // 1. ROTACIÓN (Teclas A y D)
        float rotationInput = 0;
        if (kb.aKey.isPressed) rotationInput = -1;
        if (kb.dKey.isPressed) rotationInput = 1;

        // Giramos el robot sobre su eje Y
        transform.Rotate(0, rotationInput * rotationSpeed * Time.deltaTime, 0);

        // 2. MOVIMIENTO (Teclas W y S)
        float moveInput = 0;
        if (kb.wKey.isPressed) moveInput = 1;
        if (kb.sKey.isPressed) moveInput = -1;

        // Calculamos la gravedad
        if (controller.isGrounded)
        {
            verticalVelocity = -0.5f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // El movimiento es siempre hacia el frente LOCAL del robot (transform.forward)
        Vector3 move = transform.forward * moveInput * speed;
        move.y = verticalVelocity;

        // Aplicamos el movimiento
        controller.Move(move * Time.deltaTime);
    }
}
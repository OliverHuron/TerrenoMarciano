using UnityEngine;
using UnityEngine.InputSystem;

public partial class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Modo de Cámara")]
    public bool isFirstPerson = false;
    public Vector3 fpOffset = new Vector3(0, 1.8f, 0.5f);

    [Header("Configuración General")]
    public float sensitivity = 50f;
    public float smoothSpeed = 15f;

    [Header("Configuración Tercera Persona")]
    public Vector3 offset = new Vector3(0, 2, -5);
    public float minDistance = 1.5f;
    public LayerMask collisionLayers;

    private float rotX = 0f;
    private float rotY = 0f;
    private float currentDistance;
    private float maxDistance;

    void Start()
    {
        

        Cursor.lockState = CursorLockMode.Locked;
        // Sincronizamos la rotación inicial con el robot
        if (target != null) rotY = target.eulerAngles.y;

        maxDistance = offset.magnitude;
    }

    void LateUpdate()
    {
        if (Keyboard.current.cKey.wasPressedThisFrame) // Presiona la tecla 'C'
        {
            isFirstPerson = !isFirstPerson;
        }

        if (!target) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // 1. ROTACIÓN
        // Horizontal (Y): Afecta directamente al ROBOT
        rotY += mouseDelta.x * sensitivity * Time.deltaTime;
        // Vertical (X): Solo afecta a la CÁMARA (mirar arriba/abajo)
        rotX -= mouseDelta.y * sensitivity * Time.deltaTime;
        rotX = Mathf.Clamp(rotX, -80f, 80f);

        if (isFirstPerson)
        {
            // --- MODO PRIMERA PERSONA FIJA ---

            // A. Giramos el cuerpo del robot físicamente a los lados
            target.rotation = Quaternion.Euler(0, rotY, 0);

            // B. Posicionamos la cámara en los ojos (es fija respecto al robot)
            transform.position = target.TransformPoint(fpOffset);

            // C. La cámara hereda el giro del robot + su propio cabeceo arriba/abajo
            transform.rotation = Quaternion.Euler(rotX, rotY, 0);
        }
        else
        {
            // --- MODO TERCERA PERSONA (Orbital) ---
            Quaternion rotation = Quaternion.Euler(rotX, rotY, 0);
            Vector3 dir = rotation * Vector3.back;
            Vector3 rayOrigin = target.position + Vector3.up * 1.0f;

            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, dir, out hit, maxDistance, collisionLayers))
            {
                currentDistance = Mathf.Clamp(hit.distance - 0.2f, minDistance, maxDistance);
            }
            else
            {
                currentDistance = Mathf.Lerp(currentDistance, maxDistance, Time.deltaTime * smoothSpeed);
            }

            Vector3 targetPos = rayOrigin + (dir * currentDistance);
            transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
            transform.LookAt(rayOrigin);
        }
    }
}





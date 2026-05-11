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
        if (target != null) rotY = target.eulerAngles.y;
        maxDistance = offset.magnitude;
        currentDistance = maxDistance;
    }

    void LateUpdate()
    {
        if (Keyboard.current.cKey.wasPressedThisFrame) isFirstPerson = !isFirstPerson;
        if (!target) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        rotY += mouseDelta.x * sensitivity * Time.deltaTime;
        rotX -= mouseDelta.y * sensitivity * Time.deltaTime;

        // Limitamos para que no de la vuelta completa
        rotX = Mathf.Clamp(rotX, -80f, 80f);

        if (isFirstPerson)
        {
            target.rotation = Quaternion.Euler(0, rotY, 0);
            transform.position = target.TransformPoint(fpOffset);
            transform.rotation = Quaternion.Euler(rotX, rotY, 0);
        }
        else
        {
            // --- MODO TERCERA PERSONA MEJORADO ---

            // 1. AJUSTE DINÁMICO DE DISTANCIA SEGÚN ÁNGULO
            // Si rotX es negativo (mirando hacia arriba), reducimos la distancia máxima
            // para obligar a la cámara a acercarse al robot y no hundirse en el suelo.
            float angleFactor = Mathf.InverseLerp(0, -80, rotX);
            float dynamicMaxDistance = Mathf.Lerp(maxDistance, minDistance, angleFactor * 0.8f);

            Quaternion rotation = Quaternion.Euler(rotX, rotY, 0);
            Vector3 dir = rotation * Vector3.back;
            Vector3 rayOrigin = target.position + Vector3.up * 1.0f;

            // 2. COLISIÓN FÍSICA (Raycast)
            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, dir, out hit, dynamicMaxDistance, collisionLayers))
            {
                currentDistance = Mathf.Clamp(hit.distance - 0.3f, minDistance, dynamicMaxDistance);
            }
            else
            {
                // Si no hay colisión, usamos la distancia dinámica basada en el ángulo
                currentDistance = Mathf.Lerp(currentDistance, dynamicMaxDistance, Time.deltaTime * smoothSpeed);
            }

            Vector3 targetPos = rayOrigin + (dir * currentDistance);
            transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
            transform.LookAt(rayOrigin);
        }
    }
}





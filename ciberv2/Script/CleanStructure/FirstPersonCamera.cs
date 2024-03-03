using System;
using System.Collections;
using Script.Clean_Structure;
using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    
    [SerializeField] private Transform target;                      // Target camera.
    
    [SerializeField] private Transform targetRoll;                  // Target Camera roll.
    
    [SerializeField] private BehaviourManager behaviourManager;     // Reference to BehaviourManager.
    
    [SerializeField] private float rotationSpeed = 3.0f;            // Camera rotation speed.
    
    [SerializeField] private float minPitch = -50;                  // Minimum camera pitch angle.
    
    [SerializeField] private float maxPitch = 70;                   // Maximum camera pitch angle.
    
    [SerializeField] private float smoothSpeedRotation = 0.01f;     // Camera rotation smoothing speed.

    
    private float currentYaw = 0.0f;        // Current yaw angle of the camera.
    private float currentYawM = 0.0f;       // Current camera yaw angle modified.
    private float currentPitch = 0.0f;      // Current camera pitch angle.
    private float currentRoll = 0.0f;       // Current camera roll angle.

    private float mouseX = 0;               // Mouse input on the X axis.
    private float mouseY = 0;               // Mouse input on the Y axis.

    private Quaternion desiredQuaternion1;  // Desired quaternion 1 for camera rotation.
    private Quaternion desiredQuaternion2;  // Desired quaternion 2 for camera rotation.

    private Coroutine rotationCoroutine;    // Coroutine for camera rotation.

    private void Update()
    {
        // Se obtiene la entrada del mouse en los ejes X e Y
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
        
        // Se calculan los ángulos de yaw y pitch actuales de la cámara
        currentYaw += mouseX * rotationSpeed;
        currentPitch -= mouseY * rotationSpeed;
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
        
        // Se verifica si hay comportamientos sobrescritos o es diferente comportamiento por defecto.
        if (behaviourManager.overridingBehaviours.Count != 0 
            || behaviourManager.currentBehaviour != behaviourManager.defaultBehaviour)
        {
            
            if (rotationCoroutine == null)
            {
                rotationCoroutine = StartCoroutine(HandleRotationTargetAndCamera());
            }
        }
        else
        {
            
            if (rotationCoroutine != null)
            {
                StopCoroutine(rotationCoroutine);
                rotationCoroutine = null;
            }
            
            // Se restablecen los ángulos de yaw modificado y roll.
            currentYawM = 0;
            currentRoll = 0;
            targetRoll.rotation = Quaternion.identity;
        }
        
        // Se aplica la rotación a la cámara y al jugador.
        HandleRotationPlayerAndCamera();
    }

    // Maneja la rotación del objetivo y la cámara.
    private IEnumerator HandleRotationTargetAndCamera()
    {
        while (true)
        {
            InterpolateRotationCamera();
            yield return null;
        }
    }
    
    // Interpola la rotación de la cámara.
    private void InterpolateRotationCamera()    
    {
        // Se calcula la rotación del roll de la cámara.
        currentYawM += mouseX * rotationSpeed;
        targetRoll.rotation = Quaternion.Euler(0, currentYawM, 0);
        float angle = targetRoll.rotation.eulerAngles.y;
        float currentYawDesired;
        // Se determina el ángulo deseado de yaw modificado.   
        if (angle < 330 && angle > 210)
        {
            currentYawDesired = -10;
        }
        else if (angle > 30 && angle < 150)
        {
            currentYawDesired = 10;
        }
        else
        {
            currentYawDesired = 0;
        }
        
        // Se suaviza la interpolación del roll de la cámara.
        currentRoll = Mathf.Lerp(currentRoll, currentYawDesired, smoothSpeedRotation);
        
    }

    // Aplica la rotación al jugador y la cámara.
    private void HandleRotationPlayerAndCamera()
    {
        // Se calculan los cuaterniones deseados para la rotación de la cámara.
        desiredQuaternion1 = Quaternion.Euler(0, currentYaw, target.rotation.eulerAngles.z);
        desiredQuaternion2 = Quaternion.Euler(currentPitch, currentYaw, currentRoll);
        
        // Si no hay comportamientos anulando la cámara, se aplica la rotación.
        if (behaviourManager.ConditionGeneric())
        {
            target.rotation = desiredQuaternion1;
        }
        
        transform.rotation = desiredQuaternion2;
    }

    // Obtiene el ángulo de pitch actual de la cámara.
    public float GetCurrentPitchCamera()
    {
        return Mathf.Abs(currentPitch);
    }
}

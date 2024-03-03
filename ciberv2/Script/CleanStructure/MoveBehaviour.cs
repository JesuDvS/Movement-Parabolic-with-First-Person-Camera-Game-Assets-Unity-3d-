using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

namespace Script.Clean_Structure
{
    public class MoveBehaviour : GenericBehaviour
    {
        [Header("Individual Parameters")]
        
        [SerializeField] private float moveSpeed = 20f;     // Player movement speed.
        
        [SerializeField] private float jumpForce = 10f;     // Force applied when jumping.
        
        private Rigidbody rb;                               // Reference to the player's Rigidbody component.
        
        private bool hasJumpedDouble;                       // Indicator to check if the player has performed a double jump.

        private void Start()
        {
            rb = behaviourManager.GetRigidBody;
            
            // Se suscribe al BehaviourManager.
            behaviourManager.SubscribeBehaviour(this);
            
            // Se registra este comportamiento como comportamiento predeterminado.
            behaviourManager.RegisterDefaultBehaviour(GetBehaviourCode());
            
            // Se restablece el contador de saltos.
            ResetJumps();
        }

        

        private void Update()
        {
            // Gestión del salto.
            JumpManagement();
            
            // Aplicar gravedad.
            ApplyGravity();
        }
        
        public override void LocalFixedUpdate()
        {
            // Mover al jugador.
            MovePlayer(behaviourManager.GetV,behaviourManager.GetH);
        }

        public override void LocalLateUpdate()
        {
            // No se implementa en este comportamiento.
        }
        
        
        // Aplicar gravedad al jugador
        private void ApplyGravity()
        {
            // Si este comportamiento es el actual, se aplica la gravedad al jugador.
            if (behaviourManager.IsCurrentBehaviour(GetBehaviourCode()))
            {
                Vector3 gravityVector = Vector3.up * Physics.gravity.y * Time.deltaTime;
                behaviourManager.GetRigidBody.velocity += gravityVector;
            }
            
        }
        
        // Mover al jugador.
        private void MovePlayer(float verticalInput, float horizontalInput)
        {
            // Obtener la dirección de movimiento basada en la entrada del jugador y la orientación de la cámara.
            var cameraTransform = behaviourManager.playerCamera;
            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0f;
            Vector3 moveDirection = (cameraForward.normalized * verticalInput) + (cameraTransform.right.normalized * horizontalInput);
            
            // Aplicar la velocidad de movimiento al jugador.
            if (moveDirection != Vector3.zero)
            {
                
                Vector3 moveVector = moveDirection.normalized * moveSpeed;
                rb.velocity = new Vector3(moveVector.x, rb.velocity.y, moveVector.z);
                
            }
            else
            {
                // Si no hay entrada de movimiento, el jugador se detiene.
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
            }
        }

        // Gestión del salto
        private void JumpManagement()
        {
            // Si hay colisión con algún objeto, se restablece el contador de saltos
            if (IsAnyPointCollision())
            {
                ResetJumps();
            }
            
            // Si se presiona la tecla de salto y el jugador no ha realizado un doble salto
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!hasJumpedDouble)
                {
                    // Se realiza el salto
                    PerformJump();
                }
                
            }
        }

        // Realizar el salto
        private void PerformJump()
        {
            hasJumpedDouble = true;
            var velocity = rb.velocity;
            velocity = new Vector3(velocity.x, 0f, velocity.z); // Resetea la velocidad vertical antes del salto para evitar problemas con la gravedad
            velocity += Vector3.up * jumpForce;
            rb.velocity = velocity;
        }

        private void ResetJumps()
        {
            hasJumpedDouble = false;
        }
        
    }
}
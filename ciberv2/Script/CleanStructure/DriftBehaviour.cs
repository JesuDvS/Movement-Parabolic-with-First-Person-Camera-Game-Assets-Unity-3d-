using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Clean_Structure
{
    public class DriftBehaviour : GenericBehaviour
    {
        [Header("Individual Parameters")]
        
        [SerializeField] private float driftSpeed = 100f;   // Character's Drift speed.
        
        [SerializeField] private float driftTime= 1.2f;     // Total Drift time.
        
        private float driftTimer;                           // Timer to control the Drift time.
        
        private Coroutine currentCoroutine;                 // Current coroutine running.
        
        private bool allowParabolic;                        // Indicator to allow parabolic path.

        private void Start()
        {
            // Si no hay comportamientos unidos, se agrega el comportamiento ParabolicBehaviour.
            if (listJoinGenericBehaviours.Count  == 0)
            {
                listJoinGenericBehaviours.Add(GetComponent<ParabolicBehaviour>());
            }
            
            allowParabolic = false;
            
            // Se suscribe al BehaviourManager
            behaviourManager.SubscribeBehaviour(this);
        }

        private void Update()
        {
            // Si no hay ninguna coroutine en ejecución y se presiona la tecla de activación.
            if (currentCoroutine == null)
            {
                if (Input.GetKeyDown(KeyCode.LeftControl) 
                    && !behaviourManager.IsOverriding(this) 
                    && !behaviourManager.IsCurrentBehaviour(GetBehaviourCode()))
                {
                    // Se registra el comportamiento para ejecución.
                    behaviourManager.RegisterBehaviour(GetBehaviourCode());
                }
            }
            
            // Si este comportamiento es el actual y se presiona la tecla para activar la trayectoria parabólica.
            if (behaviourManager.IsCurrentBehaviour(GetBehaviourCode()) 
                && Input.GetKeyDown(KeyCode.LeftShift)
                && behaviourManager.QueueBehaviourEnd.Count == 0)
            {
                // Se unen los comportamientos, sin desactivar el registro de comportamiento.
                JoinBehaviours(false);
                allowParabolic = true;
            }
            
            
            
        }

        public override void LocalFixedUpdate()
        {
            
            // Si hay colisión con algún objeto, se detiene abruptamente la coroutine actual.
            if (IsAnyPointCollision())
            {
                StopAbruptlyCurrentCoroutine();
                return;
            }
            
            // Si no hay ninguna coroutine en ejecución, se inicia el Drift.
            if (currentCoroutine != null)
            {
                return;
            }
              
            currentCoroutine = StartCoroutine(DriftCoroutine());
            
            
        }

        // Coroutine para realizar la deriva del personaje.
        IEnumerator DriftCoroutine()
        {
            driftTimer = 0;
            
            // Si este comportamiento es el actual, se quita el padre del transformador del torso del jugador.
            if (behaviourManager.IsCurrentBehaviour(GetBehaviourCode()))
            {
                SetParentTransformTorsoPlayer(false);
            }
            
            // Bucle del drift durante el tiempo definido.
            while (driftTimer<driftTime)
            {
                
                Vector3 driftMovement = behaviourManager.forwardGeneral * (driftSpeed * Time.deltaTime);
                
                
                behaviourManager.TransformTorsoPlayer.Translate(driftMovement, Space.World);
                
                driftTimer += Time.deltaTime * (allowParabolic ? 2f : 1f);
                
                yield return null;
            }
            
            // Si este comportamiento es el actual y se permite la trayectoria parabólica, se anula el registro de este comportamiento.
            
            if (behaviourManager.IsCurrentBehaviour(GetBehaviourCode()) && allowParabolic)
            {
                behaviourManager.UnregisterBehaviour(GetBehaviourCode());
                allowParabolic = false;
            }
            else
            {
                // Si no se permite la trayectoria parabólica, se restablece el padre del transformador del torso del jugador.
                SetParentTransformTorsoPlayer();
            }

            Debug.Log("Finish InterpolationControl");
            currentCoroutine = null;
        }
        
        // Detiene abruptamente la corrutina actual
        void StopAbruptlyCurrentCoroutine()
        {
            StopAllCoroutines();
            SetParentTransformTorsoPlayer();
            currentCoroutine = null;
            Debug.Log("Secuencia detenida Total Control");
        }
    }
}
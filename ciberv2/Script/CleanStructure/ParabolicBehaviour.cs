using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using Sequence = DG.Tweening.Sequence;

namespace Script.Clean_Structure
{
    public class ParabolicBehaviour : GenericBehaviour
    {
        
        [Header("Individual Parameters")]
        
        [SerializeField] private List<Vector3> positionsSavedOnlyOnce;  // List of positions calculated only once.
        
        [SerializeField] private bool showPlayerParabolicPath = true;   // Indicates whether to show the player's parabolic trajectory.
        
        [SerializeField] private float velocityInterpolation = 0.005f;  // Interpolation speed.
        
        private Sequence sequenceInterpolation;                         // Interpolation sequence.
        
        private Tween moveTween;                                        // Movement tween.
        
        private Transform transformTorsoPlayer;                         // Transform of the player's torso.
        
        private LineRenderer lineRendererPlayer;                        // Line renderer for the player's path.
        
        private void Start()
        {
            // Inicialización de variables.
            lineRendererPlayer = behaviourManager.LineRendererPlayer;
            positionsSavedOnlyOnce = new List<Vector3>();
            transformTorsoPlayer = behaviourManager.TransformTorsoPlayer;
            
            // Si no hay comportamientos para unirse, se agrega el comportamiento (DriftBehaviour).
            if (listJoinGenericBehaviours.Count  == 0)
            {
                listJoinGenericBehaviours.Add(GetComponent<DriftBehaviour>());
            }
            // Se suscribe este comportamiento al controlador de comportamiento.
            behaviourManager.SubscribeBehaviour(this);
        }

        private void Update()
        {
            /*if (behaviourManager.ConditionGeneric())
            {
                SeeTimeRealLineParabolic();
            }*/
            
            // Se verifica si se presionó la tecla de cambio izquierdo.
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                // Se registra este comportamiento si no está registrado y no hay ninguna interpolación activa.
                if (!behaviourManager.IsCurrentBehaviour(listJoinGenericBehaviours[0].GetBehaviourCode()) 
                    && !behaviourManager.IsCurrentBehaviour(GetBehaviourCode()) 
                    && !behaviourManager.IsOverriding() 
                    && !sequenceInterpolation.IsActive())
                {
                    
                    behaviourManager.RegisterBehaviour(GetBehaviourCode());
                    
                }
                
            }
            
            
            
            
            
        }

        
        public override void LocalFixedUpdate()
        {
            
            // Si hay una interpolación activa, no se hace nada.
            if (sequenceInterpolation.IsActive())
            {
                return;
            }
            // Se realiza la interpolación.
            InterpolationShift();
            
        }

        
        public override void LocalLateUpdate()
        {
            // No se implementa en este comportamiento.
        }

        
        
        void InterpolationShift()
        {
            // Se quita el padre del torso del jugador si este comportamiento está activo.
            if (behaviourManager.IsCurrentBehaviour(GetBehaviourCode()))
            {
                SetParentTransformTorsoPlayer(false);
            }
            
            // Se calcula la trayectoria parabólica.
            ParabolicClass parabolic = new ParabolicClass(
                behaviourManager.GetCamScript.GetCurrentPitchCamera(),
                behaviourManager.GetH,
                behaviourManager.GetV,
                behaviourManager.calculationReferenceTransform,
                layerCollision
            );
            
            positionsSavedOnlyOnce = parabolic.GetListCalculated();
            behaviourManager.forwardGeneral = parabolic.ForwardSpecific;

            // Se activa la visualización de la trayectoria parabólica si está habilitada.
            if (showPlayerParabolicPath)
            {
                ActivePlayerParabolicPath();
            }
            
            // Se crea una secuencia de interpolación
            sequenceInterpolation = DOTween.Sequence();
            
            // Se mueve el torso del jugador a lo largo de la trayectoria calculada.
            foreach (var target in positionsSavedOnlyOnce)
            {
                
                moveTween = transformTorsoPlayer.DOMove(target, velocityInterpolation);
                
                sequenceInterpolation.Append(moveTween);
                
            }
            
            // Se detiene la interpolación al completarse.
            moveTween.OnComplete(() =>
            {
                
                moveTween.Kill();
                
            });
            
            sequenceInterpolation.OnComplete(() =>
            {
                Debug.Log("Finish InterpolationShift");
                
                StopSequenceTotal();
                
                
            });
            
            
        }
        
        // Método para detener la secuencia de interpolación por completo.
        void StopSequenceTotal()
        {
            
            if (sequenceInterpolation != null)
            {
                sequenceInterpolation.Kill();
                moveTween.Kill();
                sequenceInterpolation = null;
                
                // Se restablece el padre del torso del jugador si este comportamiento estaba sobrescrito.
                if (behaviourManager.IsOverriding(this))
                {
                    
                    SetParentTransformTorsoPlayer();
                }
                else
                {
                    // Se unen los comportamientos si este comportamiento esta registrado.
                    JoinBehaviours();
                }
                
                Debug.Log("Secuencia detenida Total Shift");
            }   
        }

        // Método para activar la visualización de la trayectoria parabólica del jugador.
        void ActivePlayerParabolicPath()
        {
            //lineRendererPlayer.material.color = Color.gray;
            lineRendererPlayer.transform.position = transformTorsoPlayer.position;
            
            lineRendererPlayer.positionCount = positionsSavedOnlyOnce.Count;
            
            for (int i = 0; i < positionsSavedOnlyOnce.Count; i++)
            {
                lineRendererPlayer.SetPosition(i,positionsSavedOnlyOnce[i]);
            }
        }
        void SeeTimeRealLineParabolic()
        {
            var transform1 = transform;
            Vector3 positionInitial = transform1.position;
            Quaternion rotationInitial = transform1.rotation;
            behaviourManager.LineRendererPlayer.transform.SetPositionAndRotation(positionInitial,rotationInitial);
            
            ParabolicClass parabolic = new ParabolicClass(
                behaviourManager.GetCamScript.GetCurrentPitchCamera(),
                behaviourManager.GetH,
                behaviourManager.GetV,
                behaviourManager.calculationReferenceTransform,
                layerCollision);
            
            List<Vector3> pointSee = parabolic.GetListCalculated();
            behaviourManager.LineRendererPlayer.positionCount = pointSee.Count;
            for (int i = 0; i < pointSee.Count; i++)
            {
                behaviourManager.LineRendererPlayer.SetPosition(i,pointSee[i]);
            }
            
        }
        
        
    }
}
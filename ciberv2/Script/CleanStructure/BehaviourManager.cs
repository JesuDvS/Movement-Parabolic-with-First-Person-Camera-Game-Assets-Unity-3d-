using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


namespace Script.Clean_Structure
{
    public class BehaviourManager : MonoBehaviour
    {
        //This refers to the player's torso which contains the player's main camera and other useful component objects.
        [SerializeField] private Transform transformTorsoPlayer;
        
        //Here reference is made to the line renderer that will behave as a projection of the parabolic movement
        //or use it as a trace of the player when performing the interpolation
        [SerializeField] private LineRenderer lineRendererPlayer;
        
        //Here reference is made to the object that has the same name "calculationReferenceTransform"
        //Serves as a starting point where the interpolation begins.
        public Transform calculationReferenceTransform;
        
        //Player's main camera.
        public Transform playerCamera;
        
        // List of behaviors
        public List<GenericBehaviour> behaviours; 
        // List of behaviors that overwrite
        public List<GenericBehaviour> overridingBehaviours;
        
        //This variable stores the code for the player's default behavior.
        public int defaultBehaviour;
        
        //This variable stores the code of the player's current behavior.
        public int currentBehaviour;
        
        //This variable stores the code of the player's blocked behavior.
        public int behaviourLocked;
        
        //Getter Rigidbody
        public Rigidbody GetRigidBody => rBody;
        
        //Getter TransformTorsoPlayer
        public Transform TransformTorsoPlayer
        {
            get => transformTorsoPlayer;
        }
        
        //Getter Axis Movement
        public float GetH => h;
        public float GetV => v;
        
        //Getter LineRendererPlayer
        public LineRenderer LineRendererPlayer => lineRendererPlayer;
        
        //Access Script FirstPersonCamera
        [HideInInspector] 
        public  FirstPersonCamera GetCamScript;
        
        //Forward of the player before each action.
        public Vector3 forwardGeneral;
        //PositionLocalInitialTorsoPlayer of theTorso player before each action.
        public Vector3 positionLocalInitialTorsoPlayer;
        //RotationLocalInitialTorsoPlayer of theTorso player before each action.
        public Quaternion rotationLocalInitialTorsoPlayer;
        //Queue which temporarily stores the completion of a behavior generic
        public Queue<GenericBehaviour> QueueBehaviourEnd;
        
        //Horizontal
        private float h;
        //Vertical
        private float v;
        
        // Player Rigidbody
        private Rigidbody rBody; 
        
        private void Awake()
        {
            // Variable initialization and assignment
            GetCamScript = playerCamera.GetComponent<FirstPersonCamera>();
            behaviours = new List<GenericBehaviour>();
            overridingBehaviours = new List<GenericBehaviour>();
            QueueBehaviourEnd = new Queue<GenericBehaviour>();
            rBody = GetComponent<Rigidbody>();
            
        }

        
        private void Update()
        {
            // Obtener la entrada del jugador
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");

            // Verificar si hay comportamientos en cola
            if (QueueBehaviourEnd.Count > 0 && IsCurrentBehaviour(defaultBehaviour))
            {
                // Sobrescribe el comportamiento en cola
                OverrideWithBehaviour(QueueBehaviourEnd.Dequeue());
            }
            
        }
        
        private void LateUpdate()
        {
            PerformBehaviourTypeExecute(StateExecute.LocalLateUpdate);
        }
        
        private void FixedUpdate()
        {
            PerformBehaviourTypeExecute(StateExecute.LocalFixedUpdate);
        }
        
        private void OnDrawGizmos()
        {
            if (overridingBehaviours == null || behaviours == null)
                return;
            
            if (!ConditionGeneric())
            {
                DrawCollisionPoints(overridingBehaviours);
                return;
            }
            DrawCollisionPoints(behaviours);
            
        }
        // Dibuja los puntos de colisión para los comportamientos añadido en listGenericBehaviour.
        private void DrawCollisionPoints(List<GenericBehaviour> listGenericBehaviour)
        {
            foreach (var behaviour in listGenericBehaviour)
            {
                if (behaviour.collisionPoints == null)
                    continue;

                foreach (var point in behaviour.collisionPoints)
                {
                    if (point == null)
                        continue;

                    Gizmos.color = behaviour.DetectCollision(point.position, point.localScale.x, behaviour.layerCollision) ? Color.green : Color.red;
                    Gizmos.DrawSphere(point.position, point.localScale.x);
                }
            }
        }
        
        

        #region Public Method
        // Suscribe un comportamiento genérico.
        public void SubscribeBehaviour(GenericBehaviour behaviour)
        {
            behaviours.Add(behaviour);
        }
        
        // Registra el comportamiento por defecto.
        public void RegisterDefaultBehaviour(int behaviourCode)
        {
            defaultBehaviour = behaviourCode;
            currentBehaviour = behaviourCode;
        }
        
        // Registra un comportamiento.
        public void RegisterBehaviour(int behaviourCode)
        {
            if (currentBehaviour == defaultBehaviour)
            {
                currentBehaviour = behaviourCode;
            }
        }
        
        // Anula el registro de un comportamiento.
        public void UnregisterBehaviour(int behaviourCode)
        {
            if (currentBehaviour == behaviourCode)
            {
                currentBehaviour = defaultBehaviour;
            }
        }

        // Sobrescribe el comportamiento actual con uno nuevo.
        public bool OverrideWithBehaviour(GenericBehaviour behaviour)
        {

            if (!overridingBehaviours.Contains(behaviour))
            {

                if (overridingBehaviours.Count == 0)
                {

                    foreach (GenericBehaviour overriddenBehaviour in behaviours)
                    {
                        if (overriddenBehaviour.isActiveAndEnabled && currentBehaviour == overriddenBehaviour.GetBehaviourCode())
                        {
                            overriddenBehaviour.OnOverride();
                            break;
                        }
                    }
                }

                overridingBehaviours.Add(behaviour);
                return true;
            }
            return false;
        }

        // Revoca la sobrescritura de un comportamiento.
        public bool RevokeOverridingBehaviour(GenericBehaviour behaviour)
        {
            if (overridingBehaviours.Contains(behaviour))
            {
                overridingBehaviours.Remove(behaviour);
                return true;
            }
            return false;
        }

        // Verifica si hay un comportamiento sobrescrito.
        public bool IsOverriding(GenericBehaviour behaviour = null)
        {
            if (behaviour == null)
                return overridingBehaviours.Count > 0;
            return overridingBehaviours.Contains(behaviour);
        }

        // Verifica si el comportamiento actual coincide con el código dado.
        public bool IsCurrentBehaviour(int behaviourCode)
        {
            return this.currentBehaviour == behaviourCode;
        }

        // Obtener si hay bloqueo temporal del comportamiento.
        public bool GetTempLockStatus(int behaviourCodeIgnoreSelf = 0)
        {
            return (behaviourLocked != 0 && behaviourLocked != behaviourCodeIgnoreSelf);
        }

        // Bloquea temporalmente un comportamiento.
        public void LockTempBehaviour(int behaviourCode)
        {
            if (behaviourLocked == 0)
            {
                behaviourLocked = behaviourCode;
            }
        }

        // Desbloquea un comportamiento bloqueado temporalmente.
        public void UnlockTempBehaviour(int behaviourCode)
        {
            if (behaviourLocked == behaviourCode)
            {
                behaviourLocked = 0;
            }
        }
        #endregion


        #region PerformBehaviourTypeExecute
        // Método para ejecutar los comportamientos según el tipo de ejecución (LateUpdate, FixedUpdate).
        void PerformBehaviourTypeExecute(StateExecute stateExecute)
        {
            bool isAnyOverridingBehaviourActive = false;
            if (ConditionGeneric())
            {
                foreach (GenericBehaviour behaviour in GetActiveBehaviours())
                {
                    behaviour.ExecuteMethodSameCondition(stateExecute);
                }
            }
            else
            {
                
                List<GenericBehaviour> overridingBehavioursCopy = new List<GenericBehaviour>(overridingBehaviours);

                foreach (GenericBehaviour behaviour in overridingBehavioursCopy)
                {
                    isAnyOverridingBehaviourActive = true;
                    behaviour.ExecuteMethodSameCondition(stateExecute);
                }
            }
            if (isAnyOverridingBehaviourActive && overridingBehaviours.Count > 0)
            {
                rBody.velocity = Vector3.zero;
            }
        }

        // Verifica si no hay un comportamiento bloqueado o hay comportamientos sobrescritos.
        public bool ConditionGeneric()
        {
            return behaviourLocked > 0 || overridingBehaviours.Count == 0;
        }

        // Obtiene los comportamientos activos.
        IEnumerable<GenericBehaviour> GetActiveBehaviours()
        {
            return behaviours.Where(behaviour => behaviour.isActiveAndEnabled
            && currentBehaviour == behaviour.GetBehaviourCode());
        }
        #endregion

        
    }

    
    // Clase abstracta para definir comportamientos genéricos
    public abstract class GenericBehaviour : MonoBehaviour
    {
        [Header("General parameters")]
        // Puntos de colisión para detección de obstáculos.
        public List<Transform> collisionPoints;
        
        // Capa de colisión para la detección de obstáculos.
        public LayerMask layerCollision;
        
        // Imagen sin procesar para el comportamiento.
        public RawImage rawImageBehaviour;
        
        // Booleano para distinguir si el comportamiento puede unirse con otros comportamientos de forma manual (Press Key)
        // o automatico.
        public bool isManualJoinBehaviours;
        
        // Lista de comportamientos genéricos a unir.
        public List<GenericBehaviour> listJoinGenericBehaviours;
        
        // Referencia al gestor de comportamientos.
        protected BehaviourManager behaviourManager;
        
        // Código único para el comportamiento.
        private int behaviourCode;
        
        
        private void Awake()
        {
            // Asignación de referencias y cálculo del código de comportamiento.
            behaviourManager = GetComponent<BehaviourManager>();
            behaviourCode = this.GetType().GetHashCode();
            
        }
        
        // Métodos virtuales que pueden ser extendidos por los comportamientos específicos.
        public virtual void LocalFixedUpdate() { }

        public virtual void LocalLateUpdate() { }

        public virtual void OnOverride() { }
        
        // Obtiene el código único del comportamiento.
        public int GetBehaviourCode()
        {
            return behaviourCode;
        }
        // Detecta colisiones utilizando una esfera.
        public bool DetectCollision(Vector3 punto,float radio,LayerMask layerCollision)
        {
            
            return Physics.CheckSphere(punto, radio,layerCollision);
        }
        // Establece el transform padre al torso del jugador.
        public bool SetParentTransformTorsoPlayer(bool valueDesired = true)
        {
            
            if (valueDesired)
            {
                var transform1 = behaviourManager.transform;
                transform1.position = behaviourManager.TransformTorsoPlayer.position;
                behaviourManager.TransformTorsoPlayer.SetParent(transform1);
                
                if (behaviourManager.IsOverriding(this))
                {
                    behaviourManager.RevokeOverridingBehaviour(this);
                }
                else
                {
                    behaviourManager.UnregisterBehaviour(GetBehaviourCode());
                }
                behaviourManager.TransformTorsoPlayer.SetLocalPositionAndRotation(behaviourManager.positionLocalInitialTorsoPlayer
                    ,behaviourManager.rotationLocalInitialTorsoPlayer);
                return true;
            }
            behaviourManager.forwardGeneral = behaviourManager.calculationReferenceTransform.forward;
            behaviourManager.TransformTorsoPlayer.GetLocalPositionAndRotation(out behaviourManager.positionLocalInitialTorsoPlayer
                , out behaviourManager.rotationLocalInitialTorsoPlayer);
            behaviourManager.TransformTorsoPlayer.SetParent(null);
            
            return false;
        }

        // Verifica si el torso del jugador tiene padre.
        public bool HasParentTransformTorsoPlayer()
        {
            return behaviourManager.TransformTorsoPlayer.parent;
        }
        // Une los comportamientos
        public bool JoinBehaviours(bool withUnregister = true)
        {
            if (listJoinGenericBehaviours ==  null)
            {
                return false;
            }

            if (withUnregister)
            {
                behaviourManager.UnregisterBehaviour(GetBehaviourCode());
            }
            
            foreach (var behaviour in listJoinGenericBehaviours)
            {
                behaviourManager.QueueBehaviourEnd.Enqueue(behaviour);
            }

            return true;
        }
        // Verifica si hay alguna colisión en alguno de los puntos de colisión
        protected bool IsAnyPointCollision()
        {
            
            if (collisionPoints == null)
            {
                return false;
            }
            Collider [] collision = null;
            foreach (var point in collisionPoints)
            {
                collision = Physics.OverlapSphere(point.position, point.localScale.x, layerCollision);
                if (collision.Length>0)
                {
                    return true;
                }
            }

            return false;
        }
        
        // Delegado para ejecutar métodos dependiendo del tipo de ejecución
        private Action sameConditionOfOrigin;
        // Ejecuta el método dependiendo del tipo de ejecución
        public void ExecuteMethodSameCondition(StateExecute stateExecute)
        {
            switch (stateExecute)
            {
                case StateExecute.LocalFixedUpdate: sameConditionOfOrigin = LocalFixedUpdate; break;
                case StateExecute.LocalLateUpdate: sameConditionOfOrigin = LocalLateUpdate; break;
            }
            sameConditionOfOrigin?.Invoke();
        }
        
    }
    // Enumeración para el tipo de ejecución
    public enum StateExecute
    {
        LocalFixedUpdate,
        LocalLateUpdate,
    }

}
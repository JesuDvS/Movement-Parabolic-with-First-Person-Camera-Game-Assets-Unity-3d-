using System;
using System.Collections.Generic;
using Script.Clean_Structure;
using UnityEngine;
using UnityEngine.UI;

namespace ciberv2.Script.CleanStructure
{
    public class HandleInterface : MonoBehaviour
    {
        
        public BehaviourManager behaviorManager;    // Reference to the BehaviorManager that manages the player's behaviors.
        
        private bool backDefault = false;           // Flag to check if the default color should be restored.
        
        private bool isCyan = false;                // Indicator to toggle between cyan and white colors.
        private void Update()
        {
            // Si hay comportamientos en la cola de comportamientos pendientes.
            if (behaviorManager.QueueBehaviourEnd.Count != 0)
            {
                // Se establece el color de todas las imágenes en blanco.
                foreach (var behavior in behaviorManager.behaviours)
                {
                    
                    behavior.rawImageBehaviour.color = Color.white;
                    
                }
                return;
            }
            // Si el comportamiento actual no es el comportamiento predeterminado.
            if (behaviorManager.currentBehaviour != behaviorManager.defaultBehaviour)
            {
                // Se alterna entre los colores rojo y cian para resaltar el comportamiento actual y sus comportamientos asociados.
                isCyan = Mathf.PingPong(Time.time * 4, 1) > 0.5f;
                foreach (var behavior in behaviorManager.behaviours)
                {
                    if (behaviorManager.IsCurrentBehaviour(behavior.GetBehaviourCode()))
                    {
                        behavior.rawImageBehaviour.color = Color.red;
                        // Si el comportamiento tiene comportamientos asociados y se unen manualmente.
                        if (behavior.isManualJoinBehaviours)
                        {
                            behavior.listJoinGenericBehaviours[0].rawImageBehaviour.color = isCyan ? Color.white : Color.cyan; // Alterna entre blanco y cian
                        }
                    }
                }
                backDefault = true;
            }
            else
            {
                // Si el comportamiento actual es el comportamiento predeterminado.
                if (backDefault == false)
                {
                    return;
                }
                // Se restaura el color predeterminado de todas las imágenes.
                foreach (var behavior in behaviorManager.behaviours)
                {
                    
                    behavior.rawImageBehaviour.color = Color.white;
                    
                }

                backDefault = false;
            }
            
            
        }
        
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace Script.Clean_Structure
{
    public class ParabolicClass
    {
        
        private float currentPitchCamera;                   // Current camera pitch.
        
        private Transform calculationReferenceTransform;    // Reference transformation for calculations.
        
        private float hAxis;                                // Horizontal input.
        
        private float vAxis;                                // Vertical input.
        
        private float angle, initialVelocity;               // Angle and initial speed.
        
        private float time = 10f;                           // Maximum trajectory time.
        
        private float amountCalculated = 0.01f;             // Increased time for calculations.
        
        private float g = 10;                               // Gravity.
        
        private float V0x, V0y, X0, Y0;                     // Initial speeds and positions.
        
        // DESIRED ANGLE, DESIRED SPEED, ACTUAL ANGLE
        private float[,] tablaValores = {                   // Table of values for angles and speeds
            { 35f, 4f, 48f },
            { 30f, 5f, 40f },
            { 15f, 10f, 32f },
            { 20f, 15f, 24f },
            { 10f, 27f, 16f },
            { 10f, 28f, 8f },
            { 10f, 29f, 4f },
            { 10f, 30f, 0f },
        };

        private Vector3 forwardSpecific;                            // Vector of forward specific
        
        public Vector3 ForwardSpecific => forwardSpecific;          // Access the forward vector

        private LayerMask layerCollision;                           // Collision layer

        //  Constructor of the ParabolicClass class.
        public ParabolicClass(float currentPitchCamera, 
            float hAxis, float vAxis, Transform calculationReferenceTransform, LayerMask layerCollision)
        {
            this.currentPitchCamera = currentPitchCamera;
            this.hAxis = hAxis;
            this.vAxis = vAxis;
            this.calculationReferenceTransform = calculationReferenceTransform;
            this.layerCollision = layerCollision;
        }
        
        // Calcula una lista de puntos que representan una trayectoria parabólica.
        public List<Vector3> GetListCalculated()
        {
            List<Vector3> listCalculate = new List<Vector3>();
            
            // Calcula el ángulo y la velocidad inicial basados en el ángulo de la cámara
            GetValuesForRealAngle(currentPitchCamera, out angle,out initialVelocity);
            
            // Calcula las velocidades iniciales en los ejes X e Y
            V0x = Mathf.Cos(Mathf.Deg2Rad * angle) * initialVelocity;
            V0y = Mathf.Sin(Mathf.Deg2Rad * angle) * initialVelocity;
            
            float angleDesired = 0;
            
            // Ajusta el ángulo deseado según las entradas del jugador
            if (hAxis>0)
            {
                angleDesired = 90;
            }else if (hAxis<0)
            {
                angleDesired = -90;
            }
            if (vAxis<0)
            {
                angleDesired = 180;
            }

            // Rota la transformación de referencia según el ángulo deseado
            calculationReferenceTransform.rotation *= Quaternion.Euler(0, angleDesired, 0);
            forwardSpecific = calculationReferenceTransform.forward;
            
            // Calcula la trayectoria parabólica
            for (float t = 0; t < time; t += amountCalculated)
            {
                
                float zBola = V0x * t + X0;
                
                float yBola = 0.5f * (-g) * Mathf.Pow(t, 2) + V0y * t + Y0;
                
                Vector3 point = new Vector3(0, yBola, zBola);
                
                Vector3 testVector3 = calculationReferenceTransform.TransformPoint(point);
                
                // Verifica si hay colisión en el punto calculado o si la altura es demasiado baja
                if (testVector3.y<=-1f || CheckCollisionInScene(testVector3))
                {
                    break;
                }
                
                listCalculate.Add(testVector3);
            }
            
            return listCalculate;
        }

        // Verifica si hay colisión en un punto dado.
        bool CheckCollisionInScene(Vector3 point)
        {
            float radio = 0.1f;             // Radio para la verificación de colisión
            return Physics.CheckSphere(point, radio,layerCollision);
        }
        
        // Obtiene los valores de ángulo y velocidad deseados basados en un ángulo real dado.
        private void GetValuesForRealAngle(float anguloReal, out float anguloDeseado, out float velocidadDeseada)
        {
            anguloDeseado = 0f;
            velocidadDeseada = 0f;

            float distanciaMinima = float.MaxValue;

            for (int i = 0; i < tablaValores.GetLength(0); i++)
            {
                float distanciaActual = Mathf.Abs(tablaValores[i, 2] - anguloReal);

                if (distanciaActual < distanciaMinima)
                {
                    distanciaMinima = distanciaActual;
                    anguloDeseado = tablaValores[i, 0];
                    velocidadDeseada = tablaValores[i, 1];
                }
            }
        }
    }
}
using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ciberv2.Script.CleanStructure
{
    public class GameManager : MonoBehaviour
    {
        private bool isEnterInButtonForRestart;

        private void Start()
        {
            DOTween.SetTweensCapacity(500, 50);
        }

        private void Update()
        {
            if (IsMouseClick() && !isEnterInButtonForRestart)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                
            }
            if (IsEscapeKeyPressed())
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        bool IsMouseClick()
        {
            return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1);
        }
        
        bool IsEscapeKeyPressed()
        {
            return Input.GetKeyDown("escape");
        }

        public void Restart()
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex);
        }

        public void IsEnterInButton(bool value)
        {
            isEnterInButtonForRestart = value;
        }
    
    }
}

using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Script.Clean_Structure
{
    public class EnvironmentPool : MonoBehaviour
    {
        public List<Transform> listEnv;
        public Transform transformInitialEnv;
        private Dictionary<Transform, Vector3> dictionaryEnv;
        private Sequence sequence; // Ahora inicializaremos esto
        private Tween tween;

        private void Awake()
        {
            dictionaryEnv = new Dictionary<Transform, Vector3>();
            sequence = DOTween.Sequence();

            foreach (Transform env in listEnv)
            {
                dictionaryEnv.Add(env, env.position);
                env.gameObject.SetActive(false);
                env.position = transformInitialEnv.position;
            }

            foreach (var env in listEnv)
            {
                tween = env.DOMove(dictionaryEnv[env], 5f).OnPlay(() =>
                {
                    env.gameObject.SetActive(true);
                });
                sequence.Append(tween);
            }

            
            sequence.OnComplete(() =>
            {
                foreach (var env in listEnv)
                {
                    env.DOMove(env.position + env.forward * 2f, 10f).SetEase(Ease.Linear).SetLoops(-1);
                }
            });
        }
    }
}
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Screamer.Transition
{
    public class BaseActivateTransition : VisualTransition
    {
        [SerializeField] private GameObject transitionObject;

        public override float Appear()
        {
            var transitionTime = HandleAppear();
            StartCoroutine(AppearCoroutine(transitionTime));

            return transitionTime;
        }

        protected virtual float HandleAppear() { return 0f; }

        public override float Disappear()
        {
            var transitionTime = HandleDisappear();
            StartCoroutine(DisappearCoroutine(transitionTime));

            return transitionTime;
        }

        protected virtual float HandleDisappear() { return 0f; }


        private IEnumerator AppearCoroutine(float time)
        {
            yield return new WaitForSeconds(time);
            transitionObject.SetActive(true);
        }

        private IEnumerator DisappearCoroutine(float time)
        {
            yield return new WaitForSeconds(time);
            transitionObject.SetActive(false);
        }
    }
}

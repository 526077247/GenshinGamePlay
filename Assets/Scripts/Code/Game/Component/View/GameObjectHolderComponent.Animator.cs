using UnityEngine;

namespace TaoTie
{
    public partial class GameObjectHolderComponent
    {
        public Animator Animator;

        public void SetWeight(int index, float weight)
        {
            if (Animator != null)
            {
                Animator.SetLayerWeight(index, weight);
            }
        }

        private void SetData(string key, int data)
        {
            if (Animator == null) return;
            Animator.SetInteger(key, data);
        }

        private void SetData(string key, float data)
        {
            if (Animator == null) return;
            Animator.SetFloat(key, data);
        }

        private void SetData(string key, bool data)
        {
            if (Animator == null) return;
            Animator.SetBool(key, data);
        }

        private void CrossFadeInFixedTime(int targetHash, float fadeDuration, int layerIndex, float targetTime)
        {
            if (Animator == null) return;
            Animator.CrossFadeInFixedTime(targetHash, fadeDuration, layerIndex, targetTime);
        }

        private void CrossFade(string stateName, int layerIndex)
        {
            if (Animator == null) return;
            Animator.CrossFade(stateName, 0, layerIndex);
        }
    }
}
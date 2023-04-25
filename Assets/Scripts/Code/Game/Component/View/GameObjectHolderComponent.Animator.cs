using UnityEngine;

namespace TaoTie
{
    public partial class GameObjectHolderComponent
    {
        private Animator animator;
        private FsmComponent fsm => parent.GetComponent<FsmComponent>();
        public void SetWeight(int index, float weight)
        {
            if (animator != null)
            {
                animator.SetLayerWeight(index, weight);
            }
        }

        private void UpdateMotionFlag(AIMoveSpeedLevel level)
        {
            fsm.SetData(FSMConst.MotionFlag,(int)level);
        }

        private void SetData(string key, int data)
        {
            if (animator == null) return;
            animator.SetInteger(key, data);
        }

        private void SetData(string key, float data)
        {
            if (animator == null) return;
            animator.SetFloat(key, data);
        }

        private void SetData(string key, bool data)
        {
            if (animator == null) return;
            animator.SetBool(key, data);
        }

        private void CrossFadeInFixedTime(int targetHash, float fadeDuration, int layerIndex, float targetTime)
        {
            if (animator == null) return;
            animator.CrossFadeInFixedTime(targetHash, fadeDuration, layerIndex, targetTime);
        }

        private void CrossFade(string stateName, int layerIndex)
        {
            if (animator == null) return;
            animator.CrossFade(stateName, 0, layerIndex);
        }
    }
}
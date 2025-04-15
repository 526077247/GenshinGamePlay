using UnityEngine;

namespace TaoTie
{
    public partial class GameObjectHolder
    {
        private bool fsmUseRagDoll;
        private bool useRagDoll;
        public Animator Animator;
        private FsmComponent fsm => parent.GetComponent<FsmComponent>();

        
        private void ChangeTimeScale(float scale)
        {
            if (Animator != null && Animator.runtimeAnimatorController != null)
            {
                Animator.speed = GameTimerManager.Instance.GetTimeScale();
            }
        }
        
        public void SetWeight(int index, float weight)
        {
            if (Animator != null)
            {
                Animator.SetLayerWeight(index, weight);
            }
        }

        private void UpdateMotionFlag(MotionFlag level)
        {
            if (fsm == null) return;
            fsm.SetData(FSMConst.MotionFlag, (int) level);
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

        private void SetData(string key, bool data, bool isTrigger)
        {
            if (Animator == null) return;
            if(!isTrigger) Animator.SetBool(key, data);
            else if(data) Animator.SetTrigger(key);
        }

        private void CrossFadeInFixedTime(string stateName, float fadeDuration, int layerIndex, float targetTime)
        {
            if (Animator == null) return;
            Animator.CrossFadeInFixedTime(stateName, fadeDuration, layerIndex, targetTime);
        }

        private void CrossFade(string stateName, int layerIndex)
        {
            if (Animator == null) return;
            Animator.CrossFade(stateName, 0, layerIndex);
        }

        private void FSMSetUseRagDoll(bool use)
        {
            if (fsmUseRagDoll != use)
            {
                fsmUseRagDoll = use;
                if (useRagDoll)
                {
                    UpdateRagDollState();
                }
            }
        }

        private void SetUseRagDoll(bool use)
        {
            if (useRagDoll != use)
            {
                useRagDoll = use;
                if (fsmUseRagDoll)
                {
                    UpdateRagDollState();
                }
            }
        }

        private void UpdateRagDollState()
        {
            var use = fsmUseRagDoll && useRagDoll;
            //todo:
        }
    }
}
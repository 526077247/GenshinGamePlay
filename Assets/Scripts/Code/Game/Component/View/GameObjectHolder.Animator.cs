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
        
        private void CheckLeftOrRightFoot()
        {
            if (Animator == null || EntityView == null) return;
            if (!Animator.avatar.isHuman) return;
            //判断是左脚还是右脚
            Transform leftFoot = Animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            Transform rightFoot = Animator.GetBoneTransform(HumanBodyBones.RightFoot);

            Vector3 leftFootLocalPos = EntityView.InverseTransformPoint(leftFoot.position);
            Vector3 rightFootLocalPos = EntityView.InverseTransformPoint(rightFoot.position);
        
            if (leftFootLocalPos.z > rightFootLocalPos.z)
            {
                Log.Info("左腿在前");
                fsm.SetData(FSMConst.LeftFoot, 1);
            }
            else
            {
                Log.Info("右腿在前");
                fsm.SetData(FSMConst.LeftFoot, 0);
            }
        }

        private void CheckAngleVF(Vector3 direction)
        {
            if (direction == Vector3.zero)
            {
                fsm.SetData(FSMConst.AngleVF, 0);
                return;
            }
            
            var faceDir = EntityView.forward;
            var viewDir = CameraManager.Instance.MainCamera().transform.forward;
            var dir = Quaternion.LookRotation(direction);
            viewDir = dir * viewDir;
            faceDir.y = 0;
            viewDir.y = 0;
            var angle = Vector3.Angle(faceDir, viewDir);
            if (angle < 45)
            {
                angle = 0;
            }
            else if (angle < 110)
            {
                angle = 90;
            }
            else if (angle < 160)
            {
                angle = 135;
            }
            else
            {
                angle = 180;
            }
            var isLeft = Vector3.Cross(faceDir, viewDir).y < 0;
            if (isLeft) angle = -angle;
            fsm.SetData(FSMConst.AngleVF, angle);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class UIAnimator: UIBaseContainer
    {
        private Animator animator;
        private Dictionary<string,AnimationClip> clips;
        public void SetEnable(bool enable)
        {
            ActivatingComponent();
            animator.enabled = enable;
        }
        void ActivatingComponent()
        {
            if (this.animator == null)
            {
                this.animator = this.GetGameObject().GetComponent<Animator>();
                if (this.animator == null)
                {
                    Log.Error($"添加UI侧组件UIAnimator时，物体{this.GetGameObject().name}上没有找到Animator组件");
                }
                else
                {
                    clips = new Dictionary<string, AnimationClip>();
                    if (this.animator.runtimeAnimatorController != null)
                    {
                        var items = this.animator.runtimeAnimatorController.animationClips;
                        for (int i = 0; i < items.Length; i++)
                        {
                            clips[items[i].name] = items[i];
                        }
                    }
                }
            }
        }

        public async ETTask Play(string name)
        {
            ActivatingComponent();
            this.animator.Play(name, 0, 0);
            if (!clips.TryGetValue(name, out var state))
            {
                return;
            }
            await TimerManager.Instance.WaitAsync((int)(state.length * 1000));
        }
        public void CrossFade(string name, float during = 0.5f)
        {
            ActivatingComponent();
            this.animator.CrossFade(name,during, 0);
        }
    }
}
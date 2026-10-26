using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// AnimatorMoveComponent 的 Mono 桥接组件。
    /// Unity 只在 Animator 所在 GameObject 上回调 OnAnimatorMove，
    /// 因此由 GameObjectHolder 保证其挂在 Animator 同节点；
    /// 仅负责转发回调，不承载任何物理/碰撞逻辑。
    /// </summary>
    public class AnimatorMoveBridge : MonoBehaviour
    {
        public System.Action OnAnimatorMoveEvt;

        void OnAnimatorMove()
        {
            OnAnimatorMoveEvt?.Invoke();
        }
    }
}

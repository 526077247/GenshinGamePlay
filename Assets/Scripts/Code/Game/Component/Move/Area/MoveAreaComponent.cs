using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 可移动区域组件：持有当前生效的可移动区域，供所有移动组件（SimpleMove/AnimatorMove 等）统一约束位置。
    /// </summary>
    public class MoveAreaComponent: Component, IComponent
    {
        /// <summary>当前可移动区域，null 表示不限制。</summary>
        public IMoveArea Area { get; private set; }

        public void Init()
        {
            Area = null;
        }

        public void Destroy()
        {
            Area = null;
        }

        /// <summary>
        /// 开始限制：设置可移动区域。
        /// </summary>
        public void BeginRestrict(IMoveArea area)
        {
            Area = area;
        }

        /// <summary>
        /// 结束限制：清除可移动区域。
        /// </summary>
        public void EndRestrict()
        {
            Area = null;
        }
    }
}
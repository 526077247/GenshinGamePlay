using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 标记HitBox类型
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class HitBoxComponent: MonoBehaviour
    {
        public HitBoxType HitBoxType;
    }
}
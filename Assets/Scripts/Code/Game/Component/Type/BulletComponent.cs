namespace TaoTie
{
    public class BulletComponent: Component,IComponent,IComponent<long>
    {
        public long CreateTime;

        /// <summary>
        /// 打出这颗子弹的实体 Id（射手）。
        ///
        /// 为什么不能只靠 <see cref="AttachComponent.ParentEntity"/>：
        /// 1. 那是弱引用、不参与攻击结算，子弹命中时攻击者可能被解析成子弹自己
        ///    （TriggerAttackEvent 直接拿 actionExecuter 当攻击者），此时「射手在哪」彻底丢失；
        /// 2. <see cref="RetreatType.ByOriginOwner"/> 需要的是<b>起点</b>，
        ///    而子弹自身的位置在命中瞬间就在受击者身上，方向会退化成零向量；
        /// 3. 后续仇恨归因、动量传递也要用同一个「谁打的」口径。
        /// 0 表示未知（老配置/非子弹链路），调用方需自行兜底。
        /// </summary>
        public long OwnerId;

        #region IComponent

        public void Init()
        {
            CreateTime = GameTimerManager.Instance.GetTimeNow();
        }
        public void Init(long time)
        {
            CreateTime = time;
        }
        public void Destroy()
        {
            CreateTime = default;
            OwnerId = default;
        }

        #endregion
    }
}

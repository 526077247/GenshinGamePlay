using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 角色体积代理持有者：体积代理集合、可阻挡/可移动标记。
    ///
    /// 与 <see cref="ActorMoveProfile"/> 的分工：那个是「墙/地形」用的扫掠半径（ModelRadius），
    /// 这个是「角色之间」互斥用的体积（BodyRadius / BodyProxies），两套口径独立，
    /// 否则大型单位会出现「墙按 0.7 判、人按体长判」的错位。
    ///
    /// 本组件目前只提供几何（代理圆 + 竖直区间），**不含速度/动量状态**：
    /// 角色互斥走「行进投影 + 重叠消解」两段纯位置方案，双方各自修正自己，
    /// 不写对方位置、不传递速度。<b>质量口径已废弃</b>：原先的 BodyMass 只被当成
    /// 「是否参与互斥」的布尔用，现已换成显式的 <see cref="CanBlock"/>；将来若做 P2
    /// 冲量传递，按 <see cref="ConfigHitImpulseResist"/> 的受力度分配，不再引入质量比。
    /// </summary>
    public class ActorBodyComponent : Component, IComponent
    {
        /// <summary>是否参与角色互斥（配置 CanBlock=false 或穿墙的单位不参与）。</summary>
        public bool CanBlock = true;

        /// <summary>是否可被推动 / 可被挤开。没有移动组件的单位（木桩）视为不可移动。</summary>
        public bool CanMove;

        /// <summary>
        /// 是否处于「被击飞」的空中状态。置位期间完全豁免角色互斥（不挡人、也不被挡）。
        ///
        /// 为什么需要它：代理的竖直区间是「实体脚下 + YBottom/YTop」。角色被击飞后 Position 抬高，
        /// 区间整体上移但底部仍从脚下算起，于是空中的人依旧与地面单位判定为交叠 —— 表现为
        /// 被击飞的人停在对方头顶上空、卡住不动。豁免只在击退带竖直分量时开启，普通跳跃不受影响。
        /// 由移动组件每物理步在着地状态确定后写入（<c>AnimatorMoveComponent.FixedUpdate</c>）。
        /// </summary>
        public bool Airborne;

        /// <summary>所有代理中的最大半径，供宽相位取 cell 尺寸。</summary>
        public float MaxProxyRadius { get; private set; } = 0.3f;

        /// <summary>所有代理中最大的水平偏移量，供宽相位把长条体型插入足够多的 cell。</summary>
        public float MaxProxyOffset { get; private set; }

        private ActorBodySystem system;
        private ConfigBodyProxy[] proxies;

        public Actor Actor => GetParent<Actor>();
        public ActorBodySystem System => system;
        public int ProxyCount => proxies?.Length ?? 0;
        public Vector3 WorldCenter => Actor != null ? Actor.Position : Vector3.zero;

        public void Init()
        {
            Actor actor = GetParent<Actor>();
            ConfigActorCommon common = actor?.ConfigActor?.Common;

            CanBlock = true;
            Airborne = false;
            proxies = null;

            if (common != null)
            {
                CanBlock = common.CanBlock;
                proxies = common.BodyProxies != null && common.BodyProxies.Length > 0
                    ? common.BodyProxies
                    : BuildSingleProxy(common);

                // 穿墙单位（CanPassWall）也不参与角色互斥，保持「能穿墙就能穿人」的一致语义
                ConfigMoveAgent agent = actor.ConfigActor.Move?.Agent;
                if ((agent is ConfigAnimatorMove am && am.CanPassWall) ||
                    (agent is ConfigSimpleMove sm && sm.CanPassWall))
                {
                    CanBlock = false;
                }
            }
            else
            {
                proxies = new[] { new ConfigBodyProxy { Radius = 0.3f, YBottom = 0f, YTop = 1.5f } };
            }

            MaxProxyRadius = 0.01f;
            MaxProxyOffset = 0f;
            for (int i = 0; i < proxies.Length; i++)
            {
                ConfigBodyProxy p = proxies[i];
                if (p == null) continue;
                MaxProxyRadius = Mathf.Max(MaxProxyRadius, p.Radius);
                MaxProxyOffset = Mathf.Max(MaxProxyOffset,
                    new Vector2(p.OffsetX, p.OffsetZ).magnitude);
            }

            // 必须在 CreateMoveComponent 之后挂载，否则 CanMove 恒为 false
            CanMove = GetComponent<MoveComponent>() != null;

            if (SceneManager.Instance != null && SceneManager.Instance.CurrentScene is SceneManagerProvider scene)
            {
                system = scene.GetManager<ActorBodySystem>();
                system?.Register(this);
            }
        }

        public void Destroy()
        {
            system?.Unregister(this);
            system = null;
            proxies = null;
            Airborne = false;
            CanBlock = false;
            CanMove = false;
        }

        private static ConfigBodyProxy[] BuildSingleProxy(ConfigActorCommon common)
        {
            return new[]
            {
                new ConfigBodyProxy
                {
                    OffsetX = 0f,
                    OffsetZ = 0f,
                    Radius = Mathf.Max(0.01f, common.BodyRadius),
                    YBottom = 0f,
                    YTop = Mathf.Max(0.01f, common.ModelHeight),
                }
            };
        }

        /// <summary>
        /// 取第 i 个代理的世界圆形（中心、半径、竖直区间）。偏移按实体 yaw 旋转，与动画无关。
        /// </summary>
        public bool GetProxyWorld(int index, out Vector3 center, out float radius, out float yMin, out float yMax)
        {
            center = Vector3.zero;
            radius = 0f;
            yMin = 0f;
            yMax = 0f;

            if (proxies == null || index < 0 || index >= proxies.Length) return false;
            Actor actor = Actor;
            if (actor == null) return false;

            ConfigBodyProxy p = proxies[index];
            if (p == null) return false;

            Vector3 basePos = actor.Position;
            float yaw = actor.Rotation.eulerAngles.y;
            Vector3 offset = Quaternion.Euler(0f, yaw, 0f) * new Vector3(p.OffsetX, 0f, p.OffsetZ);

            center = basePos + offset;
            radius = Mathf.Max(0.01f, p.Radius);
            yMin = basePos.y + p.YBottom;
            yMax = basePos.y + p.YTop;
            return true;
        }
    }
}

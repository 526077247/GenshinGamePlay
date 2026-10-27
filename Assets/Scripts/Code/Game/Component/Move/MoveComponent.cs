using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public abstract partial class MoveComponent: Component,IComponentDestroy,IUpdate
    {
        protected SceneEntity SceneEntity => parent as SceneEntity;
        
        public ConfigMoveAgent Config { get; private set; }
        public MoveInput CharacterInput{ get; private set; }
        /// <summary>
        /// 是否使用动画移动
        /// </summary>
        public abstract bool useAnimMove { get; }

        private const float WallSkin = 0.05f;
        private const float MaxDepenetrate = 2f;

        /// <summary>
        /// 墙扫掠起步重叠判定阈值：hit.distance 小于此值视为「起步时扫掠球已嵌在碰撞体内」，直接忽略。
        /// 为什么必须按 distance 过滤：扫掠球半径取 ModelRadius、球心在脚底+ModelHeight/2，
        /// 对矮胖体型（如 Fox：R=0.7/H=0.5）球底会永久嵌入地面碰撞体。Unity 对起步重叠的扫掠
        /// 一律返回 distance=0 + normal=−扫掠方向——假法线是水平的且与运动方向相反：
        /// 滑动循环先把期望位移沿该假法线整体投影掉（角色停止前进），再把 WallSkin 防粘连间隙
        /// （0.05m/帧 @50Hz = 2.5m/s）当作每帧位移沿假法线方向持续推出——正好是运动的反方向，
        /// 表现为播放前进动画却匀速后退（Fox 实测 out=(−0.2,+2.5)、posD=+0.05/帧，与本模型逐位吻合）。
        /// 起步重叠本由 ConstrainVelocity 前段的 ResolveEmbedding/GetEscapeDirection 负责
        /// （基于 ClosestPoint 的真实脱困方向，竖直重叠自动退化忽略），墙扫掠只该阻挡 distance>0 的面；
        /// 取最近的非重叠命中，墙在地面重叠命中之后也能正确检出。
        /// </summary>
        private const float WallHitMinDist = 1e-4f;

        /// <summary>
        /// 墙扫掠命中过滤器：排除起步重叠命中（distance≈0 的假法线命中），只保留 distance>0 的真实命中
        /// （静态委托，避免每帧分配）。
        /// </summary>
        private static readonly Func<RaycastHit, bool> WallHitFilter =
            h => h.distance > WallHitMinDist;

        private MoveStrategy strategy;
        protected ActorMoveProfile moveProfile;
        private int constraintWallLayer;
        protected bool HasMoveConstraint => moveProfile != null;

        protected void Init(ConfigMoveAgent configMove)
        {
            Config = configMove;
            CharacterInput = new MoveInput();
        }

        /// <summary>
        /// 初始化位移约束参数。由使用硬约束的移动组件在 InitInternal 中调用。
        /// </summary>
        protected void InitMoveConstraint(ActorMoveProfile profile, int wallLayer)
        {
            moveProfile = profile ?? new ActorMoveProfile();
            constraintWallLayer = wallLayer;
        }

        public virtual void Destroy()
        {
            strategy?.Dispose();
            strategy = null;
            CharacterInput = null;
            Config = null;
            moveProfile = null;
            constraintWallLayer = 0;
            hitVelocity = Vector3.zero;
            HitLaunched = false;
            hitImpulseUnsupportedLogged = false;
        }

        /// <summary>
        /// 强制朝向目标
        /// </summary>
        /// <param name="target"></param>
        public virtual void ForceLookAt(Vector3 target)
        {
            Vector3 dir = target - SceneEntity.Position;
            dir.y = 0;
            // dir 为零向量时直接返回，保持原朝向
            if (dir.sqrMagnitude < 0.0001f)
                return;
            SceneEntity.Rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
        /// <summary>
        /// 强制朝向方向
        /// </summary>
        /// <param name="dir"></param>
        public virtual void ForceLookTo(Vector3 dir)
        {
            if (CameraManager.Instance?.MainCamera() == null) return;
            var lookDir = CameraManager.Instance.MainCamera().transform.rotation * dir;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude < 0.0001f) return;
            SceneEntity.Rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        }

        public void Update()
        {
            if (strategy != null)
            {
                strategy?.Update();
            }

            if (strategy?.OverrideUpdate != true)
            {
                UpdateInternal();
            }
        }
        
        protected abstract void UpdateInternal();

        public void ChangeStrategy(ConfigMoveStrategy configMoveStrategy)
        {
            strategy?.Dispose();
            strategy = null;
            if (configMoveStrategy != null)
            {
                strategy = MoveSystem.Instance.CreateMoveStrategy(this, configMoveStrategy);
            }
        }
        
        public void ChangeStrategy<P1>(ConfigMoveStrategy configMoveStrategy,P1 p1)
        {
            strategy?.Dispose();
            strategy = null;
            if (configMoveStrategy != null)
            {
                strategy = MoveSystem.Instance.CreateMoveStrategy(this, configMoveStrategy, p1);
            }
        }

        /// <summary>
        /// 根据输入计算期望朝向
        /// </summary>
        protected Vector3 CalculateLookDirection()
        {
            if (CharacterInput == null || CharacterInput.FaceDirection == Vector3.zero)
                return Vector3.zero;

            Vector3 v = Vector3.zero;
            // 统一用世界竖直轴作投影/构造基准：SceneEntity.Rotation 契约恒为水平 yaw
            // （四足贴坡倾斜只落在 View 上），这里显式取世界轴避免任何倾斜污染。
            var up = Vector3.up;
            var faceRight = Quaternion.Euler(0, 90, 0) * CharacterInput.FaceDirection;
            v += Vector3.ProjectOnPlane(faceRight, up).normalized * CharacterInput.Direction.x;
            v += Vector3.ProjectOnPlane(CharacterInput.FaceDirection, up).normalized * CharacterInput.Direction.z;

            v.Normalize();
            return v;
        }

        /// <summary>
        /// 根据输入和转向速度执行旋转
        /// </summary>
        protected void HandleRotation(float deltaTime)
        {
            if (CharacterInput == null || CharacterInput.RotateSpeed <= 0) return;
            var lookDir = CalculateLookDirection();
            if (lookDir == Vector3.zero) return;

            var euler = SceneEntity.Rotation.eulerAngles;
            // 同 CalculateLookDirection：目标朝向基于世界竖直轴构造，只比较/写入 yaw 分量，
            // 不引入任何竖直系假设（实体朝向恒为水平，与 View 的贴坡倾斜解耦）。
            var dir = Quaternion.LookRotation(lookDir, Vector3.up);
            var euler2 = dir.eulerAngles;
            var angle = euler2.y - euler.y;
            while (angle < -180) angle += 360;
            while (angle > 180) angle -= 360;

            if (Mathf.Abs(angle) > CharacterInput.RotateSpeed * 0.01f)
            {
                var deltaAngle = CharacterInput.RotateSpeed * deltaTime * (angle < 0 ? -1 : 1);
                float newY = euler.y + (angle < 0 ? Mathf.Max(deltaAngle, angle) : Mathf.Min(deltaAngle, angle));
                SceneEntity.Rotation = Quaternion.Euler(euler.x, newY, euler.z);
            }
            else
            {
                SceneEntity.Rotation = Quaternion.Euler(euler.x, euler2.y, euler.z);
            }
        }

        /// <summary>
        /// 应用 ORCA 避障，返回处理后的速度
        /// </summary>
        protected Vector3 ApplyORCA(Vector3 velocity, float speed)
        {
            var orcaAgent = parent.GetComponent<ORCAAgentComponent>();
            orcaAgent?.SetVelocity(velocity, speed);
            if (orcaAgent != null)
            {
                velocity = orcaAgent.GetVelocity();
            }
            return velocity;
        }

        /// <summary>
        /// 解除墙体嵌入：实体中心进入障碍物超过 skin 间隙时，沿最近可脱离方向推出。
        /// </summary>
        protected void ResolveEmbedding()
        {
            float radius = moveProfile.Radius;
            float castHeight = Mathf.Max(0.05f, moveProfile.Height * 0.5f);

            Vector3 center = SceneEntity.Position + Vector3.up * castHeight;
            int len = PhysicsHelper.OverlapSphereNonAlloc(center, Mathf.Max(0f, radius - WallSkin),
                constraintWallLayer);
            if (len <= 0) return;

            Vector3 escape = Vector3.zero;
            float penetration = 0f;
            for (int i = 0; i < len; i++)
            {
                Collider col = PhysicsHelper.Colliders[i];
                if (col == null) continue;
                Vector3 dir = col.ClosestPoint(center) - center;
                dir.y = 0f;
                if (dir.sqrMagnitude < 1e-6f) continue;
                escape += dir.normalized;
                penetration = Mathf.Max(penetration, dir.magnitude);
            }

            if (escape.sqrMagnitude < 1e-6f) return;

            Vector3 push = escape.normalized * Mathf.Min(penetration + WallSkin, MaxDepenetrate);
            SceneEntity.Position += push;
        }

        /// <summary>
        /// 约束期望位移：先做墙体扫掠，再做地形/水面/坡度约束。调用方负责根据 CanPassWall 决定是否启用。
        /// </summary>
        protected Vector3 Constrain(Vector3 displacement, IMoveArea area)
        {
            displacement = ClampWallBlock(displacement);
            return ConstrainTerrain(displacement, area);
        }

        /// <summary>
        /// 只约束地形/水面/坡度，不做墙体扫掠。用于已由物理碰撞处理墙体的移动组件。
        /// </summary>
        protected Vector3 ConstrainTerrain(Vector3 displacement, IMoveArea area)
        {
            return ClampTerrainBlock(displacement, area);
        }

        /// <summary>
        /// 只约束墙体，不做地形/水面足印约束。
        /// 用于竖直运动期间（跳跃/下落/击退）：足印可走性判定基于站立位置，
        /// 起点位于地形上方时 IsBlockedAt 会误判为不可走，且脱困探测只覆盖水平方向，
        /// 会把含竖直分量的位移整体清零，导致角色悬停在空中。
        /// </summary>
        protected Vector3 ConstrainWallOnly(Vector3 displacement)
        {
            if (!HasMoveConstraint || displacement == Vector3.zero) return displacement;
            return ClampWallBlock(displacement);
        }

        private Vector3 ClampWallBlock(Vector3 displacement)
        {
            if (displacement == Vector3.zero) return displacement;
            Vector3 flat = displacement;
            flat.y = 0;
            float flatLen = flat.magnitude;
            if (flatLen <= 1e-4f) return displacement;

            float radius = moveProfile.Radius;
            float castHeight = Mathf.Max(0.05f, moveProfile.Height * 0.5f);

            const float maxSegment = 1f;
            const int maxIterations = 32;

            Vector3 origin = SceneEntity.Position + Vector3.up * castHeight;

            Vector3 escapeDir = GetEscapeDirection(origin, radius, WallSkin, constraintWallLayer);
            if (escapeDir != Vector3.zero)
            {
                float into = Mathf.Min(Vector3.Dot(flat, escapeDir), 0f);
                flat -= escapeDir * into;
                return new Vector3(flat.x, displacement.y, flat.z);
            }

            Vector3 rem = flat;
            Vector3 moved = Vector3.zero;

            for (int iter = 0; iter < maxIterations && rem.sqrMagnitude > 1e-8f; iter++)
            {
                Vector3 flatDir = rem.normalized;
                float segLen = Mathf.Min(rem.magnitude, maxSegment);
                if (PhysicsHelper.SphereCastNonAlloc(origin, radius, flatDir, out RaycastHit hit, segLen,
                        constraintWallLayer, QueryTriggerInteraction.Ignore, null, WallHitFilter))
                {
                    float step = Mathf.Max(hit.distance - WallSkin, 0f);
                    moved += flatDir * step;
                    rem -= flatDir * step;
                    origin += flatDir * step;

                    Vector3 wallNormal = hit.normal;
                    wallNormal.y = 0f;
                    if (wallNormal.sqrMagnitude < 1e-6f)
                        break;
                    wallNormal.Normalize();
                    float into = Vector3.Dot(rem, wallNormal);
                    if (into < 0f)
                        rem -= wallNormal * into;
                    if (rem.sqrMagnitude < flat.sqrMagnitude * 0.03f)
                        rem = Vector3.zero;
                    float gap = Mathf.Max(WallSkin - hit.distance, 0f);
                    if (rem.sqrMagnitude <= 1e-8f)
                        moved += wallNormal * gap;
                    origin += wallNormal * (gap + WallSkin);
                    continue;
                }

                moved += flatDir * segLen;
                rem -= flatDir * segLen;
                origin += flatDir * segLen;
            }

            return new Vector3(moved.x, displacement.y, moved.z);
        }

        private Vector3 GetEscapeDirection(Vector3 center, float radius, float skin, int layerMask)
        {
            int len = PhysicsHelper.OverlapSphereNonAlloc(center, Mathf.Max(0f, radius - skin), layerMask);
            if (len <= 0) return Vector3.zero;

            Vector3 escape = Vector3.zero;
            for (int i = 0; i < len; i++)
            {
                Collider col = PhysicsHelper.Colliders[i];
                if (col == null) continue;
                Vector3 dir = col.ClosestPoint(center) - center;
                dir.y = 0f;
                if (dir.sqrMagnitude < 1e-6f) continue;
                escape += dir.normalized;
            }

            if (escape.sqrMagnitude < 1e-6f) return Vector3.zero;
            return escape.normalized;
        }

        private Vector3 ClampTerrainBlock(Vector3 displacement, IMoveArea area)
        {
            float radius = moveProfile.Radius;
            if (TerrainQueryManager.Instance == null) return displacement;

            Vector3 start = SceneEntity.Position;

            if (IsBlockedAt(start, 0f))
            {
                Vector3 escape = EstimateFreeDirection(start);
                if (escape == Vector3.zero) return Vector3.zero;
                float along = Vector3.Dot(displacement, escape);
                if (along <= 0f) return Vector3.zero;
                Vector3 escapeStep = escape * Mathf.Min(along, displacement.magnitude);
                if (area != null) escapeStep = area.ClampPosition(start + escapeStep) - start;
                escapeStep.y = displacement.y;
                return escapeStep;
            }

            Vector3 target = start + displacement;
            if (area != null) target = area.ClampPosition(target);
            if (!IsBlockedAt(target, radius)) return target - start;

            Vector3 rotated = TryRotatedStep(start, displacement, radius, area);
            if (rotated.sqrMagnitude > 1e-8f) return rotated;

            if (!IsBlockedAt(target, 0f)) return target - start;
            return ClampCenterOnly(start, target, displacement, area);
        }

        /// <summary>
        /// 位置是否被阻挡：水面用当前生效的精确/近似水面查询，坡度用地形足印采样。
        /// 半径 0 表示只判中心点，用于脱困与狭窄通道兜底。
        /// </summary>
        private bool IsBlockedAt(Vector3 position, float radius)
        {
            TerrainQueryManager manager = TerrainQueryManager.Instance;
            if (manager != null &&
                manager.TryOverlapsWater(position, radius, position.y, moveProfile.Height,
                    out bool overlapsWater))
            {
                if (overlapsWater) return true;
                return !IsFootprintWalkable(position, radius, false);
            }

            return !IsFootprintWalkable(position, radius, true);
        }

        /// <summary>
        /// 判断中心点及圆周采样点是否都位于可行走地面。
        /// 没有地形查询服务时视为可通行；查询不到地形时视为不可通行。
        /// checkWater 为 false 时只检查坡度，水域由统一水面查询负责。
        /// </summary>
        private bool IsFootprintWalkable(Vector3 position, float radius, bool checkWater)
        {
            TerrainQueryManager manager = TerrainQueryManager.Instance;
            if (manager == null) return true;
            if (!manager.TrySample(position, out TerrainSample sample)) return true;
            if (checkWater ? !sample.IsWalkableGround : !sample.Valid || !sample.Walkable)
                return false;
            if (radius <= 0f) return true;

            int samples = moveProfile.FootprintSamples;
            if (samples <= 0) samples = ActorMoveProfile.DefaultFootprintSamples;
            for (int i = 0; i < samples; i++)
            {
                float angle = i * (Mathf.PI * 2f / samples);
                Vector3 p = position +
                            new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                if (!manager.TrySample(p, out TerrainSample ring)) return false;
                if (checkWater ? !ring.IsWalkableGround : !ring.Valid || !ring.Walkable)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 以原始位移为基准逐步向左/右旋转，返回第一个足印不撞障碍的完整步长位移。
        /// 位移长度保持不变，因此贴边滑行时不会损失沿障碍边界的移动速度。
        /// </summary>
        private Vector3 TryRotatedStep(Vector3 start, Vector3 displacement, float radius,
            IMoveArea area)
        {
            for (int i = 1; i <= moveProfile.SlideAngleSteps; i++)
            {
                float angle = i * moveProfile.SlideAngleStep;
                for (int side = 0; side < 2; side++)
                {
                    float signedAngle = side == 0 ? angle : -angle;
                    Vector3 rotated = Quaternion.Euler(0f, signedAngle, 0f) * displacement;
                    Vector3 candidate = start + rotated;
                    if (area != null) candidate = area.ClampPosition(candidate);
                    if (!IsBlockedAt(candidate, radius)) return candidate - start;
                }
            }

            return Vector3.zero;
        }

        /// <summary>
        /// 仅中心点约束：只禁止中心进入水面/陡坡，不要求整个足印可走。
        /// 用于狭窄通道或足印已经压到障碍、旋转也无法产生位移时的兜底，避免永久卡死。
        /// </summary>
        private Vector3 ClampCenterOnly(Vector3 start, Vector3 target,
            Vector3 displacement, IMoveArea area)
        {
            if (!IsBlockedAt(target, 0f)) return target - start;

            Vector3 flat = displacement;
            flat.y = 0f;
            float length = flat.magnitude;
            if (length <= 1e-4f) return Vector3.zero;

            Vector3 dir = flat / length;
            float lo = 0f, hi = 1f;
            for (int i = 0; i < moveProfile.SlideBinaryIterations; i++)
            {
                float mid = (lo + hi) * 0.5f;
                Vector3 p = start + dir * (length * mid);
                if (area != null) p = area.ClampPosition(p);
                if (!IsBlockedAt(p, 0f))
                {
                    lo = mid;
                }
                else
                {
                    hi = mid;
                }
            }

            Vector3 point = start + dir * (length * lo);
            if (area != null) point = area.ClampPosition(point);
            point.y = start.y + displacement.y * lo;
            return point - start;
        }

        /// <summary>
        /// 从 <paramref name="from"/> 向四周探测，返回指向最近中心点自由空间的方向（用于脱困）。
        /// 找不到自由方向时返回 <see cref="Vector3.zero"/>。
        /// </summary>
        private Vector3 EstimateFreeDirection(Vector3 from)
        {
            Vector3 best = Vector3.zero;
            float bestDistance = float.MaxValue;
            int probeCount = moveProfile.NormalProbeCount;
            float probeDistance = moveProfile.EscapeProbeDistance;

            for (int i = 0; i < probeCount; i++)
            {
                float angle = i * (Mathf.PI * 2f / probeCount);
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                if (IsBlockedAt(from + offset * probeDistance, 0f)) continue;

                // 二分找到该方向上第一个中心点可走的位置，取距离最近的方向。
                float lo = 0f, hi = 1f;
                for (int j = 0; j < 3; j++)
                {
                    float mid = (lo + hi) * 0.5f;
                    if (!IsBlockedAt(from + offset * (probeDistance * mid), 0f))
                    {
                        hi = mid;
                    }
                    else
                    {
                        lo = mid;
                    }
                }

                float distance = probeDistance * hi;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = offset;
                }
            }

            return best;
        }

        #region 击退冲量（外部输入 → 速度）

        /// <summary>
        /// 击退速度（世界系）。AnimatorMove 含竖直分量，SimpleMove 只吃水平分量。
        /// 由 <see cref="ConsumeHitImpulse"/> 从 <see cref="MoveInput.HitImpulse"/> 汇入，逐物理步线性衰减。
        /// </summary>
        protected Vector3 hitVelocity;

        /// <summary>
        /// 本次击退是否把自身打离地面（击飞）。空中期间豁免角色互斥：
        /// <see cref="ActorBodyComponent.GetProxyWorld"/> 的竖直区间跟着 Position 上移，
        /// 被击飞的身体仍会与地面单位交叠 —— 不解耦就会「停在别人头顶上空被顶住」。
        /// 只由 <see cref="ConsumeHitImpulse"/> 在击退带向上分量时置位（普通跳跃不受影响），
        /// 着地后由移动组件清除。
        /// </summary>
        protected bool HitLaunched;

        /// <summary>
        /// 判定「被击飞」的竖直速度阈值(u/s)。低于此值视为抬升已结束。
        /// 取 0.01 而不是 0：线性衰减到零前的最后几帧速度极小，没必要为它继续判空中。
        /// </summary>
        protected const float HitImpulseAirborneEps = 0.01f;

        /// <summary>冲量 → 速度的倍率。0 表示该移动方式不消费击退（由子类按各自配置覆盖）。</summary>
        protected virtual float HitImpulseToSpeed => 0f;

        /// <summary>击退速度的线性衰减速率(u/s)。位移距离 ≈ v²/(2*衰减)。</summary>
        protected virtual float HitImpulseDecayRate => 50f;

        private Actor HitActor => GetParent<Actor>();

        /// <summary>「本移动方式不支持击退」的告警只喊一次，否则每次挨打都刷。</summary>
        private bool hitImpulseUnsupportedLogged;

        /// <summary>
        /// 外部击退冲量的<b>唯一入口</b>。攻击侧只负责算「期望冲量」，
        /// 抗性缩放、是否抬离地面、由哪种移动方式消费，都在受击方这里决定。
        ///
        /// 为什么必须统一入口，而不是各处直写 <see cref="MoveInput.HitImpulse"/>：
        /// 1. 抗性表按 <see cref="ConfigHitPattern.HitImpulseType"/> 查的是<b>受击方</b>配置（同一招打史莱姆和打石像该有不同反应），
        ///    只有受击方自己拿得到；
        /// 2. <see cref="SimpleMoveComponent"/> 原先根本没有击退通道，直写字段会被静默丢弃；
        /// 3. 后续的角色互斥冲量传递（撞人时把速度投递给对方）要复用同一条路径，
        ///    保证「被技能打」和「被身体撞」的落地口径一致。
        /// </summary>
        /// <param name="impulse">世界系冲量：XZ 水平方向，y 为抬升量（语义见 ConfigHitPattern）</param>
        /// <param name="type">击退分类标签，用于查抗性表，可为空</param>
        public void ApplyHitImpulse(Vector3 impulse, string type)
        {
            if (CharacterInput == null || impulse == Vector3.zero) return;

            // 先查抗性：免疫（Scale=0）是策划的明确意图，不该再报「没有击退通道」的警。
            float scale = ResolveHitImpulseResist(type);
            if (scale <= 0f) return;

            if (HitImpulseToSpeed <= 0f)
            {
                // 没有击退通道时静默吞掉，是「配了击退却没反应」最典型的误判来源，
                // 所以这里明确告警一次并指出是哪种移动方式（目前只有刚体移动没接）。
                if (!hitImpulseUnsupportedLogged)
                {
                    hitImpulseUnsupportedLogged = true;
                    Log.Warning($"实体 {HitActor?.Id} 的移动方式 {GetType().Name} 未实现击退通道(HitImpulseToSpeed=0)，冲量被丢弃");
                }
                return;
            }

            CharacterInput.HitImpulse += impulse * scale;
        }

        /// <summary>
        /// 查受击方抗性表：先精确匹配类型标签，未命中取 Type 为空的兜底项，都没有则为 1（不缩放）。
        /// </summary>
        private float ResolveHitImpulseResist(string type)
        {
            ConfigHitImpulseResist[] table = HitActor?.ConfigActor?.Common?.HitImpulseResists;
            if (table == null || table.Length == 0) return 1f;

            float fallback = -1f;
            for (int i = 0; i < table.Length; i++)
            {
                ConfigHitImpulseResist entry = table[i];
                if (entry == null) continue;
                if (string.IsNullOrEmpty(entry.Type))
                {
                    if (fallback < 0f) fallback = entry.Scale;
                    continue;
                }
                if (entry.Type == type) return Mathf.Clamp(entry.Scale, 0f, 3f);
            }

            return fallback >= 0f ? Mathf.Clamp(fallback, 0f, 3f) : 1f;
        }

        /// <summary>
        /// 把本帧累积的 <see cref="MoveInput.HitImpulse"/> 收进 hitVelocity 并清空输入。
        /// 子类必须在各自物理步的<b>最前面</b>调用（尤其是 AnimatorMove：CheckGround 要靠它判断是否被击飞）。
        ///
        /// 「是否击飞」在这里判定而不是在 <see cref="ApplyHitImpulse"/>：
        /// 冲量可能在物理步之外投递，若那时就置位，紧接着的 SyncBodyAirborne（那时还没消费冲量、
        /// isGrounded 仍为 true）会把标记当场清掉，整次击飞都拿不到互斥豁免。
        /// </summary>
        protected void ConsumeHitImpulse()
        {
            if (CharacterInput == null) return;
            if (CharacterInput.HitImpulse.sqrMagnitude <= 1e-6f) return;

            hitVelocity += CharacterInput.HitImpulse * HitImpulseToSpeed;
            CharacterInput.HitImpulse = Vector3.zero;

            if (hitVelocity.y > HitImpulseAirborneEps) HitLaunched = true;
        }

        protected void DecayHitVelocity(float dt)
        {
            DecayVector(ref hitVelocity, HitImpulseDecayRate, dt);
        }

        /// <summary>线性衰减（不是指数）：|v| -= decay*dt，到零截断。</summary>
        protected static void DecayVector(ref Vector3 v, float decay, float dt)
        {
            if (v == Vector3.zero || decay <= 0f) return;
            float mag = v.magnitude;
            float step = decay * dt;
            if (step >= mag)
            {
                v = Vector3.zero;
                return;
            }
            v -= v.normalized * step;
        }

        #endregion

        #region 角色间互斥（体积代理，纯几何）

        /// <summary>
        /// 接触判定余量。<b>行进投影与重叠消解必须共用这一个边界</b>（r1+r2+ActorSkin）：
        /// 若一边停在 skin、另一边推到 slop，位置就会在两个目标之间逐帧来回，肉眼即为碰撞时闪烁。
        /// </summary>
        private const float ActorSkin = 0.02f;
        /// <summary>单个物理步内所有接触合计的最大修正量（米），防止出生重叠时被弹飞。</summary>
        private const float ActorMaxCorrection = 0.12f;
        /// <summary>滑移分段的单步长度，避免高速位移从细小代理之间穿过。</summary>
        private const float ActorMaxStep = 0.2f;

        // 求解器缓冲（主线程顺序执行，复用避免每帧分配）
        private static readonly List<ActorBodyComponent> s_actorNeighbors = new List<ActorBodyComponent>(16);
        private static readonly List<Vector3> s_actorContactNormals = new List<Vector3>(8);
        private static readonly List<float> s_actorContactDepths = new List<float>(8);

        protected ActorBodyComponent Body => parent.GetComponent<ActorBodyComponent>();

        /// <summary>
        /// 本物理步角色互斥是否改动了水平位置（投影收窄位移 或 重叠消解写回位置）。
        /// 供移动组件决定「是否需要在末尾重新采样地面高度」：互斥只改 XZ，
        /// 斜坡上新的 XZ 对应不同的地面高度，不重采样就会脚底低于坡面
        /// （脚 IK 被迫压短腿 → 肉体表现为半蹲 + 脚陷入地下）。
        /// </summary>
        protected bool ActorContactMoved;

        private static bool VerticalOverlap(float aMin, float aMax, float bMin, float bMax)
        {
            return aMin < bMax && bMin < aMax;
        }

        /// <summary>
        /// 行进前的角色阻挡：沿期望位移推进，遇到对方代理就把"朝对方"的分量投影掉（保留切向 → 贴人滑走）。
        /// 与墙扫掠同一套思路，但不改竖直分量。
        /// </summary>
        protected Vector3 ConstrainActor(Vector3 displacement)
        {
            ActorBodyComponent self = Body;
            if (self == null || !self.CanBlock || self.ProxyCount == 0) return displacement;

            // 被击飞期间完全豁免角色互斥（详见 HitLaunched）：
            // 身体已离地，竖直区间随 Position 上移后仍与地面单位交叠，继续求解会把角色顶在半空。
            if (self.Airborne) return displacement;

            Vector3 flat = new Vector3(displacement.x, 0f, displacement.z);
            float len = flat.magnitude;
            if (len <= 1e-4f) return displacement;

            ActorBodySystem system = self.System;
            if (system == null || !system.Enabled) return displacement;

            int steps = Mathf.Clamp(Mathf.CeilToInt(len / ActorMaxStep), 1, 4);
            Vector3 step = flat / steps;
            Vector3 start = SceneEntity.Position;
            Vector3 cursor = start;
            float queryRadius = self.MaxProxyOffset + self.MaxProxyRadius + system.MaxBodyRadius + system.MaxBodyOffset + ActorSkin;

            for (int s = 0; s < steps; s++)
            {
                // 两次投影迭代：夹在两腿之间时两条法线互相修正才能收敛
                for (int pass = 0; pass < 2; pass++)
                {
                    bool hit = false;
                    int n = system.QueryNeighbors(cursor + step, queryRadius, s_actorNeighbors);
                    for (int i = 0; i < n; i++)
                    {
                        ActorBodyComponent other = s_actorNeighbors[i];
                        if (other == null || other == self || other.IsDispose) continue;
                        if (!other.CanBlock || other.ProxyCount == 0) continue;
                        // 对方在空中（被击飞）：不挡人，也不接受推送
                        if (other.Airborne) continue;

                        for (int sp = 0; sp < self.ProxyCount; sp++)
                        {
                            if (!self.GetProxyWorld(sp, out Vector3 cSelf, out float rSelf, out float sMin, out float sMax))
                                continue;
                            // 子步推进后代理中心也要跟着前移（cursor 已含前面子步的实际位移）
                            cSelf += step + (cursor - start);

                            for (int op = 0; op < other.ProxyCount; op++)
                            {
                                if (!other.GetProxyWorld(op, out Vector3 cOther, out float rOther, out float oMin, out float oMax))
                                    continue;
                                if (!VerticalOverlap(sMin, sMax, oMin, oMax)) continue;

                                Vector3 d = cSelf - cOther;
                                d.y = 0f;
                                float dist = d.magnitude;
                                float rSum = rSelf + rOther + ActorSkin;
                                if (dist >= rSum) continue;

                                Vector3 normal = dist > 1e-4f ? d / dist : -flat.normalized;
                                float into = Vector3.Dot(step, normal);
                                if (into < 0f)
                                {
                                    step -= normal * into;
                                    hit = true;
                                }
                            }
                        }
                    }
                    if (!hit) break;
                }
                cursor += step;
            }

            Vector3 result = new Vector3(cursor.x - start.x, displacement.y, cursor.z - start.z);
            // 位移被收窄 = 本步发生过角色阻挡（供调用方判断是否需要重采样地面高度）
            float dx = result.x - flat.x;
            float dz = result.z - flat.z;
            if (dx * dx + dz * dz > 1e-8f) ActorContactMoved = true;
            return result;
        }

        /// <summary>
        /// 应用位移后的重叠消解（纯几何，无动量）。
        ///
        /// 三条约束都是从「碰撞时闪烁」换来的，改动前请先读：
        /// 1. 边界与 <see cref="ConstrainActor"/> 完全一致（r1+r2+ActorSkin）。
        ///    两者若用不同阈值，贴住时一个想把脚停在 skin、另一个想推到更外侧，
        ///    位置就会在两个目标之间逐帧来回，肉眼就是闪烁。
        /// 2. 只修正自己，绝不写 other.Actor.Position。
        ///    双方各自在自己的物理步里修正自己；若同一物理步里互相推，会被推两次而超调，
        ///    表现为「弹开 → 下一帧又被 root motion 拉近」的高频振荡。
        ///    承担比例：对方可移动则各担一半；对方不可移动（木桩）则自己全担。
        /// 3. 修正后若落进不可走区域（水面/坡度），撤销本次修正。
        ///    否则表现为「被推进墙里 → 下一帧脱困弹出」的循环。
        /// </summary>
        protected void ResolveActorContacts()
        {
            ActorBodyComponent self = Body;
            if (self == null || !self.CanBlock || self.ProxyCount == 0) return;

            // 与 ConstrainActor 同一口径：击飞期间不参与互斥
            if (self.Airborne) return;

            ActorBodySystem system = self.System;
            if (system == null || !system.Enabled) return;

            s_actorContactNormals.Clear();
            s_actorContactDepths.Clear();

            float queryRadius = self.MaxProxyOffset + self.MaxProxyRadius + system.MaxBodyRadius + system.MaxBodyOffset;
            int n = system.QueryNeighbors(SceneEntity.Position, queryRadius, s_actorNeighbors);

            // 每个对方只取「最深的一对代理」，避免 4 腿等多代理重复施加修正
            for (int i = 0; i < n; i++)
            {
                ActorBodyComponent other = s_actorNeighbors[i];
                if (other == null || other == self || other.IsDispose) continue;
                if (!other.CanBlock || other.ProxyCount == 0) continue;
                // 对方在空中（被击飞）：不参与位置消解
                if (other.Airborne) continue;

                float bestDepth = 0f;
                Vector3 bestNormal = Vector3.zero;

                for (int sp = 0; sp < self.ProxyCount; sp++)
                {
                    if (!self.GetProxyWorld(sp, out Vector3 cSelf, out float rSelf, out float sMin, out float sMax))
                        continue;

                    for (int op = 0; op < other.ProxyCount; op++)
                    {
                        if (!other.GetProxyWorld(op, out Vector3 cOther, out float rOther, out float oMin, out float oMax))
                            continue;
                        if (!VerticalOverlap(sMin, sMax, oMin, oMax)) continue;

                        Vector3 d = cSelf - cOther;
                        d.y = 0f;
                        float dist = d.magnitude;
                        // 与 ConstrainActor 同一边界：贴住时 depth <= 0，不产生任何修正
                        float depth = rSelf + rOther + ActorSkin - dist;
                        if (depth <= bestDepth) continue;

                        bestDepth = depth;
                        bestNormal = dist > 1e-4f ? d / dist : Vector3.zero;
                    }
                }

                if (bestDepth <= 0f) continue;

                if (bestNormal == Vector3.zero)
                {
                    Vector3 fallback = SceneEntity.Position - other.WorldCenter;
                    fallback.y = 0f;
                    bestNormal = fallback.sqrMagnitude > 1e-6f ? fallback.normalized : Vector3.forward;
                }

                // 不可移动的一方（木桩）不会自己修正，由可移动方全额承担
                float share = other.CanMove ? 0.5f : 1f;

                s_actorContactNormals.Add(bestNormal);
                s_actorContactDepths.Add(bestDepth * share);
            }

            int count = s_actorContactNormals.Count;
            if (count == 0) return;

            Vector3 correction = Vector3.zero;
            for (int i = 0; i < count; i++)
            {
                correction += s_actorContactNormals[i] * s_actorContactDepths[i];
            }
            correction.y = 0f;

            float mag = correction.magnitude;
            if (mag <= 1e-5f) return;
            if (mag > ActorMaxCorrection) correction *= ActorMaxCorrection / mag;

            Vector3 from = SceneEntity.Position;
            Vector3 to = from + correction;

            // 不允许把自己推进不可走区域（水面/陡坡）。原本就在里面时不拦，避免卡死。
            // 穿墙单位（CanPassWall）在 ActorBodyComponent 里已令 CanBlock=false，不会走到这里。
            if (HasMoveConstraint &&
                IsBlockedAt(to, 0f) && !IsBlockedAt(from, 0f))
            {
                return;
            }

            ActorContactMoved = true;
            SceneEntity.Position = to;
        }

        #endregion
    }

    public abstract class MoveComponent<T> : MoveComponent, IComponent<T> where T: ConfigMoveAgent
    {
        public T ConfigMove => Config as T;
        public void Init(T config)
        {
            base.Init(config);
            InitInternal();
        }

        public sealed override void Destroy()
        {
            base.Destroy();
            DestroyInternal();
        }

        protected abstract void InitInternal();
        protected abstract void DestroyInternal();
    }
}
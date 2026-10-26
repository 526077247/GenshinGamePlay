using System;
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
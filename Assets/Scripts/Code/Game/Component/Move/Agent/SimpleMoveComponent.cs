using UnityEngine;

namespace TaoTie
{
    public class SimpleMoveComponent: MoveComponent<ConfigSimpleMove>
    {
        public override bool useAnimMove => false;
        private NumericComponent numericComponent => parent.GetComponent<NumericComponent>();
        
        /// <summary>
        /// 墙体扫掠检测使用的碰撞层。子类可按配置覆写。
        /// </summary>
        protected int WallCastingLayer => ConfigMove.WallLayer;

        /// <summary>
        /// 贴墙/防嵌入使用的间隙常量。
        /// </summary>
        private const float WallSkin = 0.05f;

        /// <summary>
        /// 单帧最大推出距离（防止 ClosestPoint 异常时瞬间传送过远）。
        /// </summary>
        private const float MaxDepenetrate = 2f;

        /// <summary>
        /// 墙体阻挡：沿水平方向对位移做分段 SphereCast 扫掠。
        /// 命中墙体时移动到紧贴墙面（保留 skin 间隙），并把剩余位移去掉墙法线方向分量，
        /// 仅保留墙面内（切向）的剩余位移继续滑动，避免撞墙直接停死。
        /// y 向位移不参与扫掠（垂直运动原样保留）；高速位移会拆段扫描，避免穿隧。
        /// </summary>
        /// <param name="displacement">本次期望位移。</param>
        /// <returns>被墙体阻挡后的实际位移（无阻挡时原样返回）。</returns>
        protected Vector3 ClampWallBlock(Vector3 displacement)
        {
            if (displacement == Vector3.zero) return displacement;
            Vector3 flat = displacement;
            flat.y = 0;
            float flatLen = flat.magnitude;
            if (flatLen <= 1e-4f) return displacement;

            var actor = parent as Actor;
            float radius = actor?.ConfigActor.Common.ModelRadius ?? 0.5f;
            float castHeight = Mathf.Max(0.05f, (actor?.ConfigActor.Common.ModelHeight ?? 1f) * 0.5f);

            const float maxSegment = 1f;
            const int maxIterations = 32;
            int layerMask = WallCastingLayer;

            Vector3 origin = SceneEntity.Position + Vector3.up * castHeight;

            // 嵌入处理：当球体已与障碍重叠（嵌入障碍内部）时，SphereCast 会以 distance≈0
            // 立即命中，拆不出可用的滑行位移，角色会被卡死无法退出。
            // 此时按“远离障碍（朝最近出口面）”方向剔除朝障碍内的位移分量。
            Vector3 escapeDir = GetEscapeDirection(origin, radius, WallSkin, layerMask);
            if (escapeDir != Vector3.zero)
            {
                // 嵌入时仅剔除“朝障碍内”的分量，保留远离/切向分量：
                // 既保证能朝出口方向跑出，又能沿障碍表面滑出，同时不会越陷越深。
                float into = Mathf.Min(Vector3.Dot(flat, escapeDir), 0f);
                flat -= escapeDir * into;
                return new Vector3(flat.x, displacement.y, flat.z);
            }

            Vector3 rem = flat;                    // 剩余期望位移（不再仅沿原方向，命中后沿墙面切向）
            Vector3 moved = Vector3.zero;

            for (int iter = 0; iter < maxIterations && rem.sqrMagnitude > 1e-8f; iter++)
            {
                Vector3 flatDir = rem.normalized;
                float segLen = Mathf.Min(rem.magnitude, maxSegment);
                if (PhysicsHelper.SphereCastNonAlloc(origin, radius, flatDir, out RaycastHit hit, segLen, layerMask,
                        QueryTriggerInteraction.Ignore))
                {
                    // 命中墙体，只移动到紧贴墙面的位置（保留 skin 间隙）。
                    float step = Mathf.Max(hit.distance - WallSkin, 0f);
                    moved += flatDir * step;
                    rem -= flatDir * step;
                    origin += flatDir * step;

                    // 只去掉“朝墙内”的分量后沿墙面（切向）滑动；
                    // 背向墙面（远离墙）的移动必须保留，否则会被墙面法线剔除而无法反向移动。
                    Vector3 wallNormal = hit.normal;
                    wallNormal.y = 0f;
                    if (wallNormal.sqrMagnitude < 1e-6f)
                        break; // 法线近乎竖直（如地面），横向无法滑动，直接阻挡剩余位移。
                    wallNormal.Normalize();
                    float into = Vector3.Dot(rem, wallNormal);
                    if (into < 0f)
                        rem -= wallNormal * into;
                    // 切向死区：剩余滑动量若仅为期望位移的极小比例（几乎垂直顶墙）时，
                    // 残余切向是数值噪声，其符号会逐帧翻转，表现为贴墙甩头/身体抖动。
                    // 直接清零，让角色干脆地停在墙边（间隙外），保留正常斜向滑墙。
                    if (rem.sqrMagnitude < flat.sqrMagnitude * 0.03f)
                        rem = Vector3.zero;
                    // 贴墙间隙保持：命中距离小于 skin 时，把角色沿墙法线推回 skin 间隙。
                    // 否则 hitDist<skin 时 step 恒为 0，角色会一直“骑”在墙表面滑动，
                    // 视觉上就像陷进墙里，且 ResolveEmbedding（阈值 radius-skin）无法察觉。
                    // 仅当贴得比间隙更近时才推（静置在间隙处时 gap=0），不会引入周期性抖动。
                    float gap = Mathf.Max(WallSkin - hit.distance, 0f);
                    // 仅在“有移动意图(位移非零)但已被完全挡死(剩余切向≈0、本帧无位移结果)”时
                    // 才沿法线往外顶到间隙外；斜向滑墙等仍有切向位移的场景不做推出，
                    // 避免“推出→下一帧又被挡回→再推出”的周期性往返抖动。
                    if (rem.sqrMagnitude <= 1e-8f)
                        moved += wallNormal * gap;
                    // 扫描起点一并回到间隙外，避免切向 SphereCast 立刻再命中。
                    origin += wallNormal * (gap + WallSkin);
                    continue;
                }

                moved += flatDir * segLen;
                rem -= flatDir * segLen;
                origin += flatDir * segLen;
            }

            Vector3 result = new Vector3(moved.x, displacement.y, moved.z);
            return result;
        }

        /// <summary>
        /// 检测实体是否已嵌入障碍：重叠球直径略小于碰撞球（留 skin 间隙），避免贴墙静止时误判。
        /// 已嵌入时把各障碍“中心到最近出口表面”的方向合成并返回（水平归一化），否则返回 zero。
        /// </summary>
        private Vector3 GetEscapeDirection(Vector3 center, float radius, float skin, int layerMask)
        {
            int len = PhysicsHelper.OverlapSphereNonAlloc(center, Mathf.Max(0f, radius - skin), layerMask);
            if (len <= 0) return Vector3.zero;

            Vector3 escape = Vector3.zero;
            for (int i = 0; i < len; i++)
            {
                Collider col = PhysicsHelper.Colliders[i];
                if (col == null) continue;
                // 中心在障碍内部时，ClosestPoint 返回最近出口的边界点，
                // 从中心指向该边界点（closest - center）即远离障碍（向出口）的方向。
                Vector3 dir = col.ClosestPoint(center) - center;
                dir.y = 0f;
                if (dir.sqrMagnitude < 1e-6f) continue;
                escape += dir.normalized;
            }
            if (escape.sqrMagnitude < 1e-6f) return Vector3.zero;
            return escape.normalized;
        }

        /// <summary>
        /// 防嵌入：实体与障碍重叠（深度超过 skin 间隙）时，无条件沿逃生方向把实体推出，
        /// 与移动输入、ORCA 速度均无关。每帧在移动逻辑之前调用，保证即使速度为零/指向墙内
        /// 也能逐帧向出口脱身，不再卡死在障碍里。推出后回到 skin 间隙，不干扰正常贴墙滑行。
        /// </summary>
        protected void ResolveEmbedding()
        {
            var actor = parent as Actor;
            float radius = actor?.ConfigActor.Common.ModelRadius ?? 0.5f;
            float castHeight = Mathf.Max(0.05f, (actor?.ConfigActor.Common.ModelHeight ?? 1f) * 0.5f);
            int layerMask = WallCastingLayer;

            Vector3 center = SceneEntity.Position + Vector3.up * castHeight;
            int len = PhysicsHelper.OverlapSphereNonAlloc(center, Mathf.Max(0f, radius - WallSkin), layerMask);
            if (len <= 0) return;

            Vector3 escape = Vector3.zero;
            float penetration = 0f;
            for (int i = 0; i < len; i++)
            {
                Collider col = PhysicsHelper.Colliders[i];
                if (col == null) continue;
                // 中心在障碍内部时，ClosestPoint 返回最近出口的边界点，
                // 从中心指向该边界点（closest - center）即远离障碍（向出口）的方向。
                Vector3 dir = col.ClosestPoint(center) - center;
                dir.y = 0f;
                if (dir.sqrMagnitude < 1e-6f) continue;
                escape += dir.normalized;
                penetration = Mathf.Max(penetration, dir.magnitude);
            }
            if (escape.sqrMagnitude < 1e-6f)
            {
                return;
            }

            Vector3 push = escape.normalized * Mathf.Min(penetration + WallSkin, MaxDepenetrate);
            SceneEntity.Position += push;
        }

        protected override void InitInternal()
        {
            
        }

        protected override void DestroyInternal()
        {
            
        }
        protected override void UpdateInternal()
        {
            // 防嵌入：无论是否有输入/速度，每帧先强制脱离障碍，避免卡死。
            if (!ConfigMove.CanPassWall)
                ResolveEmbedding();
            
            if(CharacterInput == null) return;
            float deltaTime = GameTimerManager.Instance.GetDeltaTime() / 1000f;
            HandleRotation(deltaTime);
            
            //doMove
            var speed = numericComponent.GetAsFloat(NumericType.Speed);
            var velocity = CharacterInput.Direction.normalized * speed * CharacterInput.SpeedScale;
            velocity = ApplyORCA(velocity, speed);

            CharacterInput.Velocity = velocity;
            if (velocity != Vector3.zero)
            {
                Vector3 displacement = deltaTime * velocity;
                if (!ConfigMove.CanPassWall)
                {
                    displacement = ClampWallBlock(displacement);
                }
                SceneEntity.Position += displacement;
            }
        }
    }
}
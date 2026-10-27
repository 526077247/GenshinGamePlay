using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 动画驱动移动：水平位移来自 Animator 根运动（<see cref="OnAnimatorMove"/> 采样），
    /// 竖直速度（重力/跳跃/击退）与地形/墙体约束由本组件在 FixedUpdate 中合成。
    /// 位移直接写入 SceneEntity.Position，View 经 Messager 同步，不依赖 Rigidbody。
    /// Mono 侧仅保留 AnimatorMoveBridge 转发 OnAnimatorMove 回调。
    /// </summary>
    public partial class AnimatorMoveComponent : MoveComponent<ConfigAnimatorMove>, IFixedUpdate
    {
        public override bool useAnimMove => true;

        // 击退：冲量→速度倍率与线性衰减速率取本移动方式的配置（见 MoveComponent「击退冲量」region）
        protected override float HitImpulseToSpeed => ConfigMove.HitImpulseScale;
        protected override float HitImpulseDecayRate => ConfigMove.HitImpulseDecay;

        private UnitModelComponent model => parent.GetComponent<UnitModelComponent>();
        private FsmComponent fsm => GetComponent<FsmComponent>();
        private ORCAAgentComponent orcaAgent => GetComponent<ORCAAgentComponent>();

        private Transform view;
        private Animator animator;
        private AnimatorMoveBridge bridge;
        private bool viewHasOffset;

        /// <summary>
        /// 模型 pivot 补偿量（米，世界向上）：部分资产（如 KeQing）模型根浮在脚底上方 0.0545，
        /// 动画站位下脚掌会比模型根低一截。若只把脚踝按「地面 + mFootHeight」钉住，髋不动，
        /// 腿就被压短同样多 —— 实测屈膝 31.6°(平地)/47.5°(斜面)，肉眼「半蹲」。
        /// 因此角色站在地面时实体高度要额外加上该值：模型根整体上移后，动画脚踝正好落在
        /// 「地面 + mFootHeight」，足部 IK 退化为无操作，腿不弯、脚底同时贴地。
        /// 取值来自模型上的 MecFootPlacer.mPelvisLift（0 = 不补偿）。
        /// 注意：不能在 MecFootPlacer 里靠抬 Hips 骨骼实现 —— Unity 的 IK 回写按内部姿态重解算，
        /// OnAnimatorIK 里手写的骨骼位移既不参与解算也不保留（实测 LateUpdate 里髋高仍是动画原值）。
        /// </summary>
        private float pivotLift;

        // 击退速度 hitVelocity 已上提到 MoveComponent（SimpleMove 也要用），见那里的「击退冲量」region
        // 离地时继承的水平速度（世界系，空中线性衰减）
        private Vector3 airVelocity;
        // 上一帧的常规水平速度（不含击退），用于离地瞬间继承
        private Vector3 lastTotalHorizontal;
        // 竖直速度（重力/起跳锁定）
        private float verticalVelocity;
        private bool isGrounded;
        private bool lastIsGrounded;
        private float groundY;
        private float lastGroundY; // 上一帧地面高度，用于浅穿透兜底
        // CheckGround 命中的地面法线，供四足贴坡倾斜使用
        private Vector3 groundNormal = Vector3.up;
        // 平滑后的地面法线：贴坡倾斜只作用于 View（逻辑朝向恒水平），每渲染帧按它现算倾斜角。
        // 必须独立于 groundNormal 平滑，不能用「旋转做 Slerp」——那样既有平滑延迟，
        // 又会与 HandleRotation/FSM 对 SceneEntity.Rotation 的写入互相覆盖（表现为倾斜滞后/时有时无）。
        private Vector3 groundNormalSmooth = Vector3.up;
        // 连续丢失着地信号的时长（s），用于贴坡姿态的宽限保持
        private float airborneTime;
        private bool jumpTriggered;
        private float jumpPinTime;

        /// <summary>
        /// Entity 层的位掩码。用于把角色自身的层从「地面检测」里剔出去。
        /// </summary>
        private static readonly int EntityLayerBit = LayerMask.GetMask("Entity");

        /// <summary>
        /// 地面检测实际使用的掩码 = 配置掩码 剔除 <b>Entity</b> 层。
        ///
        /// 为什么必须剔除：<see cref="ConfigAnimatorMove.GroundMask"/> 的默认值含 Entity，
        /// 而着地检测是一个半径 <c>ModelRadius*0.8 ≈ 0.56</c> 的球从脚底上方往下扫。
        /// 角色互斥上线前两个单位可以互相穿过，这颗球永远不会持续与对方相交；
        /// 现在会真的贴在一起，于是：
        ///  • 击中对方胶囊侧面 → 法线接近水平 → 角度超 SlopeLimit → 判为「未着地」，
        ///    着地吸附随之失效、重力把角色一直往下压 —— 表现为「撞在一起后半蹲并陷入地下」；
        ///  • 击中对方胶囊顶部 → groundY 取到对方头顶高度 → 角色被抬到对方身上。
        /// 地面只能由地形/水面提供，因此这里统一裁剪，不改配置数据（配置仍保留 Entity 以兼容旧行为开关）。
        /// </summary>
        private int GroundMaskWithoutEntity
        {
            get
            {
                int mask = ConfigMove.GroundMask & ~EntityLayerBit;
                // 兜底：若配置只配了 Entity，退回原值，避免完全丧失着地检测
                return mask != 0 ? mask : ConfigMove.GroundMask;
            }
        }

        protected override void InitInternal()
        {
            InitMoveConstraint(ActorMoveProfile.Create(parent as Actor),
                ConfigMove.CanPassWall ? 0 : ConfigMove.WallLayer.value);
            InitAsync().Coroutine();
        }

        private async ETTask InitAsync()
        {
            var model = parent.GetComponent<UnitModelComponent>();
            if (model == null)
            {
                Log.Error($"AnimatorMoveComponent 缺少 UnitModelComponent, entity: {parent.Id}");
                return;
            }
            await model.WaitLoadGameObjectOver();
            if (IsDispose) return;
            view = model.EntityView;
            viewHasOffset = model.IsViewOffset;

            // 模型 pivot 补偿量由模型自己声明（见 pivotLift 注释），必须在这里读一次：
            // 之后所有地面高度换算都要带上它，否则脚底会陷进地面 pivotLift。
            pivotLift = 0f;
            if (view != null)
            {
                var footPlacer = view.GetComponentInChildren<MecFootPlacer>(true);
                if (footPlacer != null) pivotLift = footPlacer.mPelvisLift;
            }

            animator = model.GetAnimator();
            if (animator == null)
            {
                Log.Error($"AnimatorMoveComponent 找不到 Animator, entity: {parent.Id}");
                return;
            }
            bridge = animator.gameObject.GetComponent<AnimatorMoveBridge>();
            if (bridge == null) bridge = animator.gameObject.AddComponent<AnimatorMoveBridge>();
            bridge.OnAnimatorMoveEvt = OnAnimatorMove;
        }

        protected override void DestroyInternal()
        {
            if (bridge != null)
            {
                bridge.OnAnimatorMoveEvt = null;
                bridge = null;
            }
            animator = null;
            view = null;
            hitVelocity = Vector3.zero;
            airVelocity = Vector3.zero;
            lastTotalHorizontal = Vector3.zero;
            verticalVelocity = 0f;
            isGrounded = false;
            lastIsGrounded = false;
            lastGroundY = 0f;
            groundNormal = Vector3.up;
            groundNormalSmooth = Vector3.up;
            airborneTime = 0f;
            jumpTriggered = false;
        }

        protected override void UpdateInternal()
        {
            if (animator == null) return;
            // animator.speed 单一写入点：timeScale × 状态动画速率。
            // playSpeed 在 OnAnimatorMove 中按 RVO 结果刷新，此处每帧统一生效，避免与 ChangeTimeScale 竞争。
            animator.speed = GameTimerManager.Instance.GetTimeScale() * currentPlaySpeed;
        }

        public void FixedUpdate()
        {
            if (view == null || animator == null || CharacterInput == null) return;
            float dt = Time.fixedDeltaTime;
            if (dt <= 0f) return;

            ActorContactMoved = false;

            // 击退冲量必须先入账再判着地：CheckGround 用 hitVelocity.y 识别「被击飞」，
            // 若放在它之后就消费，击飞的第一帧会先被判为着地，着地吸附当帧就把角色按回地面。
            ConsumeHitImpulse();
            DecayHitVelocity(dt);

            // 兼容外部直接写 View 的情况（多模型 View 带偏移，不能反推实体位置）
            if (!viewHasOffset) SceneEntity.SyncViewPosition(view.position);

            CheckGround(dt);

            // 必须紧跟 CheckGround：后面 ConstrainVelocity → ConstrainActor 要读本帧最新的
            // Airborne（击飞第一帧就得豁免，否则冲量会被对方体积当场投影掉，表现为「打不动」）。
            SyncBodyAirborne();

            // 朝向：与 RigidbodyMove/SimpleMove 一致走基类 HandleRotation（RotateSpeed 步进 + 死区吸附）。
            // 必须在速度合成之前调用：GetAnimatorHorizontalVelocity 依赖 SceneEntity.Rotation
            // 把局部系动画速度重定向到新朝向，实现边转边沿新方向位移。
            HandleRotation(dt);

            // 四足贴坡站立：只采样/平滑地面法线，倾斜统一由 SyncViewTilt 写 View。
            // 不能在这里写 SceneEntity.Rotation——逻辑朝向必须恒为水平 yaw，否则与
            // HandleRotation/FSM 的每帧 yaw 写入互相覆盖，倾斜会被反复抹掉（表现为不实时）。
            UpdateGroundAlign(dt);
            // 立即把贴坡姿态落到 View，不能等到动画回调再写（见 SyncViewTilt 注释）
            SyncViewTilt();

            // 击退冲量已在物理步最前面入账（ConsumeHitImpulse），此处不再重复消费
            UpdateJump(dt);

            // 起跳锁定期间豁免重力：复刻旧版 Jumping 状态每帧覆盖垂直速度为 JumpSpeed 的语义，
            // 保证 0.2s 锁定期内上升速度恒为 JumpSpeed（跳跃高度不缩水）。
            // 松手截断时 jumpTriggered 已置 false，重力当帧立即生效（旧版松手转 Rising 同语义）。
            if (!isGrounded && !jumpTriggered) verticalVelocity -= ConfigMove.Gravity * dt;

            var animHorizontal = GetAnimatorHorizontalVelocity();
            Vector3 horizontal;
            if (isGrounded)
            {
                airVelocity = Vector3.zero;
                horizontal = animHorizontal;
            }
            else
            {
                if (lastIsGrounded)
                {
                    // 离地瞬间继承上一帧水平速度，离地当帧不叠加动画水平速度，避免翻倍
                    airVelocity = lastTotalHorizontal;
                    horizontal = airVelocity;
                }
                else
                {
                    DecayVector(ref airVelocity, ConfigMove.AirFriction, dt);
                    horizontal = animHorizontal + airVelocity;
                }
            }
            // 只记「常规水平速度」，不含击退：击退有独立通道，若一并记进来，
            // 被击飞当帧会走「离地继承 airVelocity = lastTotalHorizontal」再叠加一次 hitVelocity，
            // 水平击退翻倍（表现为被击飞瞬间突然横向窜出去）。
            lastTotalHorizontal = horizontal;

            Vector3 velocity = horizontal + hitVelocity + Vector3.up * verticalVelocity;
            velocity = ConstrainVelocity(velocity);

            // 着地截断：纯代码位移没有物理碰撞兜底（旧版/RigidbodyMove 靠 PhysX），
            // 本帧下降不得越过地面，防止高速下落穿透；已浅穿透时抬回地面。
            // 击退上升期（hitVelocity.y > 0）不截断。
            if (isGrounded && velocity.y < 0f && hitVelocity.y <= 0f)
            {
                float maxFallSpeed = (groundY + pivotLift - SceneEntity.Position.y) / dt;
                if (velocity.y < maxFallSpeed) velocity.y = maxFallSpeed;
            }

            if (velocity != Vector3.zero)
                SceneEntity.Position += velocity * dt;

            // 着地吸附：把脚底贴回地面（兼容台阶/斜坡）， grounded 时抑制向下速度
            if (isGrounded && verticalVelocity <= 0f && hitVelocity.y <= 0f)
            {
                var pos = SceneEntity.Position;
                float standY = groundY + pivotLift;
                if (Mathf.Abs(pos.y - standY) > 1e-4f)
                {
                    pos.y = standY;
                    SceneEntity.Position = pos;
                }
                verticalVelocity = 0f;
            }

            // 角色间重叠消解（纯几何，只动 XZ，不影响着地检测与脚部 IK）。
            // 必须放在着地吸附之后：吸附会重写 y，这里只累加水平分量。
            ResolveActorContacts();

            // 互斥改过水平位置：斜坡上新 XZ 对应的地面高度与当前 y 已不对应，必须就地重采样。
            // 不重采样的后果：脚底低于坡面 → 脚部 IK 为够到「地面 + mFootHeight」只能压短腿
            // （视觉半蹲 + 脚陷入地下）；坡陡时射线还会从坡体内部起算而丢失着地信号。
            // 平地不受影响：XZ 变化不改变地面高度。
            if (ActorContactMoved)
            {
                CheckGround(dt);
                if (isGrounded && verticalVelocity <= 0f && hitVelocity.y <= 0f)
                {
                    var snapped = SceneEntity.Position;
                    float actorStandY = groundY + pivotLift;
                    if (Mathf.Abs(snapped.y - actorStandY) > 1e-4f)
                    {
                        snapped.y = actorStandY;
                        SceneEntity.Position = snapped;
                    }
                    verticalVelocity = 0f;
                }
            }

            // 击飞状态落地即解除，并同步给体积代理（互斥在 Airborne 期间整体豁免）。
            // 末尾再同步一次：互斥重采样可能改了 isGrounded。
            SyncBodyAirborne();

            // 着地状态提交：放在互斥/重采样之后，保证 FSM 收到的是本物理步的最终结果
            if (isGrounded != lastIsGrounded)
            {
                lastIsGrounded = isGrounded;
                fsm?.SetData(FSMConst.Land, isGrounded);
            }
            lastGroundY = groundY;
        }

        /// <summary>
        /// 地面检测：从脚底上方球射向下，自身碰撞体通过 ignoreRoot 过滤（无需交换 Layer）。
        /// 着地时传感器范围额外延伸一个台阶高度，兼容下台阶/斜坡。
        /// 上升期（verticalVelocity > 0）不采信地面：复刻旧控制器 Rising/Jumping 语义，
        /// 否则起跳后数帧内传感器仍命中脚下地面，着地吸附会把角色按回地面导致跳不起来/跳极低。
        /// 检测长度纳入本帧计划下落距离（swept）：纯代码位移无物理碰撞兜底，
        /// 固定检测带会被高速下落的单帧步长整体越过并使传感器起点钻入地面以下导致穿透；
        /// swept 长度保证本帧路径经过地面即命中。浅穿透恢复由 lastGroundY 兜底负责。
        /// </summary>
        private void CheckGround(float dt)
        {
            var pos = SceneEntity.Position;
            // pos.y 是「实体高度」= 脚底地面 + pivotLift；groundY 始终记真实地面高度。
            // 下面所有「掉到哪了」的判断都换回脚底空间，否则 pivotLift 会让着地提前/滞后。
            float footY = pos.y - pivotLift;
            // 上升期（跳跃锁定/重力反弹）与「击退抬升期」都不采信地面。
            // 击退的竖直分量走的是 hitVelocity 而不是 verticalVelocity：若这里只判后者，
            // 击飞全程 isGrounded 仍为 true，等 hitVelocity.y 衰减到 0 的那一帧会被着地吸附
            // 瞬间拉回地面 —— 表现为「弹一下又贴回去」，既没有抛物线也没有滞空。
            if (verticalVelocity > 0f || hitVelocity.y > HitImpulseAirborneEps)
            {
                isGrounded = false;
                groundY = footY;
                return;
            }
            var profile = moveProfile;
            float radius = Mathf.Max(0.05f, (profile != null ? profile.Radius : 0.5f) * 0.8f);
            // 检测球球心抬高量取两者较大值：半身高+半个台阶 与 半径+最小离地余量。
            // 为什么不能只用前者：球半径=ModelRadius*0.8，对矮胖体型（H/2+SH/2 < R*0.8，如 Fox H=0.5/R=0.7：
            // 0.325 < 0.56）球底会低于脚底并永久嵌进地面碰撞体。平地上接触点还能采到当前地面，
            // 上坡时地形升高立即没过球底，SphereCast 起步重叠返回的 distance=0 假命中使 groundY 失真，
            // 着地吸附把角色按在旧高度——每帧陷入坡体，表现为上坡不生效、直接嵌入地面。
            // 保证球底至少高于脚底 MinGroundClearance：缓坡每帧地形上升量（速度×dt×tan坡角）远小于余量，
            // 扫掠始终命中真实地表，滞后一帧的着地吸附即可把角色贴坡抬升。
            // 取 max 而非固定值：高挑体型（KeQing H=1.5/R=0.7：0.825 > 0.81）仍走原口径，行为零变化。
            const float MinGroundClearance = 0.25f;
            float originLift = Mathf.Max(
                (profile != null ? profile.Height : 1f) * 0.5f + ConfigMove.StepHeight * 0.5f,
                radius + MinGroundClearance);
            float fallDist = Mathf.Max(0f, -verticalVelocity * dt);
            float length = originLift + (isGrounded ? ConfigMove.StepHeight : 0f) + ConfigMove.GroundSkin
                           + fallDist;
            Vector3 origin = pos + Vector3.up * originLift;
            bool hit = PhysicsHelper.SphereCastNonAlloc(origin, radius, Vector3.down, out var rh,
                length, GroundMaskWithoutEntity, QueryTriggerInteraction.Ignore, view);
            bool landed = hit && Vector3.Angle(rh.normal, Vector3.up) <= ConfigMove.SlopeLimit;
            if (landed && !lastIsGrounded)
            {
                // 下落中：swept 检测带覆盖「本帧路径 + 余量」，命中的地面本帧可能根本到不了
                // （曾因恢复余量 0.5m 在离地 1.1m 处提前判着地，着地吸附随即把角色瞬移贴地）。
                // 只有本帧结束位置到达/穿过地面（含 GroundSkin 容忍）才算着地。
                // 站立中（lastIsGrounded）豁免：脚下台阶/斜坡的命中距离天然大于本帧步长。
                if (footY - fallDist > rh.point.y + ConfigMove.GroundSkin)
                    landed = false;
            }
            if (landed)
            {
                isGrounded = true;
                groundY = rh.point.y;
                groundNormal = rh.normal;
            }
            else if (lastIsGrounded && footY < lastGroundY - 1e-3f)
            {
                // 穿透兜底：Miss 但脚底已低于上一帧已知地面 = 已浅穿透。
                // origin 退到地下后向下扫因起点重叠永远 Miss，不兜底会无限穿透。
                isGrounded = true;
                groundY = lastGroundY;
            }
            else
            {
                isGrounded = false;
                groundY = footY;
                groundNormal = Vector3.up;
            }
        }

        /// <summary>
        /// 把「被击飞」状态同步给体积代理：空中期间角色互斥整体豁免（见 <see cref="ActorBodyComponent.Airborne"/>）。
        /// 两处调用：① CheckGround 之后（ConstrainVelocity/ConstrainActor 需要本帧最新状态）；
        /// ② 物理步末尾（互斥改过 XZ 会重采样地面，isGrounded 可能变化）。
        /// 着地即清 HitLaunched，落地当帧就恢复互斥，重叠交给常规消解处理。
        /// </summary>
        private void SyncBodyAirborne()
        {
            if (isGrounded) HitLaunched = false;
            var body = Body;
            if (body != null) body.Airborne = HitLaunched;
        }

        /// <summary>
        /// 四足贴坡站立：平滑地面法线，倾斜本身不在这里落盘（见 <see cref="GroundTilt"/>）。
        /// 两足（Human rig）不启用——身体保持竖直，足部贴合由足部 IK（MecFootPlacer）负责；
        /// Generic rig（无人类直立骨骼假设、动画按 rig 原样播放）必须靠身体倾斜贴坡，
        /// 否则前后脚高差必然一陷一悬。
        /// 平滑只做在法线上，且跟随速率取高（35/s，60ms 走完 88%）：上坡当帧即开始贴坡，坡度变化无可见滞后；
        /// 离地回正走更慢的速率并带宽限期，避免斜坡上偶发丢失着地信号时身体抖动。
        /// </summary>
        private void UpdateGroundAlign(float dt)
        {
            // 仅 Humanoid rig 跳过（身体保持竖直，贴合交给足部 IK）；
            // avatar 为空视为非 Humanoid 的 rig（Generic 或裸骨骼），同样需要靠身体倾斜贴坡。
            if (animator == null || (animator.avatar != null && animator.avatar.isHuman))
            {
                groundNormalSmooth = Vector3.up;
                return;
            }

            var target = Vector3.up;
            if (isGrounded && groundNormal.sqrMagnitude > 0.5f)
            {
                airborneTime = 0f;
                var n = groundNormal.normalized;
                if (Vector3.Angle(n, Vector3.up) <= ConfigMove.SlopeLimit) target = n;
                else target = groundNormalSmooth;   // 超坡度：保持当前姿态，不突然回正
            }
            else
            {
                // 着地信号在斜坡/台阶边缘会偶发丢一两帧（球扫掠瞬时脱手），
                // 立即回正会让身体在坡上抖动；超过宽限期才真正判定离地回正。
                airborneTime += dt;
                if (airborneTime < AlignAirborneGrace) target = groundNormalSmooth;
            }

            float rate = isGrounded ? GroundAlignFollow : GroundAlignRecover;
            float k = 1f - Mathf.Exp(-rate * dt);
            groundNormalSmooth = Vector3.Slerp(groundNormalSmooth, target, k).normalized;
        }

        // 贴坡跟随速率（1/s）：35 → 约 3 帧（60ms）走完 88%，接近即时；再快会把坡面三角面的
        // 法线跳变直接抖到模型上。
        private const float GroundAlignFollow = 35f;
        // 离地回正速率（1/s）：慢一倍，抑制斜坡上着地信号抖动引起的翻转。
        private const float GroundAlignRecover = 10f;
        // 着地信号丢失的宽限期（s）：期间保持现有贴坡姿态。
        private const float AlignAirborneGrace = 0.15f;

        /// <summary>
        /// 当前贴坡倾斜（仅 pitch/roll，世界系）。每渲染帧由 <see cref="OnAnimatorMove"/> 现算，
        /// 与当帧 SceneEntity.Rotation（yaw）相乘得到 View 姿态：yaw 零延迟、倾斜只受法线平滑影响。
        /// 法线接近竖直时直接返回 identity，避免 FromToRotation 退化。
        /// </summary>
        internal Quaternion GroundTilt
        {
            get
            {
                if (groundNormalSmooth.y >= 1f - 1e-4f) return Quaternion.identity;
                return Quaternion.FromToRotation(Vector3.up, groundNormalSmooth);
            }
        }

        /// <summary>
        /// 把「贴坡倾斜 × 逻辑 yaw」写到 View。唯一写入点，两处调用：
        /// ① FixedUpdate 末尾——同帧的后续逻辑（BillboardComponent.Update 读模型下 Hud 节点世界坐标、
        ///    各种挂点）都在这之后跑，若倾斜等到动画回调才落盘，血条会按「水平姿态」算位置，
        ///    与渲染时的倾斜身体错位、陷进模型体内产生深度争用（表现为血条一直闪烁）；
        /// ② PinViewRotation（OnAnimatorMove）——Unity 已把本帧根运动应用到 View 上，需重新钉回。
        /// </summary>
        private void SyncViewTilt()
        {
            if (view == null) return;
            view.rotation = GroundTilt * SceneEntity.Rotation;
        }

        /// <summary>
        /// 把局部系动画速度转换为世界系水平速度。
        /// 保留 MotionDirection.Left/Right 的 2.5D 横移契约：速度大小重定向到右轴。
        /// 竖直分量忽略（由重力/跳跃/击退通道负责）。
        /// 贴坡倾斜只作用在 View 上，SceneEntity.Rotation 恒为水平 yaw，rot*local 天然无竖直分量；
        /// 末尾的 y 清零保留作兜底，坡面抬升由着地吸附（pos.y=groundY）承担。
        /// </summary>
        private Vector3 GetAnimatorHorizontalVelocity()
        {
            if (CharacterInput == null) return Vector3.zero;
            var local = CharacterInput.Velocity;
            local.y = 0f;
            if (local.sqrMagnitude < 1e-8f) return Vector3.zero;
            var rot = SceneEntity.Rotation;
            Vector3 world;
            switch (CharacterInput.MotionDirection)
            {
                case MotionDirection.Left:
                    world = rot * Vector3.left * local.magnitude;
                    break;
                case MotionDirection.Right:
                    world = rot * Vector3.right * local.magnitude;
                    break;
                default:
                    world = rot * local;
                    break;
            }
            world.y = 0f;
            return world;
        }

        /// <summary>
        /// 起跳：IsJump 上升沿且着地时锁定上升速度 JumpDuration，保证跳跃高度；
        /// IsJump 结束时提前解除锁定（可变跳跃高度）。
        /// 起跳条件用 lastIsGrounded 而非 isGrounded：CheckGround 的"本帧到达"判定意味着
        /// 落地帧角色可能还在半空（本帧才下落到地面），当帧起跳会从半空位置上抛
        /// （着地吸附被 verticalVelocity>0 跳过，位置不连续，表现为"空气平台起跳"）。
        /// 站立帧起跳则位置天然连续，延迟一帧（20ms）手感无感。
        /// </summary>
        private void UpdateJump(float dt)
        {
            if (CharacterInput == null) return;
            if (CharacterInput.Jump && !jumpTriggered && lastIsGrounded)
            {
                jumpTriggered = true;
                jumpPinTime = 0f;
            }
            if (!CharacterInput.Jump) jumpTriggered = false;
            if (jumpTriggered)
            {
                verticalVelocity = ConfigMove.JumpSpeed;
                jumpPinTime += dt;
                if (jumpPinTime >= ConfigMove.JumpDuration) jumpTriggered = false;
            }
        }

        private Vector3 ConstrainVelocity(Vector3 velocity)
        {
            if (!HasMoveConstraint || velocity == Vector3.zero) return velocity;
            float dt = Time.fixedDeltaTime;
            if (dt <= 0f) return velocity;

            // 竖直分量（重力/起跳锁定/击退）不参与位移约束：
            // ClampTerrainBlock 以足印可走性判定位置合法性，空中下落时起点即判定为不可走，
            // 而 EstimateFreeDirection 只探测水平方向（探测点与起点同高度，全不可走），
            // 含竖直分量的位移会被整体清零——角色悬停在空中（表现为"落到空气平台"），
            // 随后着地吸附把角色瞬移到地面（表现为"闪一下落地"）。
            float vertical = velocity.y;

            // 水平分量：墙体扫掠全程保留（防空中穿墙/飞出可移动区域）；
            // 地形足印约束仅着地时启用——足印可走性只描述站立面，
            // 起跳/下落/击退期间起点可能位于地形上方，判定无意义，
            // 且会把空中水平位移一并清零导致跳跃/击退中无法移动。
            var area = GetComponent<MoveAreaComponent>()?.Area;
            var flat = velocity;
            flat.y = 0f;
            Vector3 displacement = flat * dt;
            if (ConfigMove.CanPassWall)
            {
                if (isGrounded) displacement = ConstrainTerrain(displacement, area);
            }
            else
            {
                ResolveEmbedding();
                displacement = isGrounded
                    ? Constrain(displacement, area)
                    : ConstrainWallOnly(displacement);
            }

            // 角色互斥（行进滑移）：放在墙/地形之后，保证先被墙挡住再考虑推挤。
            // 只处理水平面；竖直分量原样带出（重力/跳跃/击退）。
            displacement = ConstrainActor(displacement);

            var result = displacement / dt;
            result.y = vertical;
            return result;
        }
    }
}

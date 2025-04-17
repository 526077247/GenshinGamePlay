using System;
using UnityEngine;

namespace TaoTie
{
    public class PlatformMoveComponent:Component,IComponent<ConfigRoute,SceneGroup>,IUpdate
    {
        /// <summary>
        /// 可为空，为空路径表示世界坐标
        /// </summary>
        private SceneGroup sceneGroup;
        public bool IsStart { get; private set; }
        
        public bool IsPause { get; private set; }
        
        public delegate void OnMoveStateChangeDelegate(bool isStart);
        public event OnMoveStateChangeDelegate onMoveStateChange; 
        
        private ConfigRoute route;
        
        #region 移动相关参数

        /// <summary>
        /// 是否使用动画移动
        /// </summary>
        private bool useAnimMove;

        /// <summary>
        /// 下一个或这一个（抵达后等待中）路径点
        /// </summary>
        public ConfigWaypoint NextTarget
        {
            get
            {
                if (this.n >= this.targets.Length)
                {
                    var sceneGroupActor = this.parent.GetComponent<SceneGroupActorComponent>();
                    if (sceneGroupActor == null)
                    {
                        Log.Error($"路径内部错误! 请策划检查配置  routeId={route.LocalId}");
                    }
                    else
                    {
                        Log.Error($"路径内部错误! 请策划检查配置  SceneGroupId={sceneGroupActor.SceneGroup.Config.Id} " +
                                  $"actor={sceneGroupActor.LocalId} routeId={route.LocalId}");
                    }

                    return this.targets[this.targets.Length - 1];
                }

                return this.targets[this.n];
            }
        }

        /// <summary>开启移动的时间</summary>
        private float beginTime;

        /// <summary>当前路径点的开始时间</summary>
        private float startTime { get; set; }

        /// <summary>上次Update的开始时间</summary>
        private float updateTime { get; set; }

        /// <summary>开启移动协程的Unit的位置</summary>
        private Vector3 startPos;

        /// <summary> 移动速度 m/s </summary>
        private float speed;

        /// <summary> 当前移动会耗时多久 </summary>
        private float needTime;

        /// <summary> 路径点 </summary>
        private ConfigWaypoint[] targets
        {
            get
            {
                if (routeType != RouteType.Reciprocate || !isBack)
                    return route.Points;
                return backPoints;
            }
        }

        /// <summary> 记录到达第几个路径点 </summary>
        private int n;

        /// <summary> 到达下一个目标点后等待时间 </summary>
        private float waitTime;

        /// <summary> 到达下一个目标点后是否一直等待 </summary>
        private bool reachStop;

        /// <summary> 到达下一个目标点后是否广播 </summary>
        private bool hasReachEvent;

        /// <summary> 是否允许移动 </summary>
        private bool enable;

        /// <summary> 是否需要广播角色靠近事件 </summary>
        private bool hasAvatarNearEvent;

        #endregion

        #region 路径类型相关参数

        /// <summary>
        /// 路径类型
        /// </summary>
        private RouteType routeType;

        /// <summary>
        /// 判定抵达的距离
        /// </summary>
        private float sqlArriveRange;

        /// <summary>
        /// 来回类型路径是否是返回
        /// </summary>
        private bool isBack;

        /// <summary>
        /// 返回时的路径
        /// </summary>
        private ConfigWaypoint[] backPoints;

        #endregion

        #region 旋转参数

        private RotType rotType;

        /// <summary> 等待结束时方向 </summary>
        private Quaternion rotRoundLeaveDir;

        /// <summary> 等待开始时方向 </summary>
        private Quaternion rotRoundReachDir;

        /// <summary> 旋转时围绕的轴 </summary>
        private Vector3 axis;

        /// <summary> 移动转向速度 度/s </summary>
        private float moveAngularSpeed;

        /// <summary> 等待转向速度 度/s </summary>
        private float waitAngularSpeed;

        /// <summary> 当前旋转会耗时多久 </summary>
        private float turnTime;

        #endregion

        /// <summary>
        /// 当抵达路径点后触发事件（即使有等待时间一个路径点也只会触发一次）
        /// </summary>
        public Action onReachEvent;

        /// <summary> 是否已经广播过抵达事件 </summary>
        private bool hadBroadcastReachEvent;


        /// <summary>
        /// 判断角色靠近距离的平方
        /// </summary>
        private float sqlAvatarTriggerEventDistance;

        /// <summary>
        /// 延时启动的间隔时间
        /// </summary>
        private int delay;

        /// <summary>
        /// 延时到什么时间启动
        /// </summary>
        private float delayTillTime;

        public void Init(ConfigRoute config,SceneGroup sceneGroup)
        {
            this.sceneGroup = sceneGroup;
            if(config!=null) SetRoute(config);
        }

        public void Destroy()
        {
            this.sceneGroup = null;
            sqlAvatarTriggerEventDistance = 0;
            route = null;
            this.startTime = 0;
            this.startPos = Vector3.zero;
            this.beginTime = 0;
            this.needTime = 0;
            this.speed = 0;
            this.n = 0;
            this.turnTime = 0;
            this.moveAngularSpeed = 0;
            this.axis = default;
            this.backPoints = null;
            this.rotType = default;
            this.isBack = false;
            this.routeType = default;
            this.rotRoundLeaveDir = default;
            this.hadBroadcastReachEvent = false;
            this.hasAvatarNearEvent = false;
            this.onReachEvent = null;
            delayTillTime = 0;
            delay = 0;
            enable = false;
        }
        
        
        public void OnStart()
        {
            if (this.IsStart) return;
            IsStart = true;
            IsPause = false;
            onMoveStateChange?.Invoke(true);
            if (route == null)
            {
                Log.Error("未设置路由");
                return;
            }

            delayTillTime = 0;
            delay = 0;
            this.isBack = false;
            this.enable = true;
            this.n = -1;
        }

        public void Update()
        {
            var nowtime = GameTimerManager.Instance.GetTimeNow() / 1000f;
            #region 延迟启动

            float delay = this.delay / 1000f;
            if (this.delay > 0 && delayTillTime <= 0)
            {
                delayTillTime = nowtime + delay;
            }

            if (nowtime < delayTillTime)
            {
                return;
            }

            if (this.beginTime > 0)
            {
                this.startTime += delay;
                this.beginTime += delay;
                this.updateTime += delay;
            }

            delayTillTime = 0;
            this.delay = 0;

            #endregion

            #region 开始时间

            if (this.beginTime <= 0 && enable)
            {
                this.beginTime = nowtime;
                this.updateTime = this.beginTime;
                this.startTime = this.beginTime;
                this.needTime = 0;
                this.waitTime = 0;
                this.SetNextTarget();
            }

            #endregion

            if (hasAvatarNearEvent)
            {
                var sceneGroupActor = parent.GetComponent<SceneGroupActorComponent>();
                if (sceneGroupActor != null && SceneManager.Instance.GetCurrentScene() is MapScene scene)
                {
                    var avatar = scene.Self;
                    if (avatar != null &&
                        Vector3.SqrMagnitude(avatar.Position - GetParent<Unit>().Position) <
                        sqlAvatarTriggerEventDistance)
                    {
                        Messager.Instance.Broadcast(sceneGroupActor.SceneGroup.Id,MessageId.SceneGroupEvent,new AvatarNearPlatformEvt
                        {
                            ActorId = sceneGroupActor.LocalId,
                            RouteId = route.LocalId,
                            PointIndex = this.NextTarget.Index, 
                            IsMoving = enable
                        });
                    }
                }
            }

            if (!useAnimMove)
            {
                OnUpdateWithVelocity(nowtime);
            }
            else
            {
                OnUpdateWithAnim(nowtime);
            }
        }

        /// <summary>
        /// 使用速度移动
        /// </summary>
        /// <param name="nowtime"></param>
        private void OnUpdateWithVelocity(float nowtime)
        {
            Unit unit = parent as Unit;
            float lastUpdateTime = this.updateTime;
            float elapsetime = nowtime - lastUpdateTime;
            this.updateTime = nowtime;
            if (!this.enable) //暂停中
            {
                this.startTime += this.updateTime - lastUpdateTime;
                return;
            }

            float moveTime = this.updateTime - this.startTime;


            if (moveTime <= 0)
            {
                return;
            }

            // 计算位置插值
            if (moveTime >= this.needTime)
            {
                unit.Position = this.NextTarget.GetPosition(sceneGroup);
                if (this.waitAngularSpeed == 0)
                {
                    unit.Rotation = this.rotRoundLeaveDir;
                }
            }
            else
            {
                // 计算位置插值
                float amount = moveTime * 1f / this.needTime;
                if (amount > 0)
                {
                    Vector3 newPos = Vector3.Lerp(this.startPos, this.NextTarget.GetPosition(sceneGroup), amount);
                    unit.Position = newPos;
                }

                // 计算方向
                if (this.turnTime > 0)
                {
                    unit.Rotation = Quaternion.Euler(unit.Rotation.eulerAngles + this.axis * moveAngularSpeed * elapsetime);
                }
            }

            moveTime -= this.needTime;

            // 进入了配置的抵达范围
            if (Vector3.SqrMagnitude(unit.Position - this.NextTarget.GetPosition(sceneGroup)) <= sqlArriveRange)
            {
            }

            // 表示这个点还没走完，等下一帧再来
            if (moveTime < 0)
            {
                return;
            }

            // 到这里说明这个点已经走完
            if (hasReachEvent && !hadBroadcastReachEvent)
            {
                var sceneGroupActor = unit.GetComponent<SceneGroupActorComponent>();
                if (sceneGroupActor != null)
                {
                    Messager.Instance.BroadcastNextFrame(sceneGroupActor.SceneGroup.Id,MessageId.SceneGroupEvent,new PlatformReachPointEvt
                    {
                        ActorId = sceneGroupActor.LocalId,
                        RouteId = route.LocalId,
                        PointIndex = this.NextTarget.Index,
                    });
                }

                onReachEvent?.Invoke(); // todo:是到达目标点再广播，还是到达配置范围内就广播？
                hadBroadcastReachEvent = true;
            }

            if (moveTime < waitTime)
            {
                // 计算方向插值
                unit.Rotation = Quaternion.Euler(unit.Rotation.eulerAngles + this.axis * waitAngularSpeed * elapsetime);
                return;
            }

            if (reachStop) //到达停止,等待再唤醒
            {
                Pause();
                return;
            }

            // 如果是最后一个点
            if (this.n >= this.targets.Length - 1)
            {
                var pos = this.NextTarget.GetPosition(sceneGroup);
                if (this.targets.Length > 0)
                    unit.Position = pos;

                if (routeType == RouteType.OneWay)
                {
                    this.Destroy();
                    return;
                }
                else
                {
                    this.n = 0; //终点作为起点
                    if (routeType == RouteType.Reciprocate)
                    {
                        isBack = !isBack;
                    }
                }
            }

            this.SetNextTarget();
        }

        /// <summary>
        /// 使用动画移动
        /// </summary>
        /// <param name="nowtime"></param>
        private void OnUpdateWithAnim(float nowtime)
        {
        }

        /// <summary>
        /// 设置下一个路径点
        /// </summary>
        private void SetNextTarget()
        {
            var unit = parent as Unit;
            ++this.n;

            hadBroadcastReachEvent = false;
            //移动速度
            this.speed = NextTarget.TargetVelocity;

            //指向目标点的向量
            Vector3 faceV = this.GetFaceV();
            float distance = faceV.magnitude;

            // 插值的起始点要以unit的真实位置来算
            this.startPos = unit.Position;

            //更新起始时间
            this.startTime += this.needTime + this.waitTime;
            //更新需要移动的时间
            this.needTime = distance / this.speed;
            // 到达后等待时间
            this.waitTime = NextTarget.WaitTime;
            // 达到后等待
            this.reachStop = NextTarget.ReachStop;
            // 达到后广播
            this.hasReachEvent = NextTarget.HasReachEvent;
            // 角色靠近广播
            this.hasAvatarNearEvent = reachStop || NextTarget.HasAvatarNearEvent;

            if (rotType == RotType.ROT_AUTO || (n == 0 && rotType != RotType.ROT_NONE))
            {
                var to = Quaternion.LookRotation(faceV, Vector3.up);
                unit.Rotation = to;
            }

            //持续旋转 todo:
            if (rotType == RotType.ROT_ANGLE)
            {
                this.moveAngularSpeed = NextTarget.RotAngleMoveSpeed;
                this.waitAngularSpeed = NextTarget.RotAngleWaitSpeed;
                // 要用transform的位置
                if (faceV.sqrMagnitude < 0.0001f)
                {
                    return;
                }

                if (NextTarget.RotAngleSameStop)
                {
                    float angle = 360f;
                    this.turnTime = moveAngularSpeed > 0 ? angle / speed : 0;
                }
                else
                {
                    this.turnTime = needTime;
                }

                this.rotRoundReachDir = unit.Rotation;
                this.rotRoundLeaveDir = unit.Rotation;
                return;
            }

            //旋转指定圈数
            if (rotType == RotType.ROT_ROUND)
            {
                this.turnTime = needTime;

                // 要用transform的位置
                if (faceV.sqrMagnitude < 0.0001f)
                {
                    return;
                }

                var nextRotRoundReachDir = NextTarget.GetRotRoundReachDir(sceneGroup);
                var nextRotRoundLeaveDir = NextTarget.GetRotRoundLeaveDir(sceneGroup);
                this.rotRoundReachDir = Quaternion.Euler(nextRotRoundReachDir);
                this.rotRoundLeaveDir = Quaternion.Euler(nextRotRoundLeaveDir);
                var angle = GetAngle(unit.Rotation.eulerAngles, nextRotRoundReachDir);
                this.moveAngularSpeed =
                    n == 0 ? 0 : (angle + NextTarget.RotRoundReachRounds * 360) / turnTime; //第一个点不需要转弯
                if (NextTarget.WaitTime == 0)
                {
                    this.waitAngularSpeed = 0;
                }
                else
                {
                    if (n == 0) //第一个点等到了目标点再转弯
                    {
                        angle = GetAngle(unit.Rotation.eulerAngles, nextRotRoundLeaveDir);
                    }
                    else
                    {
                        angle = GetAngle(nextRotRoundReachDir, nextRotRoundLeaveDir);
                    }

                    this.waitAngularSpeed = (angle + NextTarget.RotRoundWaitRounds * 360) / waitTime;
                }

                return;
            }

            if (rotType == RotType.ROT_AUTO)
            {
                this.turnTime = needTime;

                // 要用transform的位置
                if (faceV.sqrMagnitude < 0.0001f)
                {
                    return;
                }

                this.rotRoundReachDir = unit.Rotation;
                this.rotRoundLeaveDir = this.rotRoundReachDir;
                this.moveAngularSpeed = 0; //移动不需要转弯
                if (NextTarget.WaitTime == 0)
                {
                    this.waitAngularSpeed = 0;
                }
                else
                {
                    var next = GetPointAfterNextTarget();
                    var nextFaceV = next.GetPosition(sceneGroup) - NextTarget.GetPosition(sceneGroup);
                    var to = Quaternion.LookRotation(nextFaceV, Vector3.up);
                    var angle = GetAngle(unit.Rotation.eulerAngles, to.eulerAngles);
                    this.waitAngularSpeed = angle / waitTime;
                }

                return;
            }

            //无转向
            this.moveAngularSpeed = 0;
            this.waitAngularSpeed = 0;
            if (Mathf.Abs(faceV.x) > 0.01 || Mathf.Abs(faceV.z) > 0.01)
            {
                this.rotRoundReachDir = unit.Rotation;
                this.rotRoundLeaveDir = unit.Rotation;
            }

            this.turnTime = 0;
        }

        /// <summary>
        /// 计算夹角
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private float GetAngle(Vector3 from, Vector3 to)
        {
            float res = 0;
            if (route.RotAngleType == RotAngleType.ROT_ANGLE_X)
            {
                res = to.x - from.x;
            }
            else if (route.RotAngleType == RotAngleType.ROT_ANGLE_Y)
            {
                res = to.y - from.y;
            }
            else
            {
                res = to.z - from.z;
            }

            while (res < -180)
            {
                res += 360;
            }

            while (res > 180)
            {
                res -= 360;
            }

            return res;
        }

        /// <summary>
        /// 获取指向目标的方向向量
        /// </summary>
        /// <returns></returns>
        private Vector3 GetFaceV()
        {
            return this.NextTarget.GetPosition(sceneGroup) - GetParent<Unit>().Position;
        }

        /// <summary>
        /// 获取NextTarget的下一个点
        /// </summary>
        /// <returns></returns>
        private ConfigWaypoint GetPointAfterNextTarget()
        {
            if (n == targets.Length - 1)
            {
                if (routeType == RouteType.Reciprocate) //终点作为起点
                {
                    return targets[n - 1];
                }
                else if (routeType == RouteType.Loop) //终点作为起点
                {
                    return targets[1];
                }
                else if (routeType == RouteType.OneWay)
                {
                }

                return targets[n];
            }

            return targets[n + 1];
        }

        public void OnStop()
        {
            IsStart = false;
            IsPause = false;
            onMoveStateChange?.Invoke(false);
            this.beginTime = 0;
            enable = false;
            this.needTime = 0;
            this.waitTime = 0;
        }

        public void Pause()
        {
            if (!IsPause)
            {
                IsPause = true;
                onMoveStateChange?.Invoke(false);
            }
            enable = false;
        }

        /// <summary>
        /// 停止后重新唤醒
        /// </summary>
        public void Resume()
        {
            if (this.IsPause) //广播过事件后被重新唤醒
            {
                reachStop = false;
                IsPause = false;
                onMoveStateChange?.Invoke(true);
            }
            enable = true;
        }

        /// <summary>
        /// 设置路径
        /// </summary>
        /// <param name="route"></param>
        /// <param name="startDelay">延时启动</param>
        public void SetRoute(ConfigRoute route, int startDelay = -1)
        {
            if (this.route != null)
            {
                OnStop();
            }

            this.route = route;
            if (route != null)
            {
                rotType = this.route.RotType;
                routeType = this.route.Type;
                //来回路径的返回路径点
                if (routeType == RouteType.Reciprocate)
                {
                    backPoints = new ConfigWaypoint[this.route.Points.Length];
                    for (int i = 0; i < this.route.Points.Length; i++)
                    {
                        backPoints[this.route.Points.Length - 1 - i] = this.route.Points[i];
                    }
                }

                sqlArriveRange = this.route.ArriveRange * this.route.ArriveRange;
                sqlAvatarTriggerEventDistance = this.route.AvatarNearRange * this.route.AvatarNearRange;
                if (this.route.RotAngleType == RotAngleType.ROT_ANGLE_X)
                {
                    axis = Vector3.forward;
                }
                else if (this.route.RotAngleType == RotAngleType.ROT_ANGLE_Y)
                {
                    axis = Vector3.up;
                }
                else
                {
                    axis = Vector3.left;
                }

                if (startDelay >= 0)
                {
                    DelayStart(startDelay);
                }
            }
        }

        /// <summary>
        /// 延时启动
        /// </summary>
        /// <param name="delay"></param>
        public void DelayStart(int delay)
        {
            if (delay < 0) return;
            OnStart();
            if (delay > 0)
            {
                this.delay = delay;
            }
        }
        /// <summary>
        /// 延时启动
        /// </summary>
        /// <param name="delay"></param>
        public void DelayResume(int delay)
        {
            Resume();
            if (delay > 0)
            {
                this.delay = delay;
            }
        }
    }
}
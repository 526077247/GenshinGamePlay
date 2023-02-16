using System;
using UnityEngine;

namespace TaoTie
{
    public class PlatformMoveComponent:Component,IComponent<ConfigRoute>,IUpdateComponent
    {
        public bool isStart { get; private set; }
        
        public bool isPause { get; private set; }
        
        public delegate void OnMoveStateChangeDelegate(bool isStart);
        public event OnMoveStateChangeDelegate onMoveStateChange; 
        
        private ConfigRoute _route;
        
        #region 移动相关参数

        /// <summary>
        /// 是否使用动画移动
        /// </summary>
        private bool _useAnimMove;

        /// <summary>
        /// 下一个或这一个（抵达后等待中）路径点
        /// </summary>
        public ConfigWaypoint nextTarget
        {
            get
            {
                if (this._n >= this._targets.Length)
                {
                    var gearActor = this.Parent.GetComponent<GearActorComponent>();
                    if (gearActor == null)
                    {
                        Log.Error($"路径内部错误! 请策划检查配置  routeId={_route.localId}");
                    }
                    else
                    {
                        Log.Error($"路径内部错误! 请策划检查配置  gearId={gearActor.gear.config.id} " +
                                  $"actor={gearActor.localId} routeId={_route.localId}");
                    }

                    return this._targets[this._targets.Length - 1];
                }

                return this._targets[this._n];
            }
        }

        /// <summary>开启移动的时间</summary>
        private float _beginTime;

        /// <summary>当前路径点的开始时间</summary>
        private float _startTime { get; set; }

        /// <summary>上次Update的开始时间</summary>
        private float _updateTime { get; set; }

        /// <summary>开启移动协程的Unit的位置</summary>
        private Vector3 _startPos;

        /// <summary> 移动速度 m/s </summary>
        private float _speed;

        /// <summary> 当前移动会耗时多久 </summary>
        private float _needTime;

        /// <summary> 路径点 </summary>
        private ConfigWaypoint[] _targets
        {
            get
            {
                if (_routeType != RouteType.Reciprocate || !_isBack)
                    return _route.points;
                return _backPoints;
            }
        }

        /// <summary> 记录到达第几个路径点 </summary>
        private int _n;

        /// <summary> 到达下一个目标点后等待时间 </summary>
        private float _waitTime;

        /// <summary> 到达下一个目标点后是否一直等待 </summary>
        private bool _reachStop;

        /// <summary> 到达下一个目标点后是否广播 </summary>
        private bool _hasReachEvent;

        /// <summary> 是否允许移动 </summary>
        private bool _enable;

        /// <summary> 是否需要广播角色靠近事件 </summary>
        private bool _hasHeroNearEvent;

        #endregion

        #region 路径类型相关参数

        /// <summary>
        /// 路径类型
        /// </summary>
        private RouteType _routeType;

        /// <summary>
        /// 判定抵达的距离
        /// </summary>
        private float _sqlArriveRange;

        /// <summary>
        /// 来回类型路径是否是返回
        /// </summary>
        private bool _isBack;

        /// <summary>
        /// 返回时的路径
        /// </summary>
        private ConfigWaypoint[] _backPoints;

        #endregion

        #region 旋转参数

        private RotType _rotType;

        /// <summary> 等待结束时方向 </summary>
        private Quaternion _rotRoundLeaveDir;

        /// <summary> 等待开始时方向 </summary>
        private Quaternion _rotRoundReachDir;

        /// <summary> 旋转时围绕的轴 </summary>
        private Vector3 _axis;

        /// <summary> 移动转向速度 度/s </summary>
        private float _moveAngularSpeed;

        /// <summary> 等待转向速度 度/s </summary>
        private float _waitAngularSpeed;

        /// <summary> 当前旋转会耗时多久 </summary>
        private float _turnTime;

        #endregion

        /// <summary>
        /// 当抵达路径点后触发事件（即使有等待时间一个路径点也只会触发一次）
        /// </summary>
        public Action onReachEvent;

        /// <summary> 是否已经广播过抵达事件 </summary>
        private bool _hadBroadcastReachEvent;


        /// <summary>
        /// 判断距离的平方
        /// </summary>
        private float _sqlAvatarTriggerEventDistance;

        /// <summary>
        /// 延时启动的间隔时间
        /// </summary>
        private int _delay;

        /// <summary>
        /// 延时到什么时间启动
        /// </summary>
        private float _delayTillTime;

        public void Init(ConfigRoute config)
        {
            SetRoute(config);
        }

        public void Destroy()
        {
            _sqlAvatarTriggerEventDistance = 0;
            _route = null;
            this._startTime = 0;
            this._startPos = Vector3.zero;
            this._beginTime = 0;
            this._needTime = 0;
            this._speed = 0;
            this._n = 0;
            this._turnTime = 0;
            this._moveAngularSpeed = 0;
            this._axis = default;
            this._backPoints = null;
            this._rotType = default;
            this._isBack = false;
            this._routeType = default;
            this._rotRoundLeaveDir = default;
            this._hadBroadcastReachEvent = false;
            this._hasHeroNearEvent = false;
            this.onReachEvent = null;
            _delayTillTime = 0;
            _delay = 0;
            _enable = false;
        }
        
        
        public void OnStart()
        {
            if (this.isStart) return;
            isStart = true;
            isPause = false;
            onMoveStateChange?.Invoke(true);
            if (_route == null)
            {
                Log.Error("未设置路由");
                return;
            }

            _delayTillTime = 0;
            _delay = 0;
            this._isBack = false;
            this._enable = true;
            this._n = -1;
        }

        public void Update()
        {
            var nowtime = GameTimerManager.Instance.GetTimeNow() / 1000f;
            #region 延迟启动

            float delay = _delay / 1000f;
            if (_delay > 0 && _delayTillTime <= 0)
            {
                _delayTillTime = nowtime + delay;
            }

            if (nowtime < _delayTillTime)
            {
                return;
            }

            if (this._beginTime > 0)
            {
                this._startTime += delay;
                this._beginTime += delay;
                this._updateTime += delay;
            }

            _delayTillTime = 0;
            _delay = 0;

            #endregion

            #region 开始时间

            if (this._beginTime <= 0 && _enable)
            {
                this._beginTime = nowtime;
                this._updateTime = this._beginTime;
                this._startTime = this._beginTime;
                this._needTime = 0;
                this._waitTime = 0;
                this.SetNextTarget();
            }

            #endregion

            if (_hasHeroNearEvent)
            {
                var gearActor = Parent.GetComponent<GearActorComponent>();
                if (gearActor != null && SceneManager.Instance.GetCurrentScene() is BaseMapScene scene )
                {
                    var avatar = Parent.Parent.Get<Unit>(scene.MyId);
                    if (avatar != null &&
                        Vector3.SqrMagnitude(avatar.Position - GetParent<Unit>().Position) <
                        _sqlAvatarTriggerEventDistance)
                    {
                        Messager.Instance.Broadcast(gearActor.gear.Id,MessageId.GearEvent,new HeroNearPlatformEvt
                        {
                            actorId = gearActor.localId,
                            routeId = _route.localId,
                            pointIndex = this.nextTarget.index, 
                            isMoving = _enable
                        });
                    }
                }
            }

            if (!_useAnimMove)
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
            Unit unit = Parent as Unit;
            float lastUpdateTime = this._updateTime;
            float elapsetime = nowtime - lastUpdateTime;
            this._updateTime = nowtime;
            if (!this._enable) //暂停中
            {
                this._startTime += this._updateTime - lastUpdateTime;
                return;
            }

            float moveTime = this._updateTime - this._startTime;


            if (moveTime <= 0)
            {
                return;
            }

            // 计算位置插值
            if (moveTime >= this._needTime)
            {
                unit.Position = this.nextTarget.pos;
                if (this._waitAngularSpeed == 0)
                {
                    unit.Rotation = this._rotRoundLeaveDir;
                }
            }
            else
            {
                // 计算位置插值
                float amount = moveTime * 1f / this._needTime;
                if (amount > 0)
                {
                    Vector3 newPos = Vector3.Lerp(this._startPos, this.nextTarget.pos, amount);
                    unit.Position = newPos;
                }

                // 计算方向
                if (this._turnTime > 0)
                {
                    unit.Rotation = Quaternion.Euler(unit.Rotation.eulerAngles + this._axis * _moveAngularSpeed * elapsetime);
                }
            }

            moveTime -= this._needTime;

            // 进入了配置的抵达范围
            if (Vector3.SqrMagnitude(unit.Position - this.nextTarget.pos) <= _sqlArriveRange)
            {
            }

            // 表示这个点还没走完，等下一帧再来
            if (moveTime < 0)
            {
                return;
            }

            // 到这里说明这个点已经走完
            if (_hasReachEvent && !_hadBroadcastReachEvent)
            {
                var gearActor = unit.GetComponent<GearActorComponent>();
                if (gearActor != null)
                {
                    Messager.Instance.Broadcast(gearActor.gear.Id,MessageId.GearEvent,new PlatformReachPointEvt
                    {
                        actorId = gearActor.localId,
                        routeId = _route.localId,
                        pointIndex = this.nextTarget.index,
                    });
                }

                onReachEvent?.Invoke(); // todo:是到达目标点再广播，还是到达配置范围内就广播？
                _hadBroadcastReachEvent = true;
            }

            if (moveTime < _waitTime)
            {
                // 计算方向插值
                unit.Rotation = Quaternion.Euler(unit.Rotation.eulerAngles + this._axis * _waitAngularSpeed * elapsetime);
                return;
            }

            if (_reachStop) //到达停止,等待再唤醒
            {
                Pause();
                return;
            }

            // 如果是最后一个点
            if (this._n >= this._targets.Length - 1)
            {
                var pos = this.nextTarget.pos;
                if (this._targets.Length > 0)
                    unit.Position = pos;

                if (_routeType == RouteType.OneWay)
                {
                    this.Destroy();
                    return;
                }
                else
                {
                    this._n = 0; //终点作为起点
                    if (_routeType == RouteType.Reciprocate)
                    {
                        _isBack = !_isBack;
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
            var unit = Parent as Unit;
            ++this._n;

            _hadBroadcastReachEvent = false;
            //移动速度
            this._speed = nextTarget.targetVelocity;

            //指向目标点的向量
            Vector3 faceV = this.GetFaceV();
            float distance = faceV.magnitude;

            // 插值的起始点要以unit的真实位置来算
            this._startPos = unit.Position;

            //更新起始时间
            this._startTime += this._needTime + this._waitTime;
            //更新需要移动的时间
            this._needTime = distance / this._speed;
            // 到达后等待时间
            this._waitTime = nextTarget.waitTime;
            // 达到后等待
            this._reachStop = nextTarget.reachStop;
            // 达到后广播
            this._hasReachEvent = nextTarget.hasReachEvent;
            // 角色靠近广播
            this._hasHeroNearEvent = _reachStop || nextTarget.hasHeroNearEvent;

            if (_rotType == RotType.ROT_AUTO || (_n == 0 && _rotType != RotType.ROT_NONE))
            {
                var to = Quaternion.LookRotation(faceV, Vector3.up);
                unit.Rotation = to;
            }

            //持续旋转 todo:
            if (_rotType == RotType.ROT_ANGLE)
            {
                this._moveAngularSpeed = nextTarget.rotAngleMoveSpeed;
                this._waitAngularSpeed = nextTarget.rotAngleWaitSpeed;
                // 要用transform的位置
                if (faceV.sqrMagnitude < 0.0001f)
                {
                    return;
                }

                if (nextTarget.rotAngleSameStop)
                {
                    float angle = 360f;
                    this._turnTime = _moveAngularSpeed > 0 ? angle / _speed : 0;
                }
                else
                {
                    this._turnTime = _needTime;
                }

                this._rotRoundReachDir = unit.Rotation;
                this._rotRoundLeaveDir = unit.Rotation;
                return;
            }

            //旋转指定圈数
            if (_rotType == RotType.ROT_ROUND)
            {
                this._turnTime = _needTime;

                // 要用transform的位置
                if (faceV.sqrMagnitude < 0.0001f)
                {
                    return;
                }

                this._rotRoundReachDir = Quaternion.Euler(nextTarget.rotRoundReachDir);
                this._rotRoundLeaveDir = Quaternion.Euler(nextTarget.rotRoundLeaveDir);
                var angle = GetAngle(unit.Rotation.eulerAngles, nextTarget.rotRoundReachDir);
                this._moveAngularSpeed =
                    _n == 0 ? 0 : (angle + nextTarget.rotRoundReachRounds * 360) / _turnTime; //第一个点不需要转弯
                if (nextTarget.waitTime == 0)
                {
                    this._waitAngularSpeed = 0;
                }
                else
                {
                    if (_n == 0) //第一个点等到了目标点再转弯
                    {
                        angle = GetAngle(unit.Rotation.eulerAngles, nextTarget.rotRoundLeaveDir);
                    }
                    else
                    {
                        angle = GetAngle(nextTarget.rotRoundReachDir, nextTarget.rotRoundLeaveDir);
                    }

                    this._waitAngularSpeed = (angle + nextTarget.rotRoundWaitRounds * 360) / _waitTime;
                }

                return;
            }

            if (_rotType == RotType.ROT_AUTO)
            {
                this._turnTime = _needTime;

                // 要用transform的位置
                if (faceV.sqrMagnitude < 0.0001f)
                {
                    return;
                }

                this._rotRoundReachDir = unit.Rotation;
                this._rotRoundLeaveDir = this._rotRoundReachDir;
                this._moveAngularSpeed = 0; //移动不需要转弯
                if (nextTarget.waitTime == 0)
                {
                    this._waitAngularSpeed = 0;
                }
                else
                {
                    var next = GetPointAfterNextTarget();
                    var nextFaceV = next.pos - nextTarget.pos;
                    var to = Quaternion.LookRotation(nextFaceV, Vector3.up);
                    var angle = GetAngle(unit.Rotation.eulerAngles, to.eulerAngles);
                    this._waitAngularSpeed = angle / _waitTime;
                }

                return;
            }

            //无转向
            this._moveAngularSpeed = 0;
            this._waitAngularSpeed = 0;
            if (Mathf.Abs(faceV.x) > 0.01 || Mathf.Abs(faceV.z) > 0.01)
            {
                this._rotRoundReachDir = unit.Rotation;
                this._rotRoundLeaveDir = unit.Rotation;
            }

            this._turnTime = 0;
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
            if (_route.rotAngleType == RotAngleType.ROT_ANGLE_X)
            {
                res = to.x - from.x;
            }
            else if (_route.rotAngleType == RotAngleType.ROT_ANGLE_Y)
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
            return this.nextTarget.pos - GetParent<Unit>().Position;
        }

        /// <summary>
        /// 获取NextTarget的下一个点
        /// </summary>
        /// <returns></returns>
        private ConfigWaypoint GetPointAfterNextTarget()
        {
            if (_n == _targets.Length - 1)
            {
                if (_routeType == RouteType.Reciprocate) //终点作为起点
                {
                    return _targets[_n - 1];
                }
                else if (_routeType == RouteType.Loop) //终点作为起点
                {
                    return _targets[1];
                }
                else if (_routeType == RouteType.OneWay)
                {
                }

                return _targets[_n];
            }

            return _targets[_n + 1];
        }

        public void OnStop()
        {
            isStart = false;
            isPause = false;
            onMoveStateChange?.Invoke(false);
            this._beginTime = 0;
            _enable = false;
            this._needTime = 0;
            this._waitTime = 0;
        }

        public void Pause()
        {
            if (!isPause)
            {
                isPause = true;
                onMoveStateChange?.Invoke(false);
            }
            _enable = false;
        }

        /// <summary>
        /// 停止后重新唤醒
        /// </summary>
        public void Resume()
        {
            if (this.isPause) //广播过事件后被重新唤醒
            {
                _reachStop = false;
                isPause = false;
                onMoveStateChange?.Invoke(true);
            }
            _enable = true;
        }

        /// <summary>
        /// 设置路径
        /// </summary>
        /// <param name="route"></param>
        /// <param name="startDelay">延时启动</param>
        public void SetRoute(ConfigRoute route, int startDelay = -1)
        {
            if (_route != null)
            {
                OnStop();
            }

            _route = route;
            if (route != null)
            {
                _rotType = _route.rotType;
                _routeType = _route.type;
                //来回路径的返回路径点
                if (_routeType == RouteType.Reciprocate)
                {
                    _backPoints = new ConfigWaypoint[_route.points.Length];
                    for (int i = 0; i < _route.points.Length; i++)
                    {
                        _backPoints[_route.points.Length - 1 - i] = _route.points[i];
                    }
                }

                _sqlArriveRange = _route.arriveRange * _route.arriveRange;
                if (_route.rotAngleType == RotAngleType.ROT_ANGLE_X)
                {
                    _axis = Vector3.forward;
                }
                else if (_route.rotAngleType == RotAngleType.ROT_ANGLE_Y)
                {
                    _axis = Vector3.up;
                }
                else
                {
                    _axis = Vector3.left;
                }

                DelayStart(startDelay);
            }
        }

        /// <summary>
        /// 延时启动
        /// </summary>
        /// <param name="delay"></param>
        public void DelayStart(int delay)
        {
            OnStart();
            if (delay > 0)
            {
                _delay = delay;
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
                _delay = delay;
            }
        }
    }
}
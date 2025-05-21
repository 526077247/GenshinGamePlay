using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class TriggerComponent: Component, IComponent<ConfigTrigger>,IComponent<ConfigShape,int>
    {
        [Timer(TimerType.TriggerCheck)]
        public class TriggerCheck: ATimer<TriggerComponent>
        {
            public override void Run(TriggerComponent c)
            {
                try
                {
                    c.Check();
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }
        
        private ConcernType concernType;
        private ConfigShape shape;
        private float aabbRange;
        private TriggerCheckType checkType;
        private int configCheckCount;
        private uint configTriggerCount;
        private int configTriggerInterval;
        private uint configTotalTriggerCount;
        private TargetType triggerFlag;
        private Vector3 offset;

        private long lifeTimerId;
        private long checkTimerId;
        private int checkCount = 0;
        private CheckRangeType checkRangeType;
        private SceneEntity pSceneEntity => GetParent<SceneEntity>();
        private EntityManager em => parent.Parent;
        private MapScene map;
        private ListComponent<long> entities;
        private ListComponent<long> lastEntities;

        public int HitCount { get; private set; } = 0;
        public readonly HitInfo[] HitInfos = new HitInfo[64];
        public event Action<long> OnTriggerEnterEvt;
        public event Action<long> OnTriggerExitEvt;
        public event Action<long> OnTriggerEvt;

        private DictionaryComponent<long, long> lastTriggerTime;
        private DictionaryComponent<long, uint> triggerCount;
        private ListComponent<(Vector3, Quaternion)> checkLerpValue;
        private uint totalTriggerCount;
        private Vector3 lastPos;
        private Quaternion lastRot;
        public void Init(ConfigTrigger configTrigger)
        {
            offset = configTrigger.Offset;
            concernType = configTrigger.ConcernType;
            shape = configTrigger.ConfigShape;
            aabbRange = Mathf.Max(shape.GetAABBRange(), 0.1f);
            configCheckCount = configTrigger.CheckCount;
            checkType = configTrigger.CheckType;
            triggerFlag = configTrigger.TriggerFlag;
            configTriggerInterval = configTrigger.TriggerInterval;
            configTriggerCount = configTrigger.TriggerCount;
            configTotalTriggerCount = configTrigger.TotalTriggerCount;
            checkRangeType = configTrigger.CheckRangeType;
            map = SceneManager.Instance.GetCurrentScene<MapScene>();
            entities = ListComponent<long>.Create();
            lastTriggerTime = DictionaryComponent<long, long>.Create();
            triggerCount = DictionaryComponent<long, uint>.Create();
            totalTriggerCount = 0;
            if (configTrigger.StartCheckTime > 0)
            {
                StartCheckAsync(configTrigger.StartCheckTime,configTrigger.CheckInterval,configTrigger.LifeTime).Coroutine();
            }
            else
            {
                StartCheck(configTrigger.CheckInterval,configTrigger.LifeTime);
            }
        }

        public void Init(ConfigShape configShape,int checkInterval)
        {
            offset = Vector3.zero;
            shape = configShape;
            aabbRange = Mathf.Max(shape.GetAABBRange(), 0.1f);
            concernType = ConcernType.AllExcludeGWGO;
            configCheckCount = -1;
            checkType = TriggerCheckType.Point;
            checkRangeType = CheckRangeType.None;
            triggerFlag = TargetType.All;
            configTriggerInterval = 0;
            configTriggerCount = uint.MaxValue;
            configTotalTriggerCount = uint.MaxValue;
            totalTriggerCount = 0;
            
            map = SceneManager.Instance.GetCurrentScene<MapScene>();
            entities = ListComponent<long>.Create();
            lastTriggerTime = DictionaryComponent<long, long>.Create();
            triggerCount = DictionaryComponent<long, uint>.Create();
            StartCheck(checkInterval, -1);
        }

        private async ETTask StartCheckAsync(long startCheckTime,long checkInterval,long lifeTime)
        {
            await GameTimerManager.Instance.WaitAsync(startCheckTime);
            if(IsDispose) return;
            StartCheck(checkInterval,lifeTime);
        }

        private void StartCheck(long checkInterval,long lifeTime)
        {
            checkCount = 0;
            if (checkInterval < Define.MinRepeatedTimerInterval)
            {
                checkTimerId = GameTimerManager.Instance.NewFrameTimer(TimerType.TriggerCheck, this);
            }
            else
            {
                checkTimerId = GameTimerManager.Instance.NewRepeatedTimer(checkInterval, TimerType.TriggerCheck, this);
            }
            if (lifeTime > 0)
            {
                lifeTimerId = GameTimerManager.Instance.NewOnceTimer(GameTimerManager.Instance.GetTimeNow() + lifeTime,
                    TimerType.RemoveComponent, this);
            }
            lastPos = pSceneEntity.Position;
            lastRot = pSceneEntity.Rotation;
        }
        

        public void Destroy()
        {
            GameTimerManager.Instance.Remove(ref checkTimerId);
            GameTimerManager.Instance.Remove(ref lifeTimerId);
            if (entities != null)
            {
                entities.Dispose();
                entities = null;
            }

            lastEntities = null;
            if (lastTriggerTime != null)
            {
                lastTriggerTime.Dispose();
                lastTriggerTime = null;
            }

            if (triggerCount != null)
            {
                triggerCount.Dispose();
                triggerCount = null;
            }
            shape = null;
        }


        private void Check()
        {
            HitCount = 0;
            lastEntities = entities;
            entities = ListComponent<long>.Create();
            if (checkType == TriggerCheckType.Collider)
            {
                EntityType[] filter = AttackHelper.ActorEntityType;
                if (concernType == ConcernType.AllAvatars || concernType == ConcernType.LocalAvatar)
                {
                    filter = AttackHelper.AvatarEntityType;
                }

                var count = shape.RaycastHitInfo(pSceneEntity.Position, pSceneEntity.Rotation,
                    filter, out HitInfo[] hitInfos);

                for (int i = 0; i < count; i++)
                {
                    var sceneEntity = em.Get<SceneEntity>(hitInfos[i].EntityId);
                    if (sceneEntity != null)
                    {
                        if (concernType == ConcernType.CombatExcludeGWGO)
                        {
                            if(sceneEntity.GetComponent<CombatComponent>() == null) 
                                continue;
                        }
                        AddHitInfo(hitInfos[i]);
                        entities.Add(sceneEntity.Id);
                    }
                    
                }
            }
            else
            {
                switch (concernType)
                {
                    case ConcernType.LocalAvatar:
                        CheckItem(map.Self);
                        break;
                    case ConcernType.AllAvatars:
                        var avatars = em.GetAll<Avatar>();
                        for (int i = 0; i < avatars.Count; i++)
                        {
                            CheckItem(avatars[i]);
                        }
                        break;
                    default:
                        //加到列表里遍历，防止遍历中Dict变化报错
                        using (ListComponent<SceneEntity> targets = ListComponent<SceneEntity>.Create())
                        {
                            var all = em.GetAllDict();
                            foreach (var item in all)
                            {
                                if(!(item.Value is SceneEntity sceneEntity)) continue;
                                if (concernType == ConcernType.CombatExcludeGWGO)
                                {
                                    if(sceneEntity.GetComponent<CombatComponent>() == null) 
                                        continue;
                                    targets.Add(sceneEntity);
                                }
                                else if (concernType == ConcernType.AllExcludeGWGO)
                                {
                                    targets.Add(sceneEntity);
                                }
                            }
                            for (int i = 0; i < targets.Count; i++)
                            {
                                CheckItem(targets[i]);
                            }
                        }

                        break;
                    
                }

            }
            checkCount++;
            if (configCheckCount > 0 && checkCount > configCheckCount)
            {
                GameTimerManager.Instance.Remove(ref checkTimerId);
            }

            using (DictionaryComponent<long, int> change = DictionaryComponent<long, int>.Create())
            {
                for (int i = 0; i < entities.Count; i++)
                {
                    change[entities[i]] = 1;
                }

                if (lastEntities != null)
                {
                    for (int i = 0; i < lastEntities.Count; i++)
                    {
                        if (change.ContainsKey(lastEntities[i]))
                        {
                            change[lastEntities[i]]--;
                        }
                        else
                        {
                            change[lastEntities[i]] = -1;
                        }
                    }
                }
              
                foreach (var item in change)
                {
                    if (item.Value >= 0)
                    {
                        var id = item.Key;
                        if (item.Value > 0) //第一次进度
                        {
                            OnTriggerEnterEvt?.Invoke(id);
                            if (IsDispose)
                            {
                                OnTriggerExitEvt?.Invoke(id);
                                return;
                            }
                        }
                        
                        var timeNow = GameTimerManager.Instance.GetTimeNow();
                        if (lastTriggerTime.TryGetValue(id, out var lastTime))
                        {
                            if (lastTime + configTriggerInterval > timeNow)
                            {
                                continue;
                            }
                        }
                        if (!triggerCount.TryGetValue(id, out var count))
                        {
                            triggerCount[id] = count = 0;
                        }
                        if (count >= configTriggerCount)
                        {
                            continue;
                        }
                        triggerCount[id]++;
                        if (totalTriggerCount >= configTotalTriggerCount)
                        {
                            continue;
                        }
                        totalTriggerCount++;
                        lastTriggerTime[id] = timeNow;
                        OnTriggerEvt?.Invoke(item.Key);
                        if (IsDispose)
                        {
                            OnTriggerExitEvt?.Invoke(id);
                            return;
                        }
                    }
                    else 
                    {
                        OnTriggerExitEvt?.Invoke(item.Key);
                        if (IsDispose)
                        {
                            return;
                        }
                    }
                }
            }
            
            
            lastEntities?.Dispose();
            lastEntities = null;

            if (checkRangeType != CheckRangeType.None)
            {
                lastPos = pSceneEntity.Position;
                lastRot = pSceneEntity.Rotation;
            }
            checkLerpValue?.Dispose();
            checkLerpValue = null;
        }
        private void CheckItem(SceneEntity sceneEntity)
        {
            if (parent == null || parent.IsDispose || sceneEntity== null || sceneEntity.IsDispose ||
                !AttackHelper.CheckIsTarget(parent, sceneEntity, triggerFlag))
                return;
            if (InRange(sceneEntity))
            {
                entities.Add(sceneEntity.Id);
            }
        }

        private bool InRange(SceneEntity sceneEntity)
        {
            switch (checkRangeType)
            {
                case CheckRangeType.EachModel:
                    var umc = parent.GetComponent<UnitModelComponent>();
                    if (umc?.EntityView == null) return false;
                    for (var node = umc.Holders.First; node != null; node = node.Next)
                    {
                        if (node.Value != null && node.Value.EntityView != null)
                        {
                            if (InRange(sceneEntity, node.Value.EntityView.position, node.Value.EntityView.rotation))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                case CheckRangeType.Raycast:
                    //todo: 射线检测
                    return InRange(sceneEntity, pSceneEntity.Position, pSceneEntity.Rotation);
                case CheckRangeType.Lerp:
                    if (checkLerpValue == null)
                    {
                        checkLerpValue = ListComponent<(Vector3, Quaternion)>.Create();
                        var dis = (pSceneEntity.Position - lastPos).magnitude;
                        var count = Mathf.Min(10, dis / aabbRange);
                        if (count < 1)
                        {
                            checkLerpValue.Add((pSceneEntity.Position,pSceneEntity.Rotation));
                        }
                        else
                        {
                            for (int i = 0; i <= count; i++)
                            {
                                var progress = i / Mathf.Ceil(count);
                                checkLerpValue.Add((Vector3.Lerp(lastPos, pSceneEntity.Position, progress),
                                    Quaternion.Lerp(lastRot, pSceneEntity.Rotation, progress)));
                            }
                        }
                    }

                    for (int i = 0; i < checkLerpValue.Count; i++)
                    {
                        if (InRange(sceneEntity, checkLerpValue[i].Item1, checkLerpValue[i].Item2))
                        {
                            return true;
                        }
                    }
                    return false;
                case CheckRangeType.None: 
                default:
                    return InRange(sceneEntity, pSceneEntity.Position, pSceneEntity.Rotation);
            }
        }

        private bool InRange(SceneEntity sceneEntity, Vector3 position, Quaternion rotation)
        {
            var offsetPos = position + offset;
            var targetPos = PhysicsHelper.Transformation(offsetPos, rotation, sceneEntity.Position);
            if (checkType == TriggerCheckType.Point)
            {
                var res = shape.Contains(targetPos);
                if (res) AddHitInfo(sceneEntity.Position, sceneEntity.Id);
                return res;
            }

            if (checkType == TriggerCheckType.ModelHeight)
            {
                if (sceneEntity is Actor actor && actor.ConfigActor?.Common != null)
                {
                    var headPos = sceneEntity.Position + Vector3.up * actor.ConfigActor.Common.ModelHeight;
                    var headTargetPos = PhysicsHelper.Transformation(offsetPos, rotation, headPos);
                    var res = shape.ContainsLine(targetPos, headTargetPos);
                    if (res) AddHitInfo(offsetPos, sceneEntity.Id);
                    return res;
                }

                if (shape.Contains(targetPos))
                {
                    AddHitInfo(sceneEntity.Position, sceneEntity.Id);
                    return true;
                }

                return false;
            }

            return true;
        }

        private void AddHitInfo(Vector3 hitPos, long id, HitBoxType type = HitBoxType.Normal, Vector3 hitDir = default)
        {
            if (HitCount == HitInfos.Length) return;
            HitInfos[HitCount] = new HitInfo()
            {
                EntityId = id,
                Distance = 0,
                HitBoxType = type,
                HitPos = hitPos,
                HitDir = hitDir,
            };
            HitCount++;
        }

        private void AddHitInfo(HitInfo hitInfo)
        {
            if (HitCount == HitInfos.Length) return;
            HitInfos[HitCount] = hitInfo;
            HitCount++;
        }
    }
}
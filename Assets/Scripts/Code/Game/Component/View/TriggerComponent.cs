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
        private TriggerCheckType checkType;
        private int configCheckCount;
        private uint configTriggerCount;
        private int configTriggerInterval;
        private uint configTotalTriggerCount;
        private TargetType triggerFlag;
        private Vector3 offset;
        private bool applyEachModel;

        private long lifeTimerId;
        private long checkTimerId;
        private int checkCount = 0;
        private SceneEntity pSceneEntity => GetParent<SceneEntity>();
        private EntityManager em => parent.Parent;
        private MapScene map;
        public ListComponent<Entity> Entities;
        public event Action<Entity> OnTriggerEnterEvt;
        public event Action<Entity> OnTriggerExitEvt;

        private DictionaryComponent<long, long> lastTriggerTime;
        private DictionaryComponent<long, uint> triggerCount;
        private uint totalTriggerCount;
        public void Init(ConfigTrigger configTrigger)
        {
            offset = configTrigger.Offset;
            concernType = configTrigger.ConcernType;
            shape = configTrigger.ConfigShape;
            configCheckCount = configTrigger.CheckCount;
            checkType = configTrigger.CheckType;
            triggerFlag = configTrigger.TriggerFlag;
            configTriggerInterval = configTrigger.TriggerInterval;
            configTriggerCount = configTrigger.TriggerCount;
            configTotalTriggerCount = configTrigger.TotalTriggerCount;
            applyEachModel = configTrigger.ApplyEachModel;
            
            map = SceneManager.Instance.GetCurrentScene<MapScene>();
            Entities = ListComponent<Entity>.Create();
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
            concernType = ConcernType.AllExcludeGWGO;
            configCheckCount = -1;
            checkType = TriggerCheckType.Point;
            triggerFlag = TargetType.All;
            configTriggerInterval = 0;
            configTriggerCount = uint.MaxValue;
            configTotalTriggerCount = uint.MaxValue;
            totalTriggerCount = 0;
            applyEachModel = false;
            
            map = SceneManager.Instance.GetCurrentScene<MapScene>();
            Entities = ListComponent<Entity>.Create();
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
        }
        

        public void Destroy()
        {
            GameTimerManager.Instance.Remove(ref checkTimerId);
            GameTimerManager.Instance.Remove(ref lifeTimerId);
            if (Entities != null)
            {
                Entities.Dispose();
                Entities = null;
            }

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
            if (checkType == TriggerCheckType.Collider)
            {
                if (checkType == TriggerCheckType.Collider)
                {
                    EntityType[] filter = AttackHelper.ActorEntityType;
                    if (concernType == ConcernType.AllAvatars || concernType == ConcernType.LocalAvatar)
                    {
                        filter = AttackHelper.AvatarEntityType;
                    }

                    var count = shape.RaycastEntities(pSceneEntity.Position, pSceneEntity.Rotation,
                        filter, out long[] ids);

                    for (int i = 0; i < count; i++)
                    {
                        var sceneEntity = em.Get<SceneEntity>(ids[i]);
                        if (sceneEntity != null)
                        {
                            if (concernType == ConcernType.CombatExcludeGWGO)
                            {
                                if(sceneEntity.GetComponent<CombatComponent>() == null) 
                                    continue;
                            }
                            CheckItem(sceneEntity);
                        }
                        
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
                        var all = em.GetAllDict();
                        foreach (var item in all)
                        {
                            if(!(item.Value is SceneEntity sceneEntity)) continue;
                            if (concernType == ConcernType.CombatExcludeGWGO)
                            {
                                if(sceneEntity.GetComponent<CombatComponent>() == null) 
                                    continue;
                                CheckItem(sceneEntity);
                            }
                            else if (concernType == ConcernType.AllExcludeGWGO)
                            {
                                CheckItem(sceneEntity);
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
        }
        private void CheckItem(SceneEntity sceneEntity)
        {
            if (!AttackHelper.CheckIsTarget(parent, sceneEntity, triggerFlag))
                return;
            if (InRange(sceneEntity))
            {
                if (!Entities.Contains(sceneEntity))
                {
                    var timeNow = GameTimerManager.Instance.GetTimeNow();
                    if (lastTriggerTime.TryGetValue(sceneEntity.Id, out var lastTime))
                    {
                        if (lastTime + configTriggerInterval > timeNow)
                        {
                            return;
                        }
                    }

                    if (!triggerCount.TryGetValue(sceneEntity.Id, out var count))
                    {
                        triggerCount[sceneEntity.Id] = count = 0;
                    }
               
                    if (count >= configTriggerCount)
                    {
                        return;
                    }
                    triggerCount[sceneEntity.Id]++;
                
                    if (totalTriggerCount >= configTotalTriggerCount)
                    {
                        return;
                    }
                    totalTriggerCount++;
                    lastTriggerTime[sceneEntity.Id] = timeNow;
                    Entities.Add(sceneEntity);
                    OnTriggerEnterEvt?.Invoke(sceneEntity);
                }
            }
            else
            {
                if (Entities.Contains(sceneEntity))
                {
                    Entities.Remove(sceneEntity);
                    OnTriggerExitEvt?.Invoke(sceneEntity);
                }
            }
        }

        private bool InRange(SceneEntity sceneEntity)
        {
            if (applyEachModel)
            {
                var umc = parent.GetComponent<UnitModelComponent>();
                if (umc?.EntityView == null) return false;
                for (var node = umc.Holders.First; node.Next != null; node = node.Next)
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
            }

            return InRange(sceneEntity, pSceneEntity.Position, pSceneEntity.Rotation);
        }

        private bool InRange(SceneEntity sceneEntity, Vector3 position, Quaternion rotation)
        {
            var targetPos = PhysicsHelper.Transformation(position + offset,  rotation, sceneEntity.Position);
            if (checkType == TriggerCheckType.Point)
            {
                return shape.Contains(targetPos);
            }
            if (checkType == TriggerCheckType.ModelHeight)
            {
                if (shape.Contains(targetPos))
                {
                    return true;
                }
                if (sceneEntity is Actor actor && actor.ConfigActor?.Common!=null)
                {
                    //todo:穿过的情况
                    return shape.Contains(targetPos + Vector3.up * actor.ConfigActor.Common.ModelHeight);
                }
                return false;
            }
            return true;
        }
    }
}
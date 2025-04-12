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
        private TargetType triggerFlag;

        private long lifeTimerId;
        private long checkTimerId;
        private int checkCount = 0;
        private SceneEntity pSceneEntity => GetParent<SceneEntity>();
        private EntityManager em => parent.Parent;
        private MapScene map;
        public ListComponent<Entity> Entities;
        public event Action<Entity> OnTriggerEnterEvt;
        public event Action<Entity> OnTriggerExitEvt;
        public void Init(ConfigTrigger configTrigger)
        {
            concernType = configTrigger.ConcernType;
            shape = configTrigger.ConfigShape;
            configCheckCount = configTrigger.CheckCount;
            checkType = configTrigger.CheckType;
            triggerFlag = configTrigger.TriggerFlag;
            
            map = SceneManager.Instance.GetCurrentScene<MapScene>();
            Entities = ListComponent<Entity>.Create();

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
            shape = configShape;
            concernType = ConcernType.AllExcludeGWGO;
            configCheckCount = -1;
            checkType = TriggerCheckType.Point;
            triggerFlag = TargetType.All;
            
            map = SceneManager.Instance.GetCurrentScene<MapScene>();
            Entities = ListComponent<Entity>.Create();

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

            shape = null;
        }


        private void Check()
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
            var targetPos = Transformation(pSceneEntity.Position, pSceneEntity.Rotation, sceneEntity.Position);
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
                    return shape.Contains(targetPos +
                                          Vector3.up * actor.ConfigActor.Common.ModelHeight);
                }
                return false;
            }

            if (checkType == TriggerCheckType.Collider)
            {
                //todo:
                Log.Error("TriggerCheckType.Collider InRange 未实现！");
            }
            
            return false;
        }
        
        /// <summary>
        /// 坐标系转换
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public Vector3 Transformation(Vector3 pos, Quaternion rot, Vector3 target)
        {
            var posTrans = target - pos;
            return Quaternion.Inverse(rot) * posTrans;
        }

    }
}
using System;
using System.Collections.Generic;
using CMF;
using UnityEngine;
using UnityEngine.Events;

namespace TaoTie
{
    public partial class GameObjectHolderComponent : Component, IComponent
    {
        [Timer(TimerType.DestroyEffect)]
        public class DestroyEffectTimer : ATimer<EffectInfo>
        {
            public override void Run(EffectInfo self)
            {
                try
                {
                    self.Dispose();
                }
                catch (Exception e)
                {
                    Log.Error($"move timer error: {self.ConfigId}\n{e}");
                }
            }
        }

        public Transform EntityView;

        private ReferenceCollector collector;

        private Queue<ETTask> waitFinishTask;

        #region override

        public void Init()
        {
            LoadGameObjectAsync().Coroutine();
        }

        private async ETTask LoadGameObjectAsync()
        {
            var unit = this.GetParent<Unit>();
            GameObject obj;
            if (unit.ConfigId < 0)//约定小于0的id都是用空物体
            {
                obj = new GameObject("Empty");
            }
            else
            {
                obj = await GameObjectPoolManager.Instance.GetGameObjectAsync(unit.Config.Perfab);
                if (this.IsDispose)
                {
                    GameObjectPoolManager.Instance.RecycleGameObject(obj);
                    return;
                }
            }

            animator = obj.GetComponentInChildren<Animator>();
            if (animator != null && !string.IsNullOrEmpty(unit.Config.Controller))
            {
                animator.runtimeAnimatorController =
                    ResourcesManager.Instance.Load<RuntimeAnimatorController>(unit.Config.Controller);
                var fsm = parent.GetComponent<FsmComponent>();
                if (fsm != null && fsm.Config.ParamDict != null)
                {
                    foreach (var item in fsm.Config.ParamDict)
                    {
                        var para = item.Value;
                        if (para is ConfigParamBool paramBool)
                        {
                            SetData(paramBool.Key, fsm.GetBool(paramBool.Key));
                        }
                        else if (para is ConfigParamFloat paramFloat)
                        {
                            SetData(paramFloat.Key, fsm.GetFloat(paramFloat.Key));
                        }
                        else if (para is ConfigParamInt paramInt)
                        {
                            SetData(paramInt.Key, fsm.GetInt(paramInt.Key));
                        }
                        else if (para is ConfigParamTrigger paramTrigger)
                        {
                            SetData(paramTrigger.Key, fsm.GetBool(paramTrigger.Key));
                        }
                    }

                    for (int i = 0; i < fsm.Fsms.Length; i++)
                    {
                        CrossFade(fsm.Fsms[i].currentState.Name, fsm.Fsms[i].config.LayerIndex);
                    }
                }
            }

            EntityView = obj.transform;
            collector = obj.GetComponent<ReferenceCollector>();
            EntityView.SetParent(this.parent.Parent.GameObjectRoot);
            var ec = obj.GetComponent<EntityComponent>();
            if (ec == null) ec = obj.AddComponent<EntityComponent>();
            ec.Id = this.Id;
            ec.EntityType = unit.Type;
            if (unit is Actor actor)
            {
                ec.CampId = actor.CampId;
                EntityView.localScale = Vector3.one * actor.configActor.Common.Scale;
            }

            EntityView.position = unit.Position;
            EntityView.rotation = unit.Rotation;
            Messager.Instance.AddListener<Unit, Vector3>(Id, MessageId.ChangePositionEvt, OnChangePosition);
            Messager.Instance.AddListener<Unit, Quaternion>(Id, MessageId.ChangeRotationEvt, OnChangeRotation);
            Messager.Instance.AddListener<AIMoveSpeedLevel>(Id, MessageId.UpdateMotionFlag, UpdateMotionFlag);
            Messager.Instance.AddListener<string, float, int, float>(Id, MessageId.CrossFadeInFixedTime,
                CrossFadeInFixedTime);
            Messager.Instance.AddListener<string, int>(Id, MessageId.SetAnimDataInt, SetData);
            Messager.Instance.AddListener<string, float>(Id, MessageId.SetAnimDataFloat, SetData);
            Messager.Instance.AddListener<string, bool>(Id, MessageId.SetAnimDataBool, SetData);
            // var hud = unit.GetComponent<HudComponent>();
            // if (hud != null)
            // {
            //     HudSystem hudSys = ManagerProvider.GetManager<HudSystem>();
            //     hudSys?.ShowHeadInfo(hud.Info);
            // }
            if (waitFinishTask != null)
            {
                while (waitFinishTask.TryDequeue(out var task))
                {
                    task.SetResult();
                }

                waitFinishTask = null;
            }
            
        }

        public void Destroy()
        {
            Messager.Instance.RemoveListener<Unit, Vector3>(Id, MessageId.ChangePositionEvt, OnChangePosition);
            Messager.Instance.RemoveListener<Unit, Quaternion>(Id, MessageId.ChangeRotationEvt, OnChangeRotation);
            Messager.Instance.RemoveListener<string, int>(Id, MessageId.SetAnimDataInt, SetData);
            Messager.Instance.RemoveListener<string, float>(Id, MessageId.SetAnimDataFloat, SetData);
            Messager.Instance.RemoveListener<string, bool>(Id, MessageId.SetAnimDataBool, SetData);
            Messager.Instance.RemoveListener<string, float, int, float>(Id, MessageId.CrossFadeInFixedTime,
                CrossFadeInFixedTime);

            if (EntityView != null)
            {
                var unit = this.GetParent<Unit>();
                if (unit.ConfigId < 0)
                {
                    GameObject.Destroy(EntityView.gameObject);
                }
                else
                {
                    GameObjectPoolManager.Instance.RecycleGameObject(EntityView.gameObject);
                }
            }

            if (waitFinishTask != null)
            {
                while (waitFinishTask.TryDequeue(out var task))
                {
                    task.SetResult();
                }
                waitFinishTask = null;
            }

            if (animator != null)
            {
                ResourcesManager.Instance.ReleaseAsset(animator.runtimeAnimatorController);
                animator = null;
            }
        }

        #endregion

        public void OnChangePosition(Unit unit, Vector3 old)
        {
            EntityView.position = unit.Position;
        }

        public void OnChangeRotation(Unit unit, Quaternion old)
        {
            EntityView.rotation = unit.Rotation;
        }

        public async ETTask WaitLoadGameObjectOver()
        {
            if (EntityView == null)
            {
                ETTask task = ETTask.Create(true);
                if (waitFinishTask == null)
                    waitFinishTask = new Queue<ETTask>();
                waitFinishTask.Enqueue(task);
                await task;
            }
        }

        public T GetCollectorObj<T>(string name) where T : class
        {
            if (collector == null) return null;
            return collector.Get<T>(name);
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TaoTie
{
    public partial class GameObjectHolderComponent : Component, IComponent, IComponent<string>
    {
        public Transform EntityView;

        private ReferenceCollector collector;

        private Queue<ETTask> waitFinishTask;

        #region override

        public void Init()
        {
            if(fsm?.DefaultFsm?.CurrentState!=null)
                fsmUseRagDoll = fsm.DefaultFsm.CurrentState.UseRagDoll;
            Messager.Instance.AddListener<bool>(Id,MessageId.SetUseRagDoll,FSMSetUseRagDoll);
            Messager.Instance.AddListener<ConfigDie, DieStateFlag>(Id, MessageId.OnBeKill, OnBeKill);
            LoadGameObjectAsync().Coroutine();
        }

        public void Init(string path)
        {
            if(fsm?.DefaultFsm?.CurrentState!=null)
                fsmUseRagDoll = fsm.DefaultFsm.CurrentState.UseRagDoll;
            Messager.Instance.AddListener<bool>(Id,MessageId.SetUseRagDoll,FSMSetUseRagDoll);
            Messager.Instance.AddListener<ConfigDie, DieStateFlag>(Id, MessageId.OnBeKill, OnBeKill);
            LoadGameObjectAsync(path).Coroutine();
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
                obj = await GameObjectPoolManager.GetInstance().GetGameObjectAsync(unit.Config.Perfab);
                if (this.IsDispose)
                {
                    GameObjectPoolManager.GetInstance().RecycleGameObject(obj);
                    return;
                }
            }

            Animator = obj.GetComponentInChildren<Animator>();
            if (Animator != null && !string.IsNullOrEmpty(unit.Config.Controller))
            {
                Animator.runtimeAnimatorController = await 
                    ResourcesManager.Instance.LoadAsync<RuntimeAnimatorController>(unit.Config.Controller);
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
                        CrossFade(fsm.Fsms[i].CurrentState.Name, fsm.Fsms[i].Config.LayerIndex);
                    }
                }
            }

            EntityView = obj.transform;
            collector = obj.GetComponent<ReferenceCollector>();
            EntityView.SetParent(this.parent.Parent.GameObjectRoot);
            var ec = obj.GetComponent<EntityComponent>();
            if (ec == null) ec = obj.AddComponent<EntityComponent>();
            ec.Id = this.Id;
            ec.EntityType = parent.Type;
            if (parent is Actor actor)
            {
                ec.CampId = actor.CampId;
                EntityView.localScale = Vector3.one * actor.configActor.Common.Scale;
            }

            EntityView.position = unit.Position;
            EntityView.rotation = unit.Rotation;
            Messager.Instance.AddListener<Unit, Vector3>(Id, MessageId.ChangePositionEvt, OnChangePosition);
            Messager.Instance.AddListener<Unit, Quaternion>(Id, MessageId.ChangeRotationEvt, OnChangeRotation);
            Messager.Instance.AddListener<Unit, Vector3>(Id, MessageId.ChangeScaleEvt, OnChangeScale);
            Messager.Instance.AddListener<string, float, int, float>(Id, MessageId.CrossFadeInFixedTime,
                CrossFadeInFixedTime);
            Messager.Instance.AddListener<string, int>(Id, MessageId.SetAnimDataInt, SetData);
            Messager.Instance.AddListener<string, float>(Id, MessageId.SetAnimDataFloat, SetData);
            Messager.Instance.AddListener<string, bool>(Id, MessageId.SetAnimDataBool, SetData);
            UpdateRagDollState();
            if (waitFinishTask != null)
            {
                while (waitFinishTask.TryDequeue(out var task))
                {
                    task.SetResult();
                }

                waitFinishTask = null;
            }
            
        }

        private async ETTask LoadGameObjectAsync(string path)
        {
            var obj = await GameObjectPoolManager.GetInstance().GetGameObjectAsync(path);
            if (this.IsDispose)
            {
                GameObjectPoolManager.GetInstance().RecycleGameObject(obj);
                return;
            }
            Animator = obj.GetComponentInChildren<Animator>();
            EntityView = obj.transform;
            collector = obj.GetComponent<ReferenceCollector>();
            EntityView.SetParent(this.parent.Parent.GameObjectRoot);
            var ec = obj.GetComponent<EntityComponent>();
            if (ec == null) ec = obj.AddComponent<EntityComponent>();
            ec.Id = this.Id;
            ec.EntityType = parent.Type;
            if (parent is Actor actor)
            {
                ec.CampId = actor.CampId;
                EntityView.localScale = Vector3.one * actor.configActor.Common.Scale;
            }

            if (parent is Unit unit)
            {
                EntityView.position = unit.Position;
                EntityView.rotation = unit.Rotation;
                Messager.Instance.AddListener<Unit, Vector3>(Id, MessageId.ChangePositionEvt, OnChangePosition);
                Messager.Instance.AddListener<Unit, Quaternion>(Id, MessageId.ChangeRotationEvt, OnChangeRotation);
                Messager.Instance.AddListener<Unit, Vector3>(Id, MessageId.ChangeScaleEvt, OnChangeScale);
            }
            else if (parent is Effect effect)
            {
                EntityView.position = effect.Position;
                EntityView.rotation = effect.Rotation;
            }
            
            Messager.Instance.AddListener<string, float, int, float>(Id, MessageId.CrossFadeInFixedTime,
                CrossFadeInFixedTime);
            Messager.Instance.AddListener<string, int>(Id, MessageId.SetAnimDataInt, SetData);
            Messager.Instance.AddListener<string, float>(Id, MessageId.SetAnimDataFloat, SetData);
            Messager.Instance.AddListener<string, bool>(Id, MessageId.SetAnimDataBool, SetData);
            UpdateRagDollState();
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
            Messager.Instance.RemoveListener<bool>(Id,MessageId.SetUseRagDoll,FSMSetUseRagDoll);
            Messager.Instance.RemoveListener<Unit, Vector3>(Id, MessageId.ChangePositionEvt, OnChangePosition);
            Messager.Instance.RemoveListener<Unit, Quaternion>(Id, MessageId.ChangeRotationEvt, OnChangeRotation);
            Messager.Instance.RemoveListener<Unit, Vector3>(Id, MessageId.ChangeScaleEvt, OnChangeScale);
            Messager.Instance.RemoveListener<string, int>(Id, MessageId.SetAnimDataInt, SetData);
            Messager.Instance.RemoveListener<string, float>(Id, MessageId.SetAnimDataFloat, SetData);
            Messager.Instance.RemoveListener<string, bool>(Id, MessageId.SetAnimDataBool, SetData);
            Messager.Instance.RemoveListener<string, float, int, float>(Id, MessageId.CrossFadeInFixedTime,
                CrossFadeInFixedTime);
            Messager.Instance.RemoveListener<ConfigDie, DieStateFlag>(Id, MessageId.OnBeKill, OnBeKill);
            if (EntityView != null)
            {
                if (parent is Unit unit && unit.ConfigId < 0)
                {
                    GameObject.Destroy(EntityView.gameObject);
                }
                else
                {
                    GameObjectPoolManager.GetInstance().RecycleGameObject(EntityView.gameObject);
                }

                EntityView = null;
            }

            if (waitFinishTask != null)
            {
                while (waitFinishTask.TryDequeue(out var task))
                {
                    task.SetResult();
                }
                waitFinishTask = null;
            }

            if (Animator != null && Animator.runtimeAnimatorController != null)
            {
                ResourcesManager.Instance.ReleaseAsset(Animator.runtimeAnimatorController);
                Animator = null;
            }
        }

        #endregion

        #region Event
        
        private void OnChangePosition(Unit unit, Vector3 old)
        {
            if(EntityView == null) return;
            EntityView.position = unit.Position;
        }

        private void OnChangeRotation(Unit unit, Quaternion old)
        {
            if(EntityView == null) return;
            EntityView.rotation = unit.Rotation;
        }

        private void OnChangeScale(Unit unit, Vector3 old)
        {
            if(EntityView == null) return;
            EntityView.localScale = unit.LocalScale;
        }
        private void OnBeKill(ConfigDie configDie, DieStateFlag flag)
        {
            if (parent == null) return;
            if (configDie != null)
            {
                var unit = GetParent<Unit>();
                if (unit == null) return;
                //特效
                if (!string.IsNullOrWhiteSpace(configDie.DieDisappearEffect))
                {
                    var res = parent.Parent.CreateEntity<Effect, string, long>(configDie.DieDisappearEffect, configDie.DieDisappearEffectDelay);
                    res.Position = unit.Position;
                    res.Rotation = unit.Rotation;
                    parent.GetOrAddComponent<AttachComponent>().AddChild(res);
                }

                // 死亡动画
                if (configDie.HasAnimatorDie)
                {
                    fsm?.SetData(FSMConst.Die, true);
                }
                
                //布娃娃系统
                if (configDie.UseRagDoll)
                {
                    SetUseRagDoll(configDie.UseRagDoll);
                }
                
                // 消融
                if (configDie.DieShaderData != ShaderData.None)
                {
                   
                }
            }
        }
        #endregion
        /// <summary>
        /// 等待预制体加载完成，注意判断加载完之后Component是否已经销毁
        /// </summary>
        public async ETTask<bool> WaitLoadGameObjectOver()
        {
            if (EntityView == null)
            {
                ETTask task = ETTask.Create(true);
                if (waitFinishTask == null)
                    waitFinishTask = new Queue<ETTask>();
                waitFinishTask.Enqueue(task);
                await task;
            }
            return !IsDispose;
        }

        public T GetCollectorObj<T>(string name) where T : class
        {
            if (collector == null) return null;
            return collector.Get<T>(name);
        }
        
        /// <summary>
        /// 开启或关闭Renderer
        /// </summary>
        /// <param name="enable"></param>
        public async ETTask EnableRenderer(bool enable)
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockManager.Instance.Wait(CoroutineLockType.EnableObjView, parent.Id);
                await this.WaitLoadGameObjectOver();
                if(this.IsDispose) return;
                var renders = this.EntityView.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renders.Length; i++)
                {
                    renders[i].enabled = enable;
                }
            }
            finally
            {
                coroutineLock?.Dispose();
            }
            
        }
        
        /// <summary>
        /// 开启或关闭hitBox
        /// </summary>
        /// <param name="hitBox"></param>
        /// <param name="enable"></param>
        public async ETTask EnableHitBox(string hitBox, bool enable)
        {
            await this.WaitLoadGameObjectOver();
            if(this.IsDispose) return;
            this.GetCollectorObj<GameObject>(hitBox)?.SetActive(enable);
        }
    }
}
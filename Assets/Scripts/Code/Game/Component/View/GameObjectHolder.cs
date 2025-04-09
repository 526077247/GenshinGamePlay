using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public partial class GameObjectHolder : IDisposable
    {
        protected UnitModelComponent UnitModelComponent { get; private set; }
        protected Entity parent => UnitModelComponent.GetParent<Entity>();
        protected long Id => parent.Id;

        public static GameObjectHolder Create(UnitModelComponent unitModel, Transform tranParent)
        {
            GameObjectHolder res = ObjectPool.Instance.Fetch<GameObjectHolder>();
            res.Init(unitModel, tranParent);
            return res;
        }
        
        private void Init(UnitModelComponent unitModel, Transform tranParent)
        {
            UnitModelComponent = unitModel;
            if(fsm?.DefaultFsm?.CurrentState!=null)
                fsmUseRagDoll = fsm.DefaultFsm.CurrentState.UseRagDoll;
            Messager.Instance.AddListener<float>(0,MessageId.TimeScaleChange,ChangeTimeScale);
            Messager.Instance.AddListener<bool>(Id,MessageId.SetUseRagDoll,FSMSetUseRagDoll);
            Messager.Instance.AddListener<ConfigDie, DieStateFlag>(Id, MessageId.OnBeKill, OnBeKill);
            LoadGameObjectAsync(tranParent).Coroutine();
        }
        
        
        public void Dispose()
        {
            Destroy();
            UnitModelComponent = null;
            ObjectPool.Instance.Recycle(this);
        }
        
        public Transform EntityView;

        private ReferenceCollector collector;

        private Queue<ETTask> waitFinishTask;

        #region override

        private async ETTask LoadGameObjectAsync(Transform tranParent)
        {
            var unit = parent as Unit;
            GameObject obj = await GameObjectPoolManager.GetInstance().GetGameObjectAsync(unit.Config.Perfab);
            if (parent.IsDispose)
            {
                GameObjectPoolManager.GetInstance().RecycleGameObject(obj);
                return;
            }
            
            Animator = obj.GetComponentInChildren<Animator>();
            if (Animator != null && !string.IsNullOrEmpty(unit.Config.Controller))
            {
                Animator.runtimeAnimatorController = await 
                    ResourcesManager.Instance.LoadAsync<RuntimeAnimatorController>(unit.Config.Controller);
                Animator.speed = GameTimerManager.Instance.GetTimeScale();
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
            
            EntityView.SetParent(tranParent);
            EntityView.localScale = unit.LocalScale;
            EntityView.position = unit.Position;
            EntityView.rotation = unit.Rotation;
            
            collector = obj.GetComponent<ReferenceCollector>();
            var ec = obj.GetComponent<EntityComponent>();
            if (ec == null) ec = obj.AddComponent<EntityComponent>();
            ec.Id = this.Id;
            ec.EntityType = parent.Type;
            if (parent is Actor actor)
            {
                ec.CampId = actor.CampId;
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
        

        private void Destroy()
        {
            Messager.Instance.RemoveListener<float>(0,MessageId.TimeScaleChange,ChangeTimeScale);
            Messager.Instance.RemoveListener<bool>(Id,MessageId.SetUseRagDoll,FSMSetUseRagDoll);
            Messager.Instance.RemoveListener<string, int>(Id, MessageId.SetAnimDataInt, SetData);
            Messager.Instance.RemoveListener<string, float>(Id, MessageId.SetAnimDataFloat, SetData);
            Messager.Instance.RemoveListener<string, bool>(Id, MessageId.SetAnimDataBool, SetData);
            Messager.Instance.RemoveListener<string, float, int, float>(Id, MessageId.CrossFadeInFixedTime,
                CrossFadeInFixedTime);
            Messager.Instance.RemoveListener<ConfigDie, DieStateFlag>(Id, MessageId.OnBeKill, OnBeKill);
            if (EntityView != null)
            {
                var ec = EntityView.GetComponent<EntityComponent>();
                if (ec != null) GameObject.DestroyImmediate(ec);
                GameObjectPoolManager.GetInstance().RecycleGameObject(EntityView.gameObject);
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
                Animator.runtimeAnimatorController = null;
                Animator = null;
            }
        }

        #endregion

        #region Event
        private void OnBeKill(ConfigDie configDie, DieStateFlag flag)
        {
            if (parent == null) return;
            if (configDie != null)
            {
                var unit = parent as Unit;
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
            return !parent.IsDispose;
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
                if(parent.IsDispose) return;
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
            if(parent.IsDispose) return;
            this.GetCollectorObj<GameObject>(hitBox)?.SetActive(enable);
        }
    }
}
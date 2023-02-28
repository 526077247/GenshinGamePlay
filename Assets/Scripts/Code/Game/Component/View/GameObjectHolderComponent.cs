using System;
using System.Collections.Generic;
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

        public ReferenceCollector Collector;

        private Queue<ETTask> waitFinishTask;

        #region override

        public void Init()
        {
            LoadGameObjectAsync().Coroutine();
        }

        public async ETTask LoadGameObjectAsync()
        {
            var unit = this.GetParent<Unit>();
            var obj = await GameObjectPoolManager.Instance.GetGameObjectAsync(unit.Config.Perfab);
            if (this.IsDispose)
            {
                GameObjectPoolManager.Instance.RecycleGameObject(obj);
                return;
            }

            Animator = obj.GetComponentInChildren<Animator>();

            var fsm = Parent.GetComponent<FsmComponent>();
            if (fsm != null)
            {
                foreach (var item in fsm.config.paramDict)
                {
                    var para = item.Value;
                    if (para is ConfigParamBool paramBool)
                    {
                        SetData(paramBool.key, fsm.GetBool(paramBool.key));
                    }
                    else if (para is ConfigParamFloat paramFloat)
                    {
                        SetData(paramFloat.key, fsm.GetFloat(paramFloat.key));
                    }
                    else if (para is ConfigParamInt paramInt)
                    {
                        SetData(paramInt.key, fsm.GetInt(paramInt.key));
                    }
                    else if (para is ConfigParamTrigger paramTrigger)
                    {
                        SetData(paramTrigger.key, fsm.GetBool(paramTrigger.key));
                    }
                }

                for (int i = 0; i < fsm.fsms.Length; i++)
                {
                    CrossFade(fsm.fsms[i].currentState.name, fsm.fsms[i].config.layerIndex);
                }
            }

            EntityView = obj.transform;
            Collector = obj.GetComponent<ReferenceCollector>();
            EntityView.SetParent(this.Parent.Parent.GameObjectRoot);
            var ec = obj.GetComponent<EntityComponent>();
            if (ec == null) ec = obj.AddComponent<EntityComponent>();
            ec.Id = this.Id;

            ec.EntityType = unit.Type;
            EntityView.position = unit.Position;
            EntityView.rotation = unit.Rotation;
            Messager.Instance.AddListener<Unit, Vector3>(Id, MessageId.ChangePositionEvt, OnChanePosition);
            Messager.Instance.AddListener<Unit, Quaternion>(Id, MessageId.ChangeRotationEvt, OnChaneRotation);
            Messager.Instance.AddListener<int, float, int, float>(Id, MessageId.CrossFadeInFixedTime,
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
            Messager.Instance.RemoveListener<Unit, Vector3>(Id, MessageId.ChangePositionEvt, OnChanePosition);
            Messager.Instance.RemoveListener<Unit, Quaternion>(Id, MessageId.ChangeRotationEvt, OnChaneRotation);
            Messager.Instance.RemoveListener<string, int>(Id, MessageId.SetAnimDataInt, SetData);
            Messager.Instance.RemoveListener<string, float>(Id, MessageId.SetAnimDataFloat, SetData);
            Messager.Instance.RemoveListener<string, bool>(Id, MessageId.SetAnimDataBool, SetData);
            Messager.Instance.RemoveListener<int, float, int, float>(Id, MessageId.CrossFadeInFixedTime,
                CrossFadeInFixedTime);

            if (EntityView != null)
                GameObjectPoolManager.Instance.RecycleGameObject(EntityView.gameObject);
            while (waitFinishTask.TryDequeue(out var task))
            {
                task.SetResult();
            }

            waitFinishTask = null;
        }

        #endregion

        public void OnChanePosition(Unit unit, Vector3 old)
        {
            EntityView.position = unit.Position;
        }

        public void OnChaneRotation(Unit unit, Quaternion old)
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
            if (Collector == null) return null;
            return Collector.Get<T>(name);
        }
    }
}
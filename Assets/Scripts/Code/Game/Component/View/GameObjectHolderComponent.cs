using System;
using System.Collections.Generic;
using CMF;
using UnityEngine;
using UnityEngine.Events;

namespace TaoTie
{
    public partial class GameObjectHolderComponent : Component, IComponent, IUpdateComponent
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
            var obj = await GameObjectPoolManager.Instance.GetGameObjectAsync(unit.Config.Perfab);
            if (this.IsDispose)
            {
                GameObjectPoolManager.Instance.RecycleGameObject(obj);
                return;
            }

            animator = obj.GetComponentInChildren<Animator>();
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

            EntityView = obj.transform;
            collector = obj.GetComponent<ReferenceCollector>();
            EntityView.SetParent(this.parent.Parent.GameObjectRoot);
            var ec = obj.GetComponent<EntityComponent>();
            if (ec == null) ec = obj.AddComponent<EntityComponent>();
            ec.Id = this.Id;
            ec.EntityType = unit.Type;
            ec.CampId = unit.CampId;
            controller = obj.GetComponent<Controller>();
            if (controller is AdvancedWalkerController advancedWalkerController)
            {
                advancedWalkerController.characterInput = parent.GetComponent<AvatarMoveComponent>().InputData;
                advancedWalkerController.cameraTransform = CameraManager.Instance.MainCamera().transform;
            }

            EntityView.position = unit.Position;
            EntityView.rotation = unit.Rotation;
            Messager.Instance.AddListener<Unit, Vector3>(Id, MessageId.ChangePositionEvt, OnChangePosition);
            Messager.Instance.AddListener<Unit, Quaternion>(Id, MessageId.ChangeRotationEvt, OnChangeRotation);
            // Messager.Instance.AddListener<Unit, bool>(Id, MessageId.ChangeTurnEvt, OnChangeTurn);
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

            if (controller != null)
            {
                //Connect events to controller events;
                controller.OnLand += OnLand;
                controller.OnJump += OnJump;
            }
        }

        public void Destroy()
        {
            if (controller != null)
            {
                if (controller is AdvancedWalkerController advancedWalkerController)
                {
                    advancedWalkerController.characterInput = null;
                    advancedWalkerController.cameraTransform = null;
                }
                //Disconnect events to prevent calls to disabled gameobjects;
                controller.OnLand -= OnLand;
                controller.OnJump -= OnJump;
                controller = null;
            }
            
            Messager.Instance.RemoveListener<Unit, Vector3>(Id, MessageId.ChangePositionEvt, OnChangePosition);
            Messager.Instance.RemoveListener<Unit, Quaternion>(Id, MessageId.ChangeRotationEvt, OnChangeRotation);
            // Messager.Instance.RemoveListener<Unit, bool>(Id, MessageId.ChangeTurnEvt, OnChangeTurn);
            Messager.Instance.RemoveListener<string, int>(Id, MessageId.SetAnimDataInt, SetData);
            Messager.Instance.RemoveListener<string, float>(Id, MessageId.SetAnimDataFloat, SetData);
            Messager.Instance.RemoveListener<string, bool>(Id, MessageId.SetAnimDataBool, SetData);
            Messager.Instance.RemoveListener<string, float, int, float>(Id, MessageId.CrossFadeInFixedTime,
                CrossFadeInFixedTime);

            if (EntityView != null)
                GameObjectPoolManager.Instance.RecycleGameObject(EntityView.gameObject);
            while (waitFinishTask.TryDequeue(out var task))
            {
                task.SetResult();
            }

            waitFinishTask = null;
            if (animator != null)
            {
                ResourcesManager.Instance.ReleaseAsset(animator.runtimeAnimatorController);
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

        // public void OnChangeTurn(Unit unit, bool old)
        // {
        //     GameObject obj = GetCollectorObj<GameObject>("Obj");
        //     if (obj)
        //     {
        //         if ((unit.IsTurn && obj.transform.localScale.x > 0) || (!unit.IsTurn && obj.transform.localScale.x < 0))
        //         {
        //             obj.transform.localScale = new Vector3(-obj.transform.localScale.x, obj.transform.localScale.y, obj.transform.localScale.z);
        //         }
        //     }
        // }

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
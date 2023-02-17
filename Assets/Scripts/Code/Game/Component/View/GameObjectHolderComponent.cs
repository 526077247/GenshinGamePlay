using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TaoTie
{
    public partial class GameObjectHolderComponent:Component,IComponent
    {
        [Timer(TimerType.DestroyEffect)]
        public class DestroyEffectTimer: ATimer<EffectInfo>
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
            EntityView = obj.transform;
            Collector = obj.GetComponent<ReferenceCollector>();
            EntityView.SetParent(this.Parent.Parent.GameObjectRoot);
            var ec = obj.GetComponent<EntityComponent>();
            if (ec == null) ec = obj.AddComponent<EntityComponent>();
            ec.Id = this.Id;

            ec.EntityType = unit.Type;
            EntityView.position = unit.Position;
            EntityView.rotation = unit.Rotation;
            Messager.Instance.AddListener<Unit,Vector3>(Id,MessageId.ChangePositionEvt,OnChanePosition);
            Messager.Instance.AddListener<Unit,Quaternion>(Id,MessageId.ChangeRotationEvt,OnChaneRotation);
            // var hud = unit.GetComponent<HudComponent>();
            // if (hud != null)
            // {
            //     HudSystem hudSys = ManagerProvider.GetManager<HudSystem>();
            //     hudSys?.ShowHeadInfo(hud.Info);
            // }
            
            while (waitFinishTask.TryDequeue(out var task))
            {
                task.SetResult();
            }
            waitFinishTask = null;
        }

        public void Destroy()
        {
            Messager.Instance.RemoveListener<Unit,Vector3>(Id,MessageId.ChangePositionEvt,OnChanePosition);
            Messager.Instance.RemoveListener<Unit,Quaternion>(Id,MessageId.ChangeRotationEvt,OnChaneRotation);

            if(EntityView!=null)
                GameObjectPoolManager.Instance.RecycleGameObject(EntityView.gameObject);
            while (waitFinishTask.TryDequeue(out var task))
            {
                task.SetResult();
            }
            waitFinishTask = null;
        }

        #endregion

        public void OnChanePosition(Unit unit,Vector3 old)
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
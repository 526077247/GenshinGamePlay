using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TaoTie
{
    public partial class EffectComponent : Component, IComponent<string>
    {
        public Transform EntityView;

        private ReferenceCollector collector;

        private Queue<ETTask> waitFinishTask;

        #region override
        public void Init(string path)
        {
            LoadGameObjectAsync(path).Coroutine();
        }
        private async ETTask LoadGameObjectAsync(string path)
        {
            var obj = await GameObjectPoolManager.GetInstance().GetGameObjectAsync(path);
            if (this.IsDispose)
            {
                GameObjectPoolManager.GetInstance().RecycleGameObject(obj);
                return;
            }
            EntityView = obj.transform;
            collector = obj.GetComponent<ReferenceCollector>();
            EntityView.SetParent(this.parent.Parent.GameObjectRoot);
            var ec = obj.GetComponent<EntityComponent>();
            if (ec == null) ec = obj.AddComponent<EntityComponent>();
            ec.Id = this.Id;
            ec.EntityType = parent.Type;
            if (parent is Effect effect)
            {
                EntityView.position = effect.Position;
                EntityView.rotation = effect.Rotation;
            }
            Messager.Instance.AddListener<Effect, Vector3>(Id, MessageId.ChangePositionEvt, OnChangePosition);
            Messager.Instance.AddListener<Effect, Quaternion>(Id, MessageId.ChangeRotationEvt, OnChangeRotation);
            Messager.Instance.AddListener<Effect, Vector3>(Id, MessageId.ChangeScaleEvt, OnChangeScale);
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
            Messager.Instance.RemoveListener<Effect, Vector3>(Id, MessageId.ChangePositionEvt, OnChangePosition);
            Messager.Instance.RemoveListener<Effect, Quaternion>(Id, MessageId.ChangeRotationEvt, OnChangeRotation);
            Messager.Instance.RemoveListener<Effect, Vector3>(Id, MessageId.ChangeScaleEvt, OnChangeScale);
            if (EntityView != null)
            {
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
        }

        #endregion

        #region Event
        
        private void OnChangePosition(Effect unit, Vector3 old)
        {
            if(EntityView == null) return;
            EntityView.position = unit.Position;
        }

        private void OnChangeRotation(Effect unit, Quaternion old)
        {
            if(EntityView == null) return;
            EntityView.rotation = unit.Rotation;
        }

        private void OnChangeScale(Effect unit, Vector3 old)
        {
            if(EntityView == null) return;
            EntityView.localScale = unit.LocalScale;
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
    }
}
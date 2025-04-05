using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TaoTie
{
    public partial class EffectModelComponent : Component, IComponent<string>
    {
        public Transform EntityView;

        private Queue<ETTask> waitFinishTask;
        private Transform attachPoint;
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
            Messager.Instance.AddListener<SceneEntity, Vector3>(Id, MessageId.ChangePositionEvt, OnChangePosition);
            Messager.Instance.AddListener<SceneEntity, Quaternion>(Id, MessageId.ChangeRotationEvt, OnChangeRotation);
            Messager.Instance.AddListener<SceneEntity, Vector3>(Id, MessageId.ChangeScaleEvt, OnChangeScale);
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
            attachPoint = null;
            Messager.Instance.RemoveListener<SceneEntity, Vector3>(Id, MessageId.ChangePositionEvt, OnChangePosition);
            Messager.Instance.RemoveListener<SceneEntity, Quaternion>(Id, MessageId.ChangeRotationEvt, OnChangeRotation);
            Messager.Instance.RemoveListener<SceneEntity, Vector3>(Id, MessageId.ChangeScaleEvt, OnChangeScale);
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
        
        private void OnChangePosition(SceneEntity sceneEntity, Vector3 old)
        {
            if(EntityView == null) return;
            if (attachPoint != null)
            {
                EntityView.localPosition = sceneEntity.Position;
            }
            else
            {
                EntityView.position = sceneEntity.Position;
            }
        }

        private void OnChangeRotation(SceneEntity sceneEntity, Quaternion old)
        {
            if(EntityView == null) return;
            if (attachPoint != null)
            {
                EntityView.localRotation = sceneEntity.Rotation;
            }
            else
            {
                EntityView.rotation = sceneEntity.Rotation;
            }
        }

        private void OnChangeScale(SceneEntity sceneEntity, Vector3 old)
        {
            if(EntityView == null) return;
            EntityView.localScale = sceneEntity.LocalScale;
        }

        #endregion

        public async ETTask SetAttachPoint(Transform target, bool worldPositionStays = false)
        {
            attachPoint = target;
            var sceneEntity = GetParent<SceneEntity>();
            await WaitLoadGameObjectOver();
            await TimerManager.Instance.WaitAsync(1);
            if(IsDispose) return;
            
            EntityView.SetParent(attachPoint, worldPositionStays);
            if (!worldPositionStays)
            {
                EntityView.localPosition = Vector3.zero;
                EntityView.localRotation = Quaternion.identity;
                EntityView.localScale = sceneEntity.LocalScale;
                sceneEntity.SyncViewPosition(EntityView.localPosition);
                sceneEntity.SyncViewRotation(EntityView.localRotation);
            }
            else
            {
                sceneEntity.SyncViewPosition(EntityView.localPosition);
                sceneEntity.SyncViewRotation(EntityView.localRotation);
                sceneEntity.SyncViewLocalScale(EntityView.localScale);
            }
        }
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
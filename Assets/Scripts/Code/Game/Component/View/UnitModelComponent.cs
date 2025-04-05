using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class UnitModelComponent: Component, IComponent<ConfigModel>
    {
        /// <summary>
        /// 不要直接设置Parent
        /// </summary>
        public Transform EntityView { get; private set; }

        private ConfigModel configModel;
        public LinkedListComponent<GameObjectHolder> Holders { get; private set; }
        private bool needDestroy;
        private Queue<ETTask> waitFinishTask;
        private ArrangePlugin arrangePlugin;
        private int countKey;
        private Transform attachPoint;
        public void Init(ConfigModel config)
        {
            configModel = config;
            needDestroy = false;
            countKey = 0;
            Holders = LinkedListComponent<GameObjectHolder>.Create();
            if (configModel == null || configModel is ConfigSingletonModel)
            {
                var plugin = GameObjectHolder.Create(this,this.parent.Parent.GameObjectRoot);
                Holders.AddLast(plugin);
            }
            else if (configModel is ConfigMultiModel aroundModel)
            {
                needDestroy = true;
                EntityView = new GameObject("AroundCenter").transform;
                EntityView.SetParent(this.parent.Parent.GameObjectRoot);
                var count = aroundModel.Count.Resolve(parent, null);
                for (int i = 0; i < count; i++)
                {
                    var plugin = GameObjectHolder.Create(this,EntityView);
                    Holders.AddLast(plugin);
                }

                if (aroundModel.Count is NumericValue numericValue)
                {
                    countKey = numericValue.Key;
                    Messager.Instance.AddListener<NumericChange>(GetComponent<NumericComponent>().Id, MessageId.NumericChangeEvt, OnNumericChange);
                }

                arrangePlugin = ModelSystem.Instance.CreateArrangePlugin(aroundModel.Arrange, this);
            }
            InitAsync().Coroutine();
        }

        private async ETTask InitAsync()
        {
            using (ListComponent<ETTask<bool>> tasks = ListComponent<ETTask<bool>>.Create())
            {
                for (var item = Holders.First; item!=null; item = item.Next)
                {
                    tasks.Add(item.Value.WaitLoadGameObjectOver());
                }

                await ETTaskHelper.WaitAll(tasks);
            }
            if(IsDispose) return;
            
            if (configModel == null || configModel is ConfigSingletonModel)
            {
                EntityView = Holders.First.Value.EntityView;
            }
            else if (configModel is ConfigMultiModel aroundModel)
            {
                var unit = GetParent<Unit>();
                EntityView.localPosition = unit.Position + unit.Rotation * aroundModel.Offset.Resolve(parent, null);
                for (var item = Holders.First; item!=null; item = item.Next)
                {
                    item.Value.EntityView.SetParent(EntityView);
                }
                
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
            arrangePlugin?.Dispose();
            arrangePlugin = null;
            if (countKey!=0)
            {
                Messager.Instance.RemoveListener<NumericChange>(GetComponent<NumericComponent>().Id, MessageId.NumericChangeEvt, OnNumericChange);
            }
            Messager.Instance.RemoveListener<SceneEntity, Vector3>(Id, MessageId.ChangePositionEvt, OnChangePosition);
            Messager.Instance.RemoveListener<SceneEntity, Quaternion>(Id, MessageId.ChangeRotationEvt, OnChangeRotation);
            Messager.Instance.RemoveListener<SceneEntity, Vector3>(Id, MessageId.ChangeScaleEvt, OnChangeScale);
            
            foreach (var item in Holders)
            {
                item.Dispose();
            }
            Holders.Dispose();
            
            if (waitFinishTask != null)
            {
                while (waitFinishTask.TryDequeue(out var task))
                {
                    task.SetResult();
                }
                waitFinishTask = null;
            }

            if (needDestroy)
            {
                GameObject.Destroy(EntityView.gameObject);
            }
            EntityView = null;
            configModel = null;
        }

        #region Event

        private void OnNumericChange(NumericChange evt)
        {
            if (evt.NumericType == countKey)
            {
                while (Holders.Count > evt.New)
                {
                    var first = Holders.First;
                    Holders.RemoveFirst();
                    first.Value.Dispose();
                }
                while (Holders.Count < evt.New)
                {
                    var plugin = GameObjectHolder.Create(this,EntityView);
                    Holders.AddLast(plugin);
                }
            }
        }
        
        private void OnChangePosition(SceneEntity sceneEntity, Vector3 old)
        {
            if(EntityView == null) return;
            var offset = Vector3.zero;
            if (configModel is ConfigMultiModel aroundModel)
            {
                offset = sceneEntity.Rotation * aroundModel.Offset.Resolve(parent, null);
            }
            if (attachPoint != null)
            {
                EntityView.localPosition = sceneEntity.Position + offset;
            }
            else
            {
                EntityView.position = sceneEntity.Position + offset;
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
                if (configModel is ConfigMultiModel aroundModel)
                {
                    EntityView.localPosition = aroundModel.Offset.Resolve(parent, null);
                }
                else
                {
                    EntityView.localPosition = Vector3.zero;
                }
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
        
        public Animator GetAnimator(int index = 0)
        {
            int i = 0;
            for (var item = Holders.First; item!=null; item = item.Next)
            {
                if (i == index)
                {
                    return item.Value.Animator;
                }
                i++;
            }

            return null;
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
            return !parent.IsDispose;
        }

        public T GetCollectorObj<T>(string name, int index = 0) where T : class
        {
            int i = 0;
            for (var item = Holders.First; item!=null; item = item.Next)
            {
                if (i == index)
                {
                    return item.Value.GetCollectorObj<T>(name);
                }
                i++;
            }

            return null;
        }

        /// <summary>
        /// 开启或关闭Renderer
        /// </summary>
        /// <param name="enable"></param>
        public async ETTask EnableRenderer(bool enable)
        {
            using (ListComponent<ETTask> tasks = ListComponent<ETTask>.Create())
            {
                for (var item = Holders.First; item!=null; item = item.Next)
                {
                    tasks.Add(item.Value.EnableRenderer(enable));
                }

                await ETTaskHelper.WaitAll(tasks);
            }
        }

        /// <summary>
        /// 开启或关闭hitBox
        /// </summary>
        /// <param name="hitBox"></param>
        /// <param name="enable"></param>
        public async ETTask EnableHitBox(string hitBox, bool enable)
        {
            using (ListComponent<ETTask> tasks = ListComponent<ETTask>.Create())
            {
                for (var item = Holders.First; item!=null; item = item.Next)
                {
                    tasks.Add(item.Value.EnableHitBox(hitBox, enable));
                }

                await ETTaskHelper.WaitAll(tasks);
            }
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class ModelComponent: Component, IComponent<ConfigModel>
    {
        public Transform EntityView { get; private set; }

        private ConfigModel configModel;
        public LinkedListComponent<GameObjectHolder> Holders { get; private set; }
        private bool needDestroy;
        private Queue<ETTask> waitFinishTask;
        private ArrangePlugin arrangePlugin;
        public void Init(ConfigModel config)
        {
            configModel = config;
            needDestroy = false;
            Holders = LinkedListComponent<GameObjectHolder>.Create();
            if (configModel == null || configModel is ConfigSingletonModel)
            {
                var plugin = GameObjectHolder.Create(this);
                Holders.AddLast(plugin);
            }
            else if (configModel is ConfigMultiModel aroundModel)
            {
                needDestroy = true;
                EntityView = new GameObject("AroundCenter").transform;
                var count = aroundModel.Count.Resolve(parent, null);
                for (int i = 0; i < count; i++)
                {
                    var plugin = GameObjectHolder.Create(this);
                    Holders.AddLast(plugin);
                }

                arrangePlugin = ModelSystem.Instance.CreateArrangePlugin(aroundModel.Arrange, this);
            }
            InitAsync(configModel).Coroutine();
        }

        private async ETTask InitAsync(ConfigModel configModel)
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
            var unit = GetParent<Unit>();
            if (configModel == null || configModel is ConfigSingletonModel)
            {
                EntityView = Holders.First.Value.EntityView;
            }
            
            EntityView.SetParent(this.parent.Parent.GameObjectRoot);
            
            if (parent is Actor actor)
            {
                EntityView.localScale = Vector3.one * actor.configActor.Common.Scale;
            }
            EntityView.position = unit.Position;
            EntityView.rotation = unit.Rotation;
            
            if (configModel is ConfigMultiModel aroundModel)
            {
                EntityView.localPosition = unit.Position + unit.Rotation * aroundModel.Offset.Resolve(unit, null);
                for (var item = Holders.First; item!=null; item = item.Next)
                {
                    item.Value.EntityView.SetParent(EntityView);
                }
                
            }
            
            Messager.Instance.AddListener<Unit, Vector3>(Id, MessageId.ChangePositionEvt, OnChangePosition);
            Messager.Instance.AddListener<Unit, Quaternion>(Id, MessageId.ChangeRotationEvt, OnChangeRotation);
            Messager.Instance.AddListener<Unit, Vector3>(Id, MessageId.ChangeScaleEvt, OnChangeScale);
            
            
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
            arrangePlugin?.Dispose();
            arrangePlugin = null;
            Messager.Instance.RemoveListener<Unit, Vector3>(Id, MessageId.ChangePositionEvt, OnChangePosition);
            Messager.Instance.RemoveListener<Unit, Quaternion>(Id, MessageId.ChangeRotationEvt, OnChangeRotation);
            Messager.Instance.RemoveListener<Unit, Vector3>(Id, MessageId.ChangeScaleEvt, OnChangeScale);
            
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
            
            if(needDestroy) GameObject.Destroy(EntityView);
            EntityView = null;
            configModel = null;
        }

        #region Event

        
        private void OnChangePosition(Unit unit, Vector3 old)
        {
            if(EntityView == null) return;
            EntityView.position = unit.Position;
            if (configModel is ConfigMultiModel aroundModel)
            {
                EntityView.position = unit.Position + unit.Rotation * aroundModel.Offset.Resolve(unit, null);
            }
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

        #endregion
        
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
using System;

namespace TaoTie
{
    public abstract class BaseActorActionContext: IDisposable
    {
        protected bool isDispose = true;
        public bool IsDispose => isDispose;
        public AbilityComponent Parent{ get; private set; }
        protected ListComponent<AbilityMixin> mixins { get; private set; }
        protected BaseActorActionContext parentContext;
                
        /// <summary>
        /// 附加的modifier
        /// </summary>
        private ListComponent<BaseActorActionContext> attachedModifierList;
        
        public long ApplierID { get; private set; }
        
        
        public event Action afterAdd;
        public event Action beforeRemove;
        public event Action onExecute;

        
        public virtual void AfterAdd()
        {
            afterAdd?.Invoke();
        }

        public virtual void BeforeRemove()
        {
            beforeRemove?.Invoke();
        }

        public virtual void Execute()
        {
            onExecute?.Invoke();
        }

        
        /// <summary>
        /// 添加childModifer的关联关系,相当于该ChildModifer与Context有父子关系
        /// </summary>
        /// <param name="childModifier"></param>
        public void AddAttachedModifer(BaseActorActionContext childModifier)
        {
            if (childModifier == null) return;
            
            if (attachedModifierList == null)
            {
                childModifier.parentContext = null;
                return;
            }

            childModifier.parentContext = this;
            attachedModifierList.Add(childModifier);
        }
        /// <summary>
        /// 移除childModifer的关联关系
        /// </summary>
        /// <param name="childModifier"></param>
        private void RemoveAttachedModifer(BaseActorActionContext childModifier)
        {
            attachedModifierList?.Remove(childModifier);
        }


        protected virtual void Init(long applierID, AbilityComponent component)
        {
            ApplierID = applierID;
            Parent = component;
            mixins = ListComponent<AbilityMixin>.Create();
            attachedModifierList = ListComponent<BaseActorActionContext>.Create();
            parentContext = null;
        }
        
        public virtual void Dispose()
        {
            var children = attachedModifierList;
            attachedModifierList = null;
            
            var localMixins = mixins;
            mixins = null;
            if (localMixins != null)
            {
                for (int i = localMixins.Count-1; i >= 0; i--)
                {
                    localMixins[i].Dispose();
                }
                localMixins.Dispose();
            }

            if (children != null)
            {
                for (int i = children.Count-1; i >= 0; i--)
                {
                    children[i].Dispose();
                }
                children.Dispose();
            }

            var parent = parentContext;
            parentContext = null;
            if (parent != null) parent.RemoveAttachedModifer(this);
            
            Parent = null;
            ApplierID = 0;
            afterAdd = null;
            beforeRemove = null;
            onExecute = null;
        }

    }
}
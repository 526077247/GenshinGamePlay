using System;

namespace TaoTie
{
    public abstract class BaseActorActionContext: IDisposable
    {
        protected bool isDispose = true;
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
            childModifier.parentContext = this;
            attachedModifierList.Add(childModifier);
        }
        /// <summary>
        /// 移除childModifer的关联关系
        /// </summary>
        /// <param name="childModifier"></param>
        private void RemoveAttachedModifer(BaseActorActionContext childModifier)
        {
            attachedModifierList.Remove(childModifier);
        }


        protected virtual void Init(long applierID, AbilityComponent component)
        {
            ApplierID = applierID;
            Parent = component;
            mixins = ListComponent<AbilityMixin>.Create();
            attachedModifierList = ListComponent<BaseActorActionContext>.Create();
        }
        
        public virtual void Dispose()
        {
            for (int i = mixins.Count-1; i >= 0; i--)
            {
                mixins[i].Dispose();
            }
            mixins.Dispose();
            mixins = null;
            
            for (int i = attachedModifierList.Count-1; i >= 0; i--)
            {
                attachedModifierList[i].Dispose();
            }
            attachedModifierList.Dispose();
            attachedModifierList = null;
            
            if (parentContext != null) parentContext.RemoveAttachedModifer(this);
            
            Parent = null;
            ApplierID = 0;
            afterAdd = null;
            beforeRemove = null;
            onExecute = null;
        }

    }
}
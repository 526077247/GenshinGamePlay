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

        #region 状态字典

        /// <summary>
        /// 本上下文私有的键值状态，懒创建 —— 绝大多数 modifier 一个状态都不需要，
        /// 无条件分配等于给全场每个 buff 都挂一只 <see cref="DynDictionary"/>。
        ///
        /// <b>三层数据的最后一块拼图</b>：<see cref="NumericComponent"/> 是「一个实体一份」，
        /// ability 的 Specials 是「一个施法者一份」，而 modifier 之前根本没有容器。
        /// 需要「每个目标 × 每次施加各一份」的东西（护盾、充能层数、符文、连击点）
        /// 就只能塞进某个 Mixin 的实例字段，于是「数据」和「行为」被迫绑在一起：
        /// 外部读不到、公式取不到、加一种就得再加一个 Mixin。
        ///
        /// <b>语义边界</b>：
        /// • <see cref="ActorAbility"/> 上的 dynDictionary 就是原来的 AbilitySpecials（施加时写入初值）；
        /// • <see cref="ActorModifier"/> 上的 dynDictionary 来自 <c>ConfigAbilityModifier.ModifierSpecials</c>，
        ///   随 modifier 生命周期创建与销毁 —— 「buff 到期 = 状态消失」不再需要额外代码。
        /// </summary>
        private DynDictionary dynDictionary;

        /// <summary>
        /// 懒创建。绝大多数 modifier 一个状态都不需要，无条件分配等于给全场每个 buff 都挂一只
        /// <see cref="DynDictionary"/>。
        /// </summary>
        protected DynDictionary EnsureDynDictionary()
        {
            return dynDictionary ??= DynDictionary.Create();
        }

        /// <summary>是否已经分配（只读查询，不会触发创建）。</summary>
        public bool HasDynDictionary => dynDictionary != null;

        /// <summary>读状态；没有状态字典时返回 0，不创建。</summary>
        public float GetSpecial(string key)
        {
            if (dynDictionary == null || string.IsNullOrEmpty(key)) return 0f;
            return dynDictionary.Get(key);
        }

        /// <summary>读状态，区分「没有这个 key」与「值是 0」。</summary>
        public bool TryGetSpecial(string key, out float value)
        {
            if (dynDictionary != null && !string.IsNullOrEmpty(key))
            {
                return dynDictionary.TryGet(key, out value);
            }
            value = 0f;
            return false;
        }

        public bool ContainsSpecial(string key)
        {
            return dynDictionary != null && !string.IsNullOrEmpty(key) && dynDictionary.Contain(key);
        }

        /// <summary>写状态。会触发 <c>onValueChange</c>（注意 <see cref="DynDictionary.Set"/> 值没变也会触发）。</summary>
        public void SetSpecial(string key, float value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Log.Error($"SetSpecial 的 key 为空，写入被忽略（value={value}）");
                return;
            }
            EnsureDynDictionary().Set(key, value);
        }

        /// <summary>增量写。先读后写，读不到按 0 起算。</summary>
        public void AddSpecial(string key, float delta)
        {
            if (string.IsNullOrEmpty(key) || delta == 0f) return;
            var next = GetSpecial(key) + delta;
            SetSpecial(key, next);
        }

        private Action<string, float, float> specialChangeHandler;

        /// <summary>
        /// 状态变化回调（key, newValue, oldValue）。
        ///
        /// <b>订阅者驱动的懒创建</b>：只有真的有人监听时才创建底层字典并挂上
        /// <see cref="DynDictionary.onValueChange"/>。绝大多数 modifier 既不配 ModifierSpecials
        /// 也没人监听，不该为它们各分配一只字典。
        ///
        /// <b>过滤「值没变」</b>：<see cref="DynDictionary.Set"/> 是无条件触发的，
        /// 而调用方大多是「先算再写」（tick 回盾、扣盾后归零），不过滤会让
        /// 「盾归零」这种边沿条件退化成「每次写 0 都触发」。过滤放在这里而不是
        /// 要求每个调用方自己判断 —— 漏一处就是一个静默的重复触发。
        /// </summary>
        public event Action<string, float, float> onSpecialChange
        {
            add
            {
                if (specialChangeHandler == null) EnsureDynDictionary().onValueChange += OnSpecialValueChange;
                specialChangeHandler += value;
            }
            remove
            {
                specialChangeHandler -= value;
                if (specialChangeHandler == null && dynDictionary != null)
                {
                    dynDictionary.onValueChange -= OnSpecialValueChange;
                }
            }
        }

        private void OnSpecialValueChange(string key, float value, float oldValue)
        {
            if (value == oldValue) return;
            specialChangeHandler?.Invoke(key, value, oldValue);
        }

        #endregion

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

            // 状态放在 mixin 与子 modifier 之后回收：它们的 DisposeInternal 里可能还要读
            // （比如「移除时按剩余盾量结算」）。回池不清空的话，下一个借到这个实例的单位
            // 会带着上一个实体的盾量出场。
            specialChangeHandler = null;
            var localDynDictionary = dynDictionary;
            dynDictionary = null;
            localDynDictionary?.Dispose();

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
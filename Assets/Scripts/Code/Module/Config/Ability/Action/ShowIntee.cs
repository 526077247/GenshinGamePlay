using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    /// <summary>
    /// 显示或隐藏交互面板
    /// </summary>
    [ProtoContract]
    public partial class ShowIntee: ConfigAbilityAction
    {
        [ProtoMember(10)]
        public bool IsGlobal;
        [ProtoMember(11)]
        public bool Enable;
        [ProtoMember(12)][ShowIf("@!"+nameof(IsGlobal))]
        public int LocalId;
        
        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var ic = target.GetComponent<InteeComponent>();
            if (ic != null)
            {
                if (IsGlobal)
                {
                    ic.SetEnable(Enable);
                }
                else
                {
                    ic.SetItemEnable(Enable,LocalId);
                }
            }
        }
    }
}
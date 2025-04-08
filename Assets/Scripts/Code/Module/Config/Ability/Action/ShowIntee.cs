using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    /// <summary>
    /// 显示或隐藏交互面板
    /// </summary>
    [NinoType(false)]
    public partial class ShowIntee: ConfigAbilityAction
    {
        [NinoMember(10)]
        public bool IsGlobal;
        [NinoMember(11)]
        public bool Enable;
        [NinoMember(12)][ShowIf("@!"+nameof(IsGlobal))]
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
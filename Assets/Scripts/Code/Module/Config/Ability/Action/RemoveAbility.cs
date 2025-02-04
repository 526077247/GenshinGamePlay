using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public partial class RemoveAbility: ConfigAbilityAction
    {
        [NinoMember(10)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetAbilities)+"()",AppendNextDrawer = true)]
#endif
        public string AbilityName;

        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var ac = target.GetComponent<AbilityComponent>();
            if (ac != null)
            {
                ExecuteLater(ac,AbilityName).Coroutine();
            }
        }

        /// <summary>
        /// 防止移除后foreach循环报错，所以得等当前帧结束
        /// </summary>
        /// <param name="ac"></param>
        /// <param name="name"></param>
        private async ETTask ExecuteLater(AbilityComponent ac,string name)
        {
            await WaitHelper.WaitUpdateFinish();
            ac.RemoveAbility(name);
        }
    }
}
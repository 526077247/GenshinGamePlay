using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class RemoveModifier: ConfigAbilityAction
    {
        [NinoMember(10)]
        public string ModifierName;

        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var ac = target.GetComponent<AbilityComponent>();
            if (ac != null)
            {
                ExecuteLater(ac,ability.Config.AbilityName).Coroutine();
            }
        }

        /// <summary>
        /// 防止移除后foreach循环报错，所以得等当前帧结束
        /// </summary>
        /// <param name="ac"></param>
        /// <param name="name"></param>
        private async ETTask ExecuteLater(AbilityComponent ac,string name)
        {
            await UnityLifeTimeHelper.WaitUpdateFinish();
            ac.RemoveModifier(name, ModifierName);
        }
    }
}
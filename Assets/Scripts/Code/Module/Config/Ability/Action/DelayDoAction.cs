using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public partial class DelayDoAction: ConfigAbilityAction
    {

        [NinoMember(10)][MinValue(1)][LabelText("延时调用(ms)")]
        public int Delay = 1;
        [NinoMember(11)]
        public ConfigAbilityAction[] Actions;
        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            if(Actions==null) return;
            ExecuteAsync(actionExecuter, ability, modifier, target).Coroutine();
        }

        private async ETTask ExecuteAsync(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            await GameTimerManager.Instance.WaitAsync(Delay);
            for (int i = 0; i < Actions.Length; i++)
            {
                Actions[i].DoExecute(actionExecuter, ability, modifier, target);
            }
        }
    }
}
using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class KillGadget: ConfigAbilityAction
    {
        [NinoMember(10)]
        public ConfigSelectTargetsByChildren GadgetInfo;
        
        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            using (var entities = GadgetInfo.ResolveTargets(actionExecuter, ability, modifier, target))
            {
                for (int i = 0; i < entities.Count; i++)
                {
                    entities[i].Dispose();
                }
            }
        }
    }
}
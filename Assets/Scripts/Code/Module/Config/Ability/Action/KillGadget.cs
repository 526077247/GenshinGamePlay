using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class KillGadget: ConfigAbilityAction
    {
        [NinoMember(10)]
        public ConfigSelectTargetsByChildren GadgetInfo;
        
        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var entities = GadgetInfo.ResolveTargets(applier, ability, modifier, target);
            for (int i = 0; i < entities.Length; i++)
            {
                entities[i].Dispose();
            }
        }
    }
}
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public class ShowFightText: ConfigAbilityAction
    {
        [NinoMember(10)][LabelText("出生点")]
        public ConfigBornType born;
        [NinoMember(20)][LabelText("伤害结果")]
        public ConfigAttackResult attackResult;

        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var pos = born.ResolvePos(applier, ability, modifier, target);
            Debug.Log("pos " + pos);
            //var res = target.Parent.CreateEntity<Gadget, int,uint>(GadgetID,CampID);
            //res.Position = pos;

        }
    }
}
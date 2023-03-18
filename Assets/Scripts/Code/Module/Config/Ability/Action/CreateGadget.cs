using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public class CreateGadget: ConfigAbilityAction
    {
        [NinoMember(10)][LabelText("是否存在所有者？")]
        public bool ownerIsTarget;
        [NinoMember(11)][ShowIf(nameof(ownerIsTarget))][LabelText("所有者是？")]
        public AbilityTargetting ownerIs;
        [NinoMember(12)][ShowIf(nameof(ownerIsTarget))][LabelText("和所有者相同生命周期")]
        public bool lifeByOwnerIsAlive;
        [NinoMember(13)][LabelText("和所有者共享视野")][ShowIf(nameof(ownerIsTarget))]
        public bool sightGroupWithOwner;
        [NinoMember(14)]
        public ConfigBornType born;
        [NinoMember(15)]
        public CheckGround checkGround;
        [NinoMember(16)]
        public int GadgetID;
        [NinoMember(17)][ValueDropdown("@OdinDropdownHelper.GetCampTypeId()")]
        public uint CampID;
        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var pos = born.ResolvePos(applier, ability, modifier, target);
            if (checkGround != null && checkGround.Enable)
            {
                if (!PhysicsHelper.LinecastScene(pos + Vector3.up * checkGround.RaycastUpHeight,
                        pos + Vector3.down * checkGround.RaycastUpHeight, out var newPos))
                {
                    if (checkGround.DontCreateIfInvalid) return;
                }
                else
                {
                    if (checkGround.StickToGroundIfValid)
                    {
                        pos = newPos;
                    }
                }
            }
            var rot = born.ResolveRot(applier, ability, modifier, target);
            var res = target.Parent.CreateEntity<Gadget, int,uint>(GadgetID,CampID);
            res.Position = pos;
            res.Rotation = rot;

            if (ownerIsTarget)
            {
                var count = AbilityHelper.ResolveTarget(applier, ability, modifier, target, ownerIs, out var entities);
                if (count > 0)
                {
                    var owner = entities[0];
                    //todo: sightGroupWithOwner
                    owner.GetOrAddComponent<AttachComponent>().AddChild(res,lifeByOwnerIsAlive);
                }
            }
        }
    }
}
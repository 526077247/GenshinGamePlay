using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public class CreateGadget: ConfigAbilityAction
    {
        [NinoMember(10)][LabelText("是否存在所有者？")]
        public bool OwnerIsTarget;
        [NinoMember(11)][ShowIf(nameof(OwnerIsTarget))][LabelText("所有者是？")]
        public AbilityTargetting OwnerIs;
        [NinoMember(12)][ShowIf(nameof(OwnerIsTarget))][LabelText("和所有者相同生命周期")]
        public bool LifeByOwnerIsAlive;
        [NinoMember(13)][LabelText("和所有者共享视野")][ShowIf(nameof(OwnerIsTarget))]
        public bool SightGroupWithOwner;
        [NinoMember(14)]
        public ConfigBornType Born;
        [NinoMember(15)]
        public CheckGround CheckGround;
        [NinoMember(16)]
        public int GadgetID;
        [NinoMember(17)]
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetCampTypeId)+"()")]
        public uint CampID;
        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var pos = Born.ResolvePos(applier, ability, modifier, target);
            if (CheckGround != null && CheckGround.Enable)
            {
                if (!PhysicsHelper.LinecastScene(pos + Vector3.up * CheckGround.RaycastUpHeight,
                        pos + Vector3.down * CheckGround.RaycastUpHeight, out var newPos))
                {
                    if (CheckGround.DontCreateIfInvalid) return;
                }
                else
                {
                    if (CheckGround.StickToGroundIfValid)
                    {
                        pos = newPos;
                    }
                }
            }
            var rot = Born.ResolveRot(applier, ability, modifier, target);
            var res = target.Parent.CreateEntity<Gadget, int,uint>(GadgetID,CampID);
            res.Position = pos;
            res.Rotation = rot;

            if (OwnerIsTarget)
            {
                var count = AbilityHelper.ResolveTarget(applier, ability, modifier, target, OwnerIs, out var entities);
                if (count > 0)
                {
                    var owner = entities[0];
                    //todo: sightGroupWithOwner
                    owner.GetOrAddComponent<AttachComponent>().AddChild(res,LifeByOwnerIsAlive);
                }
            }
            Born.AfterBorn(applier, ability, modifier, target,res).Coroutine();
        }
    }
}
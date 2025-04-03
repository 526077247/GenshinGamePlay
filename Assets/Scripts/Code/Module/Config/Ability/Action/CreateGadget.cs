using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public class CreateGadget: ConfigAbilityAction
    {
        [NinoMember(10)][LabelText("是否存在所有者？")]
        public bool OwnerIsTarget;
        [NinoMember(11)][ShowIf(nameof(OwnerIsTarget))][LabelText("所有者是？")]
        public AbilityTargetting OwnerIs;
        [NinoMember(12)][ShowIf(nameof(OwnerIsTarget))][LabelText("属性所有者是？")]
        public AbilityTargetting PropOwnerIs;
        [NinoMember(13)][ShowIf(nameof(OwnerIsTarget))][LabelText("和所有者相同生命周期")]
        public bool LifeByOwnerIsAlive;
        [NinoMember(14)][LabelText("和所有者共享视野")][ShowIf(nameof(OwnerIsTarget))]
        public bool SightGroupWithOwner;
        [NinoMember(15)]
        public ConfigBornType Born;
        [NinoMember(16)]
        public CheckGround CheckGround;
        [NinoMember(17)]
        public int GadgetID;
        [NinoMember(18)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetCampTypeId)+"()")]
#endif
        public uint CampID;
        [NinoMember(19)]
        public GadgetState DefaultState;

        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var pos = Born.ResolvePos(applier, ability, modifier, target);
            if (CheckGround != null && CheckGround.Enable)
            {
                if (!PhysicsHelper.LinecastScene(pos + Vector3.up * CheckGround.RaycastUpHeight,
                        pos + Vector3.down * CheckGround.RaycastDownHeight, out var newPos))
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
            var res = target.Parent.CreateEntity<Gadget>();
            res.Position = pos;
            res.Rotation = rot;

            if (OwnerIsTarget)
            {
                var count = AbilitySystem.ResolveTarget(applier, ability, modifier, target, OwnerIs, out var entities);
                if (count > 0)
                {
                    var owner = entities[0];
                    //todo: sightGroupWithOwner
                    owner.GetOrAddComponent<AttachComponent>().AddChild(res, LifeByOwnerIsAlive);
                }

                count = AbilitySystem.ResolveTarget(applier, ability, modifier, target, PropOwnerIs, out entities);
                if (count > 0)
                {
                    var owner = entities[0];
                    res.AddOtherComponent(owner.GetComponent<NumericComponent>());
                }
            }
            res.Init(GadgetID, DefaultState, CampID);
            Born.AfterBorn(applier, ability, modifier, target, res).Coroutine();
        }
    }
}
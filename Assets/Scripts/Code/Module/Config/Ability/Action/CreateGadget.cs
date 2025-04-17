using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class CreateGadget: ConfigAbilityAction
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
        [NinoMember(20)]
        public bool OverrideCampId;
        [NinoMember(18)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetCampTypeId)+"()")]
        [ShowIf(nameof(OverrideCampId))]
#endif
        public uint CampID;
        [NinoMember(19)]
        public GadgetState DefaultState;

        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var pos = Born.ResolvePos(actionExecuter, ability, modifier, target);
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

            var rot = Born.ResolveRot(actionExecuter, ability, modifier, target);
            var res = target.Parent.CreateEntity<Gadget>();
            res.Position = pos;
            res.Rotation = rot;

            if (OwnerIsTarget)
            {
                using (var entities = TargetHelper.ResolveTarget(actionExecuter, ability, modifier, target, OwnerIs))
                {
                    if (entities.Count > 0)
                    {
                        var owner = entities[0];
                        //todo: sightGroupWithOwner
                        owner.GetOrAddComponent<AttachComponent>().AddChild(res, LifeByOwnerIsAlive);
                    }
                }
                using (var entities = TargetHelper.ResolveTarget(actionExecuter, ability, modifier, target, PropOwnerIs))
                {
                    if (entities.Count > 0)
                    {
                        var owner = entities[0];
                        res.AddOtherComponent(owner.GetComponent<NumericComponent>());
                    }
                }
            }

            uint campId = CampConst.Default;
            if (OverrideCampId)
            {
                campId = CampID;
            }
            else if (target is Actor actor)
            {
                campId = actor.CampId;
            }
            else
            {
                Log.Error("campId 获取失败");
            }

            res.Init(GadgetID, DefaultState, campId);
            Born.AfterBorn(actionExecuter, ability, modifier, target, res).Coroutine();
        }
    }
}
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigSceneGroupActorGadget: ConfigSceneGroupActor
    {
        [NinoMember(10)]
        public int ConfigID;
        [NinoMember(11)]
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupRouteIds)+"()", AppendNextDrawer = true)]
        public int RouteId;
        [NinoMember(12)][LabelText("延迟启动(ms)")][Tooltip("<0不启动；0立刻；>0延迟多久")]
        public int Delay;

        [NinoMember(13)] 
        public GadgetState DefaultState;
        public override Entity CreateActor(SceneGroup sceneGroup)
        {
            var entity = sceneGroup.Parent.CreateEntity<Gadget, int, GadgetState, uint>(ConfigID,DefaultState,CampId);
            entity.AddComponent<SceneGroupActorComponent, int, long>(LocalId, sceneGroup.Id);
            entity.Position = Position;
            if (sceneGroup.TryGetRoute(RouteId, out var route))
            {
                var pmc = entity.AddComponent<PlatformMoveComponent, ConfigRoute>(route);
                if (Delay >= 0)
                {
                    pmc.DelayStart(Delay);
                }
            }
            return entity;
        }
    }
}
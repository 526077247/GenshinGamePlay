using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigSceneGroupActorGadget: ConfigSceneGroupActor
    {
        [NinoMember(10)]
        public int configID;
        [NinoMember(11)][ValueDropdown("@OdinDropdownHelper.GetSceneGroupRouteIds()")]
        public int routeId;
        [NinoMember(12)][LabelText("延迟启动(ms)")][Tooltip("<0不启动；0立刻；>0延迟多久")]
        public int delay;

        [NinoMember(13)] 
        public GadgetState defaultState;
        public override Entity CreateActor(SceneGroup sceneGroup)
        {
            var entity = sceneGroup.Parent.CreateEntity<Gadget, int, GadgetState, uint>(configID,defaultState,campId);
            entity.Position = position;
            if (sceneGroup.TryGetRoute(routeId, out var route))
            {
                var pmc = sceneGroup.AddComponent<PlatformMoveComponent, ConfigRoute>(route);
                if (delay >= 0)
                {
                    pmc.DelayStart(delay);
                }
            }
            return entity;
        }
    }
}
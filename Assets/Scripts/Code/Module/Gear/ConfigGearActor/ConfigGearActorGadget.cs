using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigGearActorGadget: ConfigGearActor
    {
        [NinoMember(10)]
        public int configID;
        [NinoMember(11)][ValueDropdown("@OdinDropdownHelper.GetGearRouteIds()")]
        public int routeId;
        [NinoMember(12)][LabelText("延迟启动(ms)")][Tooltip("<0不启动；0立刻；>0延迟多久")]
        public int delay;

        [NinoMember(13)] 
        public GadgetState defaultState;
        public override Entity CreateActor(Gear gear)
        {
            
            var entity = gear.Parent.CreateEntity<Gadget, int, GadgetState>(configID,defaultState);
            
            if (gear.TryGetRoute(routeId, out var route))
            {
                var pmc = gear.AddComponent<PlatformMoveComponent, ConfigRoute>(route);
                if (delay >= 0)
                {
                    pmc.DelayStart(delay);
                }
            }
            return entity;
        }
    }
}
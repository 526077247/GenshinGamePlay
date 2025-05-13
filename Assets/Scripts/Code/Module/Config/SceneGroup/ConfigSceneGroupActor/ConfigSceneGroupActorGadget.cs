using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigSceneGroupActorGadget: ConfigSceneGroupActor
    {
        [NinoMember(10)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetGadgetConfigIds)+"()")]
#endif
        public int ConfigID;
        [NinoMember(11)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupRouteIds)+"()", AppendNextDrawer = true)]
#endif
        public int RouteId;
        [NinoMember(12)][LabelText("*延迟启动(ms)")][Tooltip("<0不启动；0立刻；>0延迟多久")]
        public int Delay;

        [NinoMember(13)] 
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetGadgetState)+"()")]
#endif
        public GadgetState DefaultState;

        public override Entity CreateActor(SceneGroup sceneGroup, float range)
        {
            Vector3 position;
            Quaternion rotation;
            if (IsLocal)
            {
                position = Quaternion.Euler(sceneGroup.Rotation) * Position + sceneGroup.Position;
                rotation = Quaternion.Euler(sceneGroup.Rotation + Rotation);
            }
            else
            {
                position = Position;
                rotation = Quaternion.Euler(Rotation);
            }
            if (range > 0)
            {
                position += Quaternion.Euler(0, Random.Range(0, 360), 0) * Vector3.forward * Random.Range(0, range);
            }
            var entity = sceneGroup.Parent.CreateEntity<Gadget, int, GadgetState, uint>(ConfigID, DefaultState, CampId);
            entity.AddComponent<SceneGroupActorComponent, int, long>(LocalId, sceneGroup.Id);
            entity.Position = position;
            entity.Rotation = rotation;
            if (sceneGroup.TryGetRoute(RouteId, out var route))
            {
                var pmc = entity.GetComponent<MoveComponent>();
                if (pmc == null)
                {
                    Log.Error($"CreateActor {LocalId} Not Found MoveComponent");
                }
                else
                {
                    pmc.ChangeStrategy(new ConfigPlatformMove()
                    {
                        Route = route,
                        Delay = Delay
                    }, sceneGroup);
                }
                
            }

            return entity;
        }
    }
}
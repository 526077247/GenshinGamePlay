using System;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public sealed partial class ConfigSceneGroupActorMonster : ConfigSceneGroupActor
    {
        [NinoMember(10)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetMonsterConfigIds)+"()")]
#endif
        public int ConfigID;
        
        [NinoMember(11)][LabelText("是否有防御区域")]
        public bool HasDefendArea;
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupZoneIds)+"()")]
        [ShowIf(nameof(HasDefendArea))]
#endif
        [NinoMember(12)][LabelText("防御区域ZoneId")]
        public int DefendAreaZone;
        public override Entity CreateActor(SceneGroup sceneGroup)
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

            Monster entity = null;
            if (HasDefendArea)
            {
                if (sceneGroup.TryGetZoneEntity(DefendAreaZone, out var zoneId))
                {
                    var zone = sceneGroup.Parent.Get<Zone>(zoneId);
                    entity = sceneGroup.Parent.CreateEntity<Monster, int,Vector3,uint,Zone>(ConfigID, position, CampId, zone);
                }
                else
                {
                    Log.Error("防御区域未创建 monsterId = " + LocalId);
                    entity = sceneGroup.Parent.CreateEntity<Monster, int,Vector3,uint>(ConfigID, position, CampId);
                }
            }
            else
            {
                entity = sceneGroup.Parent.CreateEntity<Monster, int,Vector3,uint>(ConfigID, position, CampId);
            }

            entity.Rotation = rotation;
            entity.AddComponent<SceneGroupActorComponent, int, long>(LocalId, sceneGroup.Id);
            return entity;
        }
    }
}
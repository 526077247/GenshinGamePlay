using System;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

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
        
        [NinoMember(11)][LabelText("防御区域")]
        public ConfigShape DefendArea;

        public override Entity CreateActor(SceneGroup sceneGroup,float range)
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

            Monster entity = null;
            if (DefendArea != null)
            {
                entity = sceneGroup.Parent.CreateEntity<Monster, int, Vector3, uint, ConfigShape>(ConfigID, position,
                    CampId, DefendArea);
            }
            else
            {
                entity = sceneGroup.Parent.CreateEntity<Monster, int, Vector3, uint>(ConfigID, position, CampId);
            }

            entity.Rotation = rotation;
            entity.AddComponent<SceneGroupActorComponent, int, long>(LocalId, sceneGroup.Id);
            return entity;
        }
    }
}
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public class ConfigSceneGroupActorCharacter : ConfigSceneGroupActor
    {
        [NinoMember(10)]
#if UNITY_EDITOR
        [ValueDropdown("@" + nameof(OdinDropdownHelper) + "." + nameof(OdinDropdownHelper.GetCharacterConfigIds) +
                       "()")]
#endif
        public int ConfigID;

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

            var entity = sceneGroup.Parent.CreateEntity<Character, int, uint>(ConfigID, CampId);
            entity.AddComponent<SceneGroupActorComponent, int, long>(LocalId, sceneGroup.Id);
            entity.Position = position;
            entity.Rotation = rotation;

            return entity;
        }
    }
}
using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public class ConfigSceneGroupActorCharacter : ConfigSceneGroupActor
    {
        [ProtoMember(10)]
#if UNITY_EDITOR
        [ValueDropdown("@" + nameof(OdinDropdownHelper) + "." + nameof(OdinDropdownHelper.GetCharacterConfigIds) +
                       "()")]
#endif
        public int ConfigID;

        protected override Actor InnerCreateActor(SceneGroup sceneGroup,float range)
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
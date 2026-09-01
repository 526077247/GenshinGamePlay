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
    public class ConfigSceneGroupActorAvatar : ConfigSceneGroupActor
    {
        [ProtoMember(10)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetAvatarConfigIds)+"()")]
#endif
        public int ConfigID;
        [ProtoMember(11)]
        public bool CreateIfExists;
        [ProtoMember(12)][Tooltip("与SceneGroup脱钩，之后SceneGroup切换suite，也不会销毁该actor")]
        public bool RemoveFromSceneGroup;
        public override Entity CreateActor(SceneGroup sceneGroup,float range)
        {
            if (!CreateIfExists)
            {
                var list = sceneGroup.Parent.GetAll<Avatar>();
                for (int i = 0; i < list?.Count; i++)
                {
                    if (list[i].GetComponent<AvatarComponent>()?.ConfigId == ConfigID)
                    {
                        var res = list[i];
                        var component = res.AddComponent<SceneGroupActorComponent, int, long>(LocalId, sceneGroup.Id);
                        if (RemoveFromSceneGroup)
                        {
                            component.RemoveFromSceneGroup();
                        }
                        return res;
                    }
                }
            }
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
            var entity = sceneGroup.Parent.CreateEntity<Avatar, int>(ConfigID);
            var sgac = entity.AddComponent<SceneGroupActorComponent, int, long>(LocalId, sceneGroup.Id);
            entity.Position = position;
            entity.Rotation = rotation;
            if (RemoveFromSceneGroup)
            {
                sgac.RemoveFromSceneGroup();
            }
            return entity;
        }
    }
}
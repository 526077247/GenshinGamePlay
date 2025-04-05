using System;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace TaoTie
{
    [LabelText("立方")]
    [NinoType(false)]
    public partial class ConfigSceneGroupZoneObb : ConfigSceneGroupZone
    {
        [NinoMember(5)]
        public Vector3 Rotation;
        [NinoMember(6)]
        public Vector3 Size;

        public override Zone CreateZone(SceneGroup sceneGroup)
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
            var entity = sceneGroup.Parent.CreateEntity<Zone>();
            var obj = entity.GameObject;
            obj.transform.rotation = rotation;
            obj.transform.position = position;
            var collider = obj.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = Size;
            entity.AddComponent<SceneGroupZoneComponent, int, long,GameObject >(LocalId, sceneGroup.Id, obj);
            return entity;
        }
    }
}
using System;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("球")]
    [NinoType(false)]
    public partial class ConfigSceneGroupZoneSphere : ConfigSceneGroupZone
    {
        [NinoMember(5)]
        public float Radius;
        
        public override Zone CreateZone(SceneGroup sceneGroup)
        {
            Vector3 position;
            if (IsLocal)
            {
                position = Quaternion.Euler(sceneGroup.Rotation) * Position + sceneGroup.Position;
            }
            else
            {
                position = Position;
            }
            var entity = sceneGroup.Parent.CreateEntity<Zone>();
            var obj = entity.GameObject;
            obj.name = "ZoneSphere";
            obj.transform.position = position;
            var collider = obj.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = Radius;
            entity.Collider = collider;
            entity.AddComponent<SceneGroupZoneComponent, int, long,GameObject>(LocalId, sceneGroup.Id, obj);
            return entity;
        }
    }
}
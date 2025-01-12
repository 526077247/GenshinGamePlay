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
            entity.Position = position;
            var ghc = entity.GetComponent<GameObjectHolderComponent>();
            ghc.EntityView.gameObject.layer = LayerMask.NameToLayer("Entity");
            var collider = ghc.EntityView.gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = Radius;
            entity.AddComponent<SceneGroupZoneComponent, int, long>(LocalId, sceneGroup.Id);
            return entity;
        }
    }
}
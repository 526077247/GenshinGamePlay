using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("球")]
    [NinoSerialize]
    public partial class ConfigSceneGroupZoneSphere : ConfigSceneGroupZone
    {
        [NinoMember(5)]
        public float Radius;
        
        public override Zone CreateZone(SceneGroup sceneGroup)
        {
            var entity = sceneGroup.Parent.CreateEntity<Zone>();
            entity.Position = Position;
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
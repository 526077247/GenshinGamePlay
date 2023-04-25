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
            entity.AddComponent<SceneGroupZoneComponent, int, long>(LocalId, sceneGroup.Id);
            return entity;
        }
    }
}
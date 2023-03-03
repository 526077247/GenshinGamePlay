using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace TaoTie
{
    [LabelText("立方")]
    [NinoSerialize]
    public partial class ConfigSceneGroupZoneObb : ConfigSceneGroupZone
    {
        [NinoMember(5)]
        public Vector3 rotation;
        [NinoMember(6)]
        public Vector3 size;

        public override Zone CreateZone(SceneGroup sceneGroup)
        {
            var entity = sceneGroup.Parent.CreateEntity<Zone>();
            entity.AddComponent<SceneGroupZoneComponent, int, long>(localId, sceneGroup.Id);
            return entity;
        }
    }
}
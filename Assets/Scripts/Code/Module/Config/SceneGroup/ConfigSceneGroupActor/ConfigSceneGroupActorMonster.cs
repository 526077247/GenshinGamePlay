using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public sealed partial class ConfigSceneGroupActorMonster : ConfigSceneGroupActor
    {
        [NinoMember(10)]
        public int ConfigID;

        public override Entity CreateActor(SceneGroup sceneGroup)
        {
            var entity = sceneGroup.Parent.CreateEntity<Monster, int,Vector3,uint>(ConfigID,Position,CampId);
            entity.Rotation = Quaternion.Euler(Rotation);
            entity.AddComponent<SceneGroupActorComponent, int, long>(LocalId, sceneGroup.Id);
            return entity;
        }
    }
}
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
        public int configID;

        public override Entity CreateActor(SceneGroup sceneGroup)
        {
            var entity = sceneGroup.Parent.CreateEntity<Monster, int,Vector3>(configID,position);
            entity.CampId = campId;
            entity.AddComponent<SceneGroupActorComponent, int, long>(localId, sceneGroup.Id);
            return entity;
        }
    }
}
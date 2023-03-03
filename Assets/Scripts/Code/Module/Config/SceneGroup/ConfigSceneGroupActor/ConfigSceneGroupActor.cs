using System;
using LitJson.Extensions;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public abstract class ConfigSceneGroupActor
    {
#if UNITY_EDITOR
        [PropertyOrder(int.MinValue+1)]
        [LabelText("策划备注")]
        private string remarks;
#endif
        [NinoMember(1)]
        [PropertyOrder(int.MinValue)]
        public int localId;
        [NinoMember(2)]
        public Vector3 position;
        [NinoMember(3)][ValueDropdown("@OdinDropdownHelper.GetCampTypeId()")]
        public uint campId;
        
        public abstract Entity CreateActor(SceneGroup sceneGroup);
    }
}
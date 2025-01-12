using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("传送")]
    [NinoType(false)]
    public partial class ConfigSceneGroupTransferAction: ConfigSceneGroupAction
    {
        [NinoMember(10)] 
        public string Scene;
        [NinoMember(11)]
        public Vector3 Position;
        [NinoMember(12)]
        public Vector3 Rotation;
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            SceneManager.Instance.SwitchMapScene(Scene,Position,Rotation).Coroutine();
        }
    }
}
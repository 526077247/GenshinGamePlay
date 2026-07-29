using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("传送")]
    [ProtoContract]
    public partial class ConfigSceneGroupTransferAction: ConfigSceneGroupAction
    {
        [ProtoMember(10)] 
        public string Scene;
        [ProtoMember(11)]
        public Vector3 Position;
        [ProtoMember(12)]
        public Vector3 Rotation;
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            SceneManager.Instance.SwitchMapScene(Scene,Position,Rotation).Coroutine();
        }
    }
}
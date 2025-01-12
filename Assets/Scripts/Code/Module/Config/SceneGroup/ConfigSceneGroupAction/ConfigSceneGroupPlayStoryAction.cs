using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 播放剧情
    /// </summary>
    [LabelText("播放剧情")]
    [NinoType(false)]
    public partial class ConfigSceneGroupPlayStoryAction : ConfigSceneGroupAction
    {
        [NinoMember(10)]
        public ulong Id;
        [NinoMember(11)]
        public Vector3 Position;
        [NinoMember(12)]
        public Vector3 Rotation;
        [NinoMember(13)][LabelText("是否是相对坐标、方向")]
        public bool IsLocal;
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            Vector3 position;
            Quaternion rotation;
            if (IsLocal)
            {
                position = Quaternion.Euler(aimSceneGroup.Rotation) * Position + aimSceneGroup.Position;
                rotation = Quaternion.Euler(aimSceneGroup.Rotation + Rotation);
            }
            else
            {
                position = Position;
                rotation = Quaternion.Euler(Rotation);
            }
            aimSceneGroup.Manager.Parent.GetManager<StorySystem>()?.PlayStory(Id, aimSceneGroup, position, rotation)
                .Coroutine();
        }
    }
}
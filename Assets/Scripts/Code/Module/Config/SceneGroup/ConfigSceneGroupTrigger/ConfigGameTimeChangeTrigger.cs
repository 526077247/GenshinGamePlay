using System;
using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("当关卡的时间到达指定时间之后")]
    [ProtoContract]
    public partial class ConfigGameTimeChangeTrigger : ConfigSceneGroupTrigger<GameTimeChange>
    {
        [ProtoMember(5)][LabelText("游戏时间（ms）")]
        public long GameTime;

        protected override bool CheckCondition(SceneGroup sceneGroup, GameTimeChange evt)
        {
            return evt.GameTimeNow >= GameTime;
        }
    }
}
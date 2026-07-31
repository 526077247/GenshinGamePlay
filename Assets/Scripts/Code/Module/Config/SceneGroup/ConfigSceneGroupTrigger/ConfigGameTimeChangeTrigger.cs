using System;
using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
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
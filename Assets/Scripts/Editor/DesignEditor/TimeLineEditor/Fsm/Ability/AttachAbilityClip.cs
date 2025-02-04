using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
namespace TaoTie
{

    [Serializable]
    public class AttachAbilityClip : PlayableAsset, ITimelineClipAsset
    {
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetAbilities)+"()",AppendNextDrawer = true)]
        public string AbilityName;
        [LabelText("当还未开始时被打断是否添加")]
        public bool AddOnBreak;
        [LabelText("结束时是否移除")]
        public bool RemoveOnOver;
        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<AttachAbilityBehaviour>.Create(graph);
            return playable;
        }
        
    }
}
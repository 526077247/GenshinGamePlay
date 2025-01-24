using System;
using DaGenGraph;
using Nino.Core;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace TaoTie
{
    [LabelText("延迟调用 节点")]
    [NinoType(false)]
    public partial class ConfigSceneGroupDelayAction: ConfigSceneGroupAction
    {
        [NinoMember(10)][Min(1)]
        [LabelText("延迟时间（ms）")]
        public long Delay = 1;
        
        [NinoMember(11)]
        [LabelText("是否现实世界时间")]
        public bool IsRealTime;
        
        [NinoMember(12)]
        [LabelText("到时间后执行")]
#if UNITY_EDITOR
        [OnCollectionChanged(nameof(Refresh))]
        [OnStateUpdate(nameof(Refresh))][DrawIgnore]
        [TypeFilter("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetFilteredActionTypeList)+"("+nameof(HandleType)+")")]
#endif
        public ConfigSceneGroupAction[] Actions;
        
#if UNITY_EDITOR
        
        private void Refresh()
        {
            if (Actions!= null)
            {
                for (int i = 0; i <  Actions.Length; i++)
                {
                    if(Actions[i]!=null)
                        Actions[i].HandleType = HandleType;
                }
                Actions.Sort((a, b) => { return a.LocalId - b.LocalId;});
            }
            
        }
#endif

        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            ExecuteAsync(evt, aimSceneGroup, fromSceneGroup).Coroutine();
        }

        private async ETTask ExecuteAsync(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            if (IsRealTime)
            {
                await TimerManager.Instance.WaitAsync(Delay);
            }
            else
            {
                await GameTimerManager.Instance.WaitAsync(Delay);
            }

            if (aimSceneGroup.IsDispose)
            {
                Log.Error("延迟调用Action，到时间后目标SceneGroup已被销毁");
                return;
            }
            for (int i = 0; i < (Actions == null ? 0 : Actions.Length); i++)
            {
                Actions[i]?.ExecuteAction(evt, aimSceneGroup, fromSceneGroup);
            } 
        }
    }
}
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class KillGadget: ConfigAbilityAction
    {
        [NinoMember(10)]
        public ConfigSelectTargetsByChildren GadgetInfo;
        [NinoMember(11)][LabelText("*是否下一帧销毁")][Tooltip("注意时序，先Kill了可能会影响后面执行的Action内部的判断，所以一般开启此项")]
        public bool KillNextFrame = true;
        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            using (var entities = GadgetInfo.ResolveTargets(actionExecuter, ability, modifier, target))
            {
                for (int i = 0; i < entities.Count; i++)
                {
                    if(KillNextFrame)
                        entities[i].DelayDispose(1);
                    else
                        entities[i].Dispose();
                }
            }
        }
    }
}
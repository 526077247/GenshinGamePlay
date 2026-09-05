using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public class SampleHoldInputAction : ConfigAbilityAction
    {
        [ProtoMember(10)][LabelText("冻结")]
        public bool Hold;
        
        [ProtoMember(11)][LabelText("抓取当前输入方向")][ShowIf(nameof(Hold))]
        public bool Sample;

        [ProtoMember(12)][LabelText("方向")][ShowIf("@!"+nameof(Sample)+"&&"+nameof(Hold))]
        public Vector3 Direction;

        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var inputController = target.GetComponent<LocalInputController>();
            if (inputController == null) return;
            if (Hold)
            {
                if (Sample)
                {
                    inputController.FreezeInput();
                }
                else
                {
                    inputController.FreezeInput(Direction);
                }
            }
            else
            {
                inputController.UnfreezeInput();
            }
        }
    }
}
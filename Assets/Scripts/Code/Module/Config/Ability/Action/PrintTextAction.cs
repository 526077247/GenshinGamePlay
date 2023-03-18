using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class PrintTextAction: ConfigAbilityAction
    {
        /// <summary>
        /// 打印的文本
        /// </summary>
        [NinoMember(1)]
        public string text;
        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            Debug.Log(text);
        }
    }
}
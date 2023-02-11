// using System;
// using Sirenix.OdinInspector;
// using UnityEngine;
// namespace TaoTie
// {
//     [TriggerType]
//     [Serializable][LabelText("GadgetState")]
//     public class ConfigGadgetStateCondition: ConfigGearCondition
//     {
//
//         [Tooltip(GearTooltips.CompareMode)] [OnValueChanged("@CheckModeType(value,mode)")] [SerializeField]
//         public CompareMode mode;
//         [ValueDropdown("@OdinDropdownHelper.GetGearActorIds()")]
//         [SerializeField] public int actorId;
//         
//         [SerializeField] public int value;
//
// #if UNITY_EDITOR
//         protected override bool CheckModeType<T>(T t, CompareMode mode)
//         {
//             if (!base.CheckModeType(t, mode))
//             {
//                 mode = CompareMode.Equal;
//                 return false;
//             }
//
//             return true;
//         }
// #endif
//     }
// }
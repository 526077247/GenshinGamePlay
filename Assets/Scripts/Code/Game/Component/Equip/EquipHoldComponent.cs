using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 装备槽与武器显隐。显隐采用「状态对齐」：
    /// 恒等于所有 FSM 当前状态 ShowWeapon 的 OR 聚合（任一为 true 即显示），
    /// 由 Fsm.ChangeState 无条件广播的 MessageId.FsmStateChanged 驱动，不再每帧轮询。
    /// </summary>
    public class EquipHoldComponent: Component, IComponent
    {
        private AttachComponent attachComponent => parent.GetComponent<AttachComponent>();
        private UnitModelComponent modelComponent => parent.GetComponent<UnitModelComponent>();

        private Dictionary<EquipType, Equip> euips;

        /// <summary>
        /// 最近一次观测到的<b>聚合</b>显隐值，用于 <c>AddEquip</c> 时给新武器 FSM 的初值。
        /// 不是「最后一次广播的值」——多 FSM 各自广播时那会退化成最后写入者胜；
        /// 也不是「跳过写入的判据」，写入差量交给 FsmComponent.SetData 内部完成。
        /// </summary>
        private bool showWeaponState;

        #region IComponent

        public void Init()
        {
            euips = new Dictionary<EquipType, Equip>();
            showWeaponState = false;
            Messager.Instance.AddListener(Id, MessageId.FsmStateChanged, OnFsmStateChanged);
            SyncShowWeapon();
        }

        public void Destroy()
        {
            Messager.Instance.RemoveListener(Id, MessageId.FsmStateChanged, OnFsmStateChanged);
            euips.Clear();
            showWeaponState = false;
        }

        #endregion

        /// <summary>
        /// 状态变更驱动：每次 ChangeState 都会广播一次，与 ShowWeapon 值是否翻转无关。
        /// Base / Hit 两个 FSM 各自切换都会触发，聚合在这里统一收敛。
        /// </summary>
        private void OnFsmStateChanged()
        {
            SyncShowWeapon();
        }

        private void SyncShowWeapon()
        {
            if (IsDispose) return;

            bool show = false;
            var fsms = parent.GetComponent<FsmComponent>()?.Fsms;
            if (fsms != null)
            {
                for (int i = 0; i < fsms.Length; i++)
                {
                    var state = fsms[i]?.CurrentState;
                    if (state != null && state.ShowWeapon)
                    {
                        show = true;
                        break;
                    }
                }
            }

            showWeaponState = show;

            // 落到收剑态（冲刺 / 受击 / 死亡 / 收剑）时，把挂起的「恢复型」补执行立刻兑现。
            // 典型场景：NA4 连段到 NA5 时排了一个等到 1.41s 的 Show，中途冲刺 —— 若继续等，
            // 角色与武器会一直停在被关闭的状态。这里不判边沿，只要聚合是 false 就兑现：
            // 收剑态下补一次 Show 只能是「提前恢复」，不会把该藏的藏起来。
            if (!show)
            {
                parent.GetComponent<AbilityComponent>()?.FlushPendingRestores();
            }

            // 不在这里用 showWeaponState 做「跳过写入」：SetData 内部已有二次差量，
            // 外层再省一次遍历，反而会让「武器 FSM 参数被外部重置」无法自愈。
            ApplyShowWeapon(show);
        }

        private void ApplyShowWeapon(bool show)
        {
            SetEquipShowWeapon(EquipType.Equip01, show);
            SetEquipShowWeapon(EquipType.Equip02, show);
            SetEquipShowWeapon(EquipType.Equip03, show);
            SetEquipShowWeapon(EquipType.Equip04, show);
        }

        private void SetEquipShowWeapon(EquipType type, bool show)
        {
            if (!euips.TryGetValue(type, out var equip) || equip == null || equip.IsDispose) return;
            equip.GetComponent<FsmComponent>()?.SetData(FSMConst.ShowWeapon, show);
        }

        public async ETTask AddEquip(int equipId)
        {
            var equip = parent.Parent.CreateEntity<Equip, int>(equipId);
            var apt = equip.GetComponent<EquipComponent>().Config.EquipType;
            if (EquipType.TryParse(apt, out EquipType equipType))
            {
                var points = (parent as Actor)?.ConfigActor?.EquipController?.AttachPoints;
                if(points != null && points.TryGetValue(equipType,out var pointName))
                {
                    if (euips.TryGetValue(equipType, out var old))
                    {
                        old.Dispose();
                    }
                    euips[equipType] = equip;
                    attachComponent.AddChild(equip);
                    // 新装备的 FSM 从 defaultValue(false) 起步；即使聚合值这次没翻转，
                    // 也要把它对齐到当前聚合值，否则新挂上的武器会一直停在 Hide。
                    SyncShowWeapon();
                    equip.GetComponent<FsmComponent>()?.SetData(FSMConst.ShowWeapon, showWeaponState);
                    await modelComponent.WaitLoadGameObjectOver();
                    if (!modelComponent.IsDispose)
                    {
                        var model = equip.GetComponent<UnitModelComponent>();
                        await model.WaitLoadGameObjectOver();
                        if (!model.IsDispose)
                        {
                            var point = modelComponent.GetCollectorObj<Transform>(pointName);
                            await model.SetAttachPoint(point);
                            return;
                        }
                    }
                }
                Log.Error((parent as Actor).Config.ActorConfig + " 未找到挂点: "+equipType);
            }
            else
            {
                Log.Error("Equip 表 AttachPointType 名称错误 id="+equipId);
            }
            equip.Dispose();
        }
    }
}

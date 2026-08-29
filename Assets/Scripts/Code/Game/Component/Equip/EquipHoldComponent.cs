using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class EquipHoldComponent: Component, IComponent
    {
        private AttachComponent attachComponent => parent.GetComponent<AttachComponent>();
        private UnitModelComponent modelComponent => parent.GetComponent<UnitModelComponent>();

        private Dictionary<EquipType, Equip> euips;
        private bool showWeaponState;
        #region IComponent

        public void Init()
        {
            euips = new Dictionary<EquipType, Equip>();
            FsmComponent fsm = parent.GetComponent<FsmComponent>();
            if (fsm != null)
            {
                showWeaponState = fsm.DefaultFsm?.CurrentState?.ShowWeapon??false;
            }
            else
            {
                showWeaponState = false;
            }
            Messager.Instance.AddListener<bool>(Id,MessageId.SetShowWeapon,SetShowWeapon);
        }

        public void Destroy()
        {
            Messager.Instance.RemoveListener<bool>(Id,MessageId.SetShowWeapon,SetShowWeapon);
            euips.Clear();
        }
        
        #endregion
        
        private void SetShowWeapon(bool show)
        {
            if(IsDispose) return;
            showWeaponState = show;
            if (euips.TryGetValue(EquipType.Equip01,out var equip))
            {
                equip?.GetComponent<FsmComponent>()?.SetData(FSMConst.ShowWeapon,show);
            }
            if (euips.TryGetValue(EquipType.Equip02,out equip))
            {
                equip?.GetComponent<FsmComponent>()?.SetData(FSMConst.ShowWeapon,show);
            }
            if (euips.TryGetValue(EquipType.Equip03,out equip))
            {
                equip?.GetComponent<FsmComponent>()?.SetData(FSMConst.ShowWeapon,show);
            }
            if (euips.TryGetValue(EquipType.Equip04,out equip))
            {
                equip?.GetComponent<FsmComponent>()?.SetData(FSMConst.ShowWeapon,show);
            }
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
                    equip.GetComponent<FsmComponent>()?.SetData(FSMConst.ShowWeapon,showWeaponState);
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
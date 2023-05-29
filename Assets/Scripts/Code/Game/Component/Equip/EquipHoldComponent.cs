using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class EquipHoldComponent: Component, IComponent
    {
        private AttachComponent attachComponent => parent.GetComponent<AttachComponent>();
        private GameObjectHolderComponent gameObjectHolderComponent => parent.GetComponent<GameObjectHolderComponent>();

        private Dictionary<EquipType, Equip> euips;
        private bool showWeaponState;
        #region IComponent

        public void Init()
        {
            euips = new Dictionary<EquipType, Equip>();
            showWeaponState = parent.GetComponent<FsmComponent>().DefaultFsm.currentState.ShowWeapon;
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
                if((parent as Actor).configActor.EquipController.AttachPoints.TryGetValue(equipType,out var pointName))
                {
                    if (euips.TryGetValue(equipType, out var old))
                    {
                        old.Dispose();
                    }
                    euips[equipType] = equip;
                    attachComponent.AddChild(equip);
                    equip.GetComponent<FsmComponent>()?.SetData(FSMConst.ShowWeapon,showWeaponState);
                    await gameObjectHolderComponent.WaitLoadGameObjectOver();
                    if (!gameObjectHolderComponent.IsDispose)
                    {
                        var goh = equip.GetComponent<GameObjectHolderComponent>();
                        await goh.WaitLoadGameObjectOver();
                        if (!goh.IsDispose)
                        {
                            var point = gameObjectHolderComponent.GetCollectorObj<Transform>(pointName);
                            goh.EntityView.SetParent(point, false);
                            goh.EntityView.localScale = Vector3.one;
                            goh.EntityView.localPosition = Vector3.zero;
                            goh.EntityView.localRotation = Quaternion.identity;
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
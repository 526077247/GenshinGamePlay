using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class EquipHoldComponent: Component, IComponent
    {
        private Equip[] equips;
        private AttachComponent attachComponent => parent.GetComponent<AttachComponent>();
        private GameObjectHolderComponent gameObjectHolderComponent => parent.GetComponent<GameObjectHolderComponent>();
        #region IComponent

        public void Init()
        {
            equips = new Equip[(int)AttachPointType.Max];
        }

        public void Destroy()
        {
            foreach (var item in equips)
            {
                item?.Dispose();
            }
            equips = null;
        }

        #endregion

        public async ETTask AddEquip(int equipId)
        {
            var equip = parent.Parent.CreateEntity<Equip, int>(equipId);
            var apt = equip.GetComponent<EquipComponent>().Config.AttachPointType;
            if (AttachPointType.TryParse(apt, out AttachPointType attachPointType))
            {
                if((parent as Unit).ConfigEntity.EquipController.AttachPoints.TryGetValue(attachPointType,out var pointName))
                {
                    attachComponent.AddChild(equip);
                    await gameObjectHolderComponent.WaitLoadGameObjectOver();
                    var goh = equip.GetComponent<GameObjectHolderComponent>();
                    await goh.WaitLoadGameObjectOver();
                    var point = gameObjectHolderComponent.GetCollectorObj<Transform>(pointName);
                    goh.EntityView.SetParent(point,false);
                    goh.EntityView.localScale = Vector3.one;
                    goh.EntityView.localPosition = Vector3.zero;
                    goh.EntityView.localRotation = Quaternion.identity;
                    return;
                }
                Log.Error((parent as Unit).Config.EntityConfig + " 未找到挂点: "+attachPointType);
            }
            else
            {
                
                Log.Error("Equip 表 AttachPointType 名称错误 id="+equipId);
            }
            equip.Dispose();
           
            
        }
    }
}
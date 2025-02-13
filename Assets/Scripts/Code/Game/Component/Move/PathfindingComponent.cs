using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 同一块地图可能有多种寻路数据，玩家可以随时切换，怪物也可能跟玩家的寻路不一样，寻路组件应该挂在Entity上
    /// </summary>
    public class PathfindingComponent: Component, IComponent<string>
    {
        public string Name;
        
        #region IComponent

        public void Init(string name)
        {
            this.Name = name;
        }

        public void Destroy()
        {
            this.Name = null;
        }

        #endregion
        
        public async ETTask<bool> Find(Vector3 start, Vector3 target, List<Vector3> result)
        {
            return await NavmeshSystem.Instance.Find(Name, start, target, result);
        }
    }
}
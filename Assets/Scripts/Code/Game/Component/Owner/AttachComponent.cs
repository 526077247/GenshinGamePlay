using System.Collections.Generic;

namespace TaoTie
{
    /// <summary>
    /// 维系Entity间微弱的父子关系
    /// </summary>
    public class AttachComponent:Component,IComponent
    {
        protected AttachComponent ParentEntity { get; private set; }

        protected ListComponent<long> Childs { get; private set; }
        
        public bool lifeByOwnerIsAlive { get;private set; }
        #region IComponent

        public void Init()
        {
            Childs = ListComponent<long>.Create();
        }

        public void Destroy()
        {
            ParentEntity.Childs.Remove(Id);
            for (int i = 0; i < Childs.Count; i++)
            {
                var e = Parent.Parent.Get<Entity>(Childs[i]);
                var ac =  e.GetComponent<AttachComponent>();
                if (ac.lifeByOwnerIsAlive)
                {
                    e.Dispose();
                }
                else
                {
                    ac.ParentEntity = null;
                }
            }
            Childs.Dispose();
            Childs = null;
        }

        #endregion

        public void AddChild(Entity entity,bool lifeByOwnerIsAlive)
        {
            var ac = entity.GetOrAddComponent<AttachComponent>();
            if (ac.ParentEntity != null)
            {
                ac.ParentEntity.Childs.Remove(entity.Id);
            }
            Childs.Add(entity.Id);
            ac.lifeByOwnerIsAlive = lifeByOwnerIsAlive;
            ac.ParentEntity = this;
        }
    }
}
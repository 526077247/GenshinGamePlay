using System.Collections.Generic;

namespace TaoTie
{
    /// <summary>
    /// 维系Entity间微弱的父子关系
    /// </summary>
    public class AttachComponent:Component,IComponent
    {
        public AttachComponent ParentEntity { get; private set; }

        public ListComponent<long> Childs { get; private set; }
        
        public bool LifeByOwnerIsAlive { get;private set; }
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
                var e = parent.Parent.Get<Entity>(Childs[i]);
                var ac =  e.GetComponent<AttachComponent>();
                if (ac.LifeByOwnerIsAlive)
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
            ac.LifeByOwnerIsAlive = lifeByOwnerIsAlive;
            ac.ParentEntity = this;
        }
    }
}
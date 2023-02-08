using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 范围
    /// </summary>
    public abstract class ConfigRange
    {
        public ConfigBornType bornType;
        
        public abstract int ResolveEntity(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target, EntityType[] filter, List<Entity> results);


        public virtual Vector3 GetCenter(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            return bornType.ResolvePos(actor, ability, modifier, target);
        }
        
        public virtual Quaternion GetDir(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            return bornType.ResolveRot(actor, ability, modifier, target);
        }
    }
}
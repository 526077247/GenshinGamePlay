using System;
using Nino.Serialization;
namespace TaoTie
{
	/// <summary>
	/// 筛选子Entity
	/// </summary>
	[NinoSerialize]
    public class ConfigSelectTargetsByChildren: ConfigSelectTargets
    {
	    [NinoMember(1)]
	    public int UnitConfigId;

	    public override Entity[] ResolveTargets(Entity actor, ActorAbility ability, ActorModifier modifier,
		    Entity target)
	    {
		    var ac = target.GetComponent<AttachComponent>();
		    if (ac != null)
		    {
			    using (ListComponent<Entity> list = ListComponent<Entity>.Create())
			    {
				    for (int i = 0; i < ac.Childs.Count; i++)
				    {
					    var child = target.Parent.Get<Unit>(ac.Childs[i]);
					    if (child != null && child.ConfigId == UnitConfigId)
					    {
						    list.Add(child);
					    }
				    }

				    return list.ToArray();
			    }
		    }
		    return Array.Empty<Entity>();
	    }
    }
}
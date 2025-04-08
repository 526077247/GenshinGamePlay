using System;
using Nino.Core;
namespace TaoTie
{
	/// <summary>
	/// 筛选子Entity
	/// </summary>
	[NinoType(false)]
    public class ConfigSelectTargetsByChildren: ConfigSelectTargets
    {
	    [NinoMember(1)]
	    public int UnitConfigId;

	    public override ListComponent<Entity> ResolveTargets(Entity actor, ActorAbility ability, ActorModifier modifier,
		    Entity target)
	    {
		    ListComponent<Entity> list = ListComponent<Entity>.Create();
		    var ac = target.GetComponent<AttachComponent>();
		    if (ac != null)
		    {
			    for (int i = 0; i < ac.Childs.Count; i++)
			    {
				    var child = target.Parent.Get<Unit>(ac.Childs[i]);
				    if (child != null && child.ConfigId == UnitConfigId)
				    {
					    list.Add(child);
				    }
			    }
		    }
		    return list;
	    }
    }
}
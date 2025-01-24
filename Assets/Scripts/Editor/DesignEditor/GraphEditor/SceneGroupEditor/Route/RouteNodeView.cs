using System;
using System.Reflection;
using DaGenGraph.Editor;

namespace TaoTie
{
    public class RouteNodeView: NodeView<RouteNode>
    {

        protected override bool NeedShowInspector(MemberInfo member, object obj, bool isDetails)
        {
            if (member is FieldInfo fieldInfo && fieldInfo.Name == "Points")
            {
                return node.ShowEditorPoints;
            }
            return base.NeedShowInspector(member, obj, isDetails);
        }
    }
}
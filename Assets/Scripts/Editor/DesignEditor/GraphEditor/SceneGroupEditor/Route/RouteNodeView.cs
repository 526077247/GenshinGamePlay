using System;
using System.Reflection;
using TaoTie.Inspector.Editor;

namespace TaoTie
{
    public class RouteNodeView: NodeView<RouteNode>
    {
        public class RouteNodeViewDrawBase : DrawBase
        {
            private RouteNode node;
            public RouteNodeViewDrawBase(RouteNode node)
            {
                this.node = node;
            }
            protected override bool NeedShowInspector(MemberInfo member, object obj, bool isDetails)
            {
                if (member is FieldInfo fieldInfo && fieldInfo.Name == "Points")
                {
                    return node.ShowEditorPoints;
                }
                return base.NeedShowInspector(member, obj, isDetails);
            }
        }
        
        protected override DrawBase CreateDrawBase()
        {
            return new RouteNodeViewDrawBase(node);
        }
    }
}
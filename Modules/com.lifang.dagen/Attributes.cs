using System;

namespace DaGenGraph
{
    public enum Ignore
    {
        All,
        Details,
        NodeView,
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class DrawIgnoreAttribute: Attribute
    {
        public Ignore Ignore;
        public DrawIgnoreAttribute()
        {
            Ignore = Ignore.All;
        }
        public DrawIgnoreAttribute(Ignore type)
        {
            Ignore = type;
        }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeViewTypeAttribute: Attribute
    {
        public NodeViewTypeAttribute(Type baseViewNode)
        {
            ViewType = baseViewNode;
        }
        public Type ViewType;
    }
    [AttributeUsage(AttributeTargets.Field|AttributeTargets.Property)]
    public class NotAssetsAttribute: Attribute
    {

    }
    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PortGroupAttribute: Attribute
    {
        public int Group;
        public PortGroupAttribute(int group)
        {
            Group = group;
        }
    }
}
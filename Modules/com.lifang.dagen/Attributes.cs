using System;
#if !ODIN_INSPECTOR
using UnityEditor;
#endif
namespace DaGenGraph
{
    public enum Ignore
    {
        All,
        Details,
        NodeView,
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DrawIgnoreAttribute : Attribute
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
    public class NodeViewTypeAttribute : Attribute
    {
        public NodeViewTypeAttribute(Type baseViewNode)
        {
            ViewType = baseViewNode;
        }

        public Type ViewType;
    }

    /// <summary>
    /// ScriptableObject专用，指定该项不是资源
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class NotAssetsAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PortGroupAttribute : Attribute
    {
        public int Group;

        public PortGroupAttribute(int group)
        {
            Group = group;
        }
    }


#if !ODIN_INSPECTOR
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class OnStateUpdateAttribute : Attribute
    {
        public string Action;

        public OnStateUpdateAttribute(string action) => this.Action = action;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class OnCollectionChangedAttribute : Attribute
    {
        public string After;
        public OnCollectionChangedAttribute(string after) => this.After = after;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class TypeFilterAttribute : Attribute
    {
        public string FilterGetter;
        public TypeFilterAttribute(string filterGetter) => this.FilterGetter = filterGetter;
    }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class HideReferenceObjectPickerAttribute : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MinValueAttribute : Attribute
    {
        public double MinValue;
        public MinValueAttribute(double minValue) => this.MinValue = minValue;
    }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MaxValueAttribute : Attribute
    {
        public double MaxValue;
        public MaxValueAttribute(double maxValue) => this.MaxValue = maxValue;
    }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DisableInEditorModeAttribute : Attribute
    {
        
    }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class OnValueChangedAttribute : Attribute
    {
        public string Action;

        public OnValueChangedAttribute(string action)
        {
            Action = action;
        }
    }

    public class PropertyOrderAttribute : Attribute
    {
        public float Order;

        public PropertyOrderAttribute(float order)
        {
            Order = order;
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public class BoxGroupAttribute : Attribute
    {
        public string GroupID;

        public BoxGroupAttribute(string groupID)
        {
            GroupID = groupID;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonAttribute : Attribute
    {
        public string Name;

        public ButtonAttribute(string name)
        {
            Name = name;
        }

    }

    public class ReadOnlyAttribute : Attribute
    {
    }

    public class InfoBoxAttribute : Attribute
    {

        public string Message;

        public MessageType InfoMessageType;

        public InfoBoxAttribute(
            string message,
            MessageType infoMessageType = MessageType.Info)
        {
            this.Message = message;
            this.InfoMessageType = infoMessageType;
        }

        public InfoBoxAttribute(string message)
        {
            this.Message = message;
            this.InfoMessageType = MessageType.Info;
        }
    }

    public class LabelTextAttribute : Attribute
    {
        public LabelTextAttribute(string text)
        {
            Text = text;
        }

        public string Text;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ShowIfAttribute : Attribute
    {
        public ShowIfAttribute(string condition)
        {
            Condition = condition;
        }

        public string Condition;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ValueDropdownAttribute : Attribute
    {
        public string ValuesGetter;

        public ValueDropdownAttribute(string valuesGetter)
        {
            ValuesGetter = valuesGetter;
        }
    }

#endif
}
using System;

namespace TaoTie
{
    /// <summary>
    /// 标识会被那个Trigger用到
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TriggerTypeAttribute : System.Attribute
    {
        public Type type;

        public TriggerTypeAttribute(Type type = null)
        {
            this.type = type;
        }
    }
}
using System;
namespace TaoTie
{
    /// <summary>
    /// 标识是哪个Event
    /// </summary>
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = true)]
    public class GearTriggerOfAttribute : System.Attribute
    {
        public Type type;

        public GearTriggerOfAttribute(Type type)
        {
            this.type = type;
        }
    }
}
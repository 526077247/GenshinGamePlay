using System;
namespace TaoTie
{

    public class TimerAttribute: BaseAttribute
    {
        public int Type { get; }

        public TimerAttribute(int type)
        {
            this.Type = type;
        }
    }
    
}
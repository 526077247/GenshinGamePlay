using System;

namespace TaoTie
{
    [Timer(TimerType.ComponentUpdate)]
    public class UpdateTimer: ATimer<IUpdate>
    {
        public override void Run(IUpdate t)
        {
            try
            {
                t.Update();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}
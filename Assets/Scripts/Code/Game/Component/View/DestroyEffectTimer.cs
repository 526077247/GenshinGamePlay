using System;

namespace TaoTie
{
    [Timer(TimerType.DestroyEffect)]
    public class DestroyEffectTimer : ATimer<Effect>
    {
        public override void Run(Effect self)
        {
            try
            {
                self.Dispose();
            }
            catch (Exception e)
            {
                Log.Error($"move timer error: {self.EffectName}\n{e}");
            }
        }
    }
}
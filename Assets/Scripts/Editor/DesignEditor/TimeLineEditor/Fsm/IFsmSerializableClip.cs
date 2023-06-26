using System.Collections.Generic;

namespace TaoTie
{
    public interface IFsmSerializableClip
    {
        public void DoSerialize(List<ConfigFsmClip> clips);
    }
}
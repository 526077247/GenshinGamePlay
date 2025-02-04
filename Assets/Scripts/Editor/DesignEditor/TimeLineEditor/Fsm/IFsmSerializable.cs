using System.Collections.Generic;

namespace TaoTie
{
    public interface IFsmSerializable
    {
        public void DoSerialize(List<ConfigFsmClip> clips);
    }
}
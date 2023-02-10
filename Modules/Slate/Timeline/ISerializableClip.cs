using System.Collections.Generic;

namespace TaoTie
{
    public interface ISerializableClip
    {
        public void DoSerialize(List<ConfigFsmClip> clips);
    }
}
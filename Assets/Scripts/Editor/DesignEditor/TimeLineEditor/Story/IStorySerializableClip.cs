using System.Collections.Generic;

namespace TaoTie
{
    public interface IStorySerializableClip
    {
        public void DoSerialize(List<ConfigStoryTimeLineClip> clips);
    }
}
using System.Collections.Generic;

namespace TaoTie
{
    public interface IStorySerializable
    {
        public void DoSerialize(List<ConfigStoryTimeLineClip> clips);
    }
}
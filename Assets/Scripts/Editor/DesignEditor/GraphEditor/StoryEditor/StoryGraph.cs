using System;
using TaoTie.Inspector;

namespace TaoTie
{
    public class StoryGraph: JsonGraphBase
    {
        public ulong Id;
        [LabelText("备注")]
        public string Remarks;

        public ConfigStoryActor[] Actors;
    }
}
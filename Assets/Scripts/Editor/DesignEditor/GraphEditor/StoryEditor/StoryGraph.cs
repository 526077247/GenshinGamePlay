using System;
using DaGenGraph;
using Sirenix.OdinInspector;

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
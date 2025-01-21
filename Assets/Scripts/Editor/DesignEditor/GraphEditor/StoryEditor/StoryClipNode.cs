using DaGenGraph;
using UnityEngine;

namespace TaoTie
{
    [NodeViewType(typeof(StoryClipNodeView))]
    public class StoryClipNode: JsonNodeBase
    {
        public ConfigStoryClip Data;
        
        public override void AddDefaultPorts()
        {
            AddInputPort("Pre",EdgeMode.Override, false, false);
            AddOutputPort("Next",EdgeMode.Override, false, false);
        }
    }
}
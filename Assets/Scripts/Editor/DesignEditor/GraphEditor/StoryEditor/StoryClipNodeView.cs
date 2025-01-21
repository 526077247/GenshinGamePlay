using System.Reflection;

namespace TaoTie
{
    public class StoryClipNodeView: OdinNodeView<StoryClipNode>
    {
        protected override float DrawFieldInspector(FieldInfo field, object obj, bool isDetails = false)
        {
            if (field.Name == nameof(node.Data))
            {
                
            }
            return base.DrawFieldInspector(field, obj, isDetails);
        }
    }
}
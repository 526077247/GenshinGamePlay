using System;
using System.Collections.Generic;
using System.Reflection;

namespace TaoTie
{
    public class StoryClipNodeView: OdinNodeView<StoryClipNode>
    {
        private List<Type> subTypes;
        private string[] subNames;
        protected override List<Type> GetSubClassList(Type type, out string[] names)
        {
            if (type == TypeInfo<ConfigStoryClip>.Type)
            {
                if (subTypes == null)
                {
                    subTypes = new List<Type>();
                    var types = type.Assembly.GetTypes();
                    foreach (var item in types)
                    {
                        if(item == TypeInfo<ConfigStoryBranchClip>.Type||
                           item == TypeInfo<ConfigStoryParallelClip>.Type||
                           item == TypeInfo<ConfigStorySerialClip>.Type) continue;
                        if (item.IsClass && !item.IsAbstract && type.IsAssignableFrom(item))
                        {
                            subTypes.Add(item);   
                        }
                    }
                    subNames = new string[subTypes.Count];
                    for (int i = 0; i < subNames.Length; i++)
                    {
                        subNames[i] = subTypes[i].FullName;
                    }
                }
                names = subNames;
                return subTypes;
            }
            return base.GetSubClassList(type, out names);
        }
    }
}
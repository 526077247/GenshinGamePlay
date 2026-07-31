using System;
using System.Collections.Generic;
using System.Reflection;
using TaoTie.Inspector;
using TaoTie.Inspector.Editor;

namespace TaoTie
{
    public class StoryClipNodeView: NodeView<StoryClipNode>
    {
        public class StoryClipNodeViewDrawBase: DrawBase
        {
            private List<Type> subTypes;
            private string[] subNames;
            protected override List<Type> GetSubClassList(FieldInfo field,object obj, Type type, out string[] names)
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
                            if (subTypes[i].GetCustomAttribute(typeof(LabelTextAttribute)) is LabelTextAttribute labelTextAttribute)
                            {
                                subNames[i] = labelTextAttribute.Text;
                            }
                            else
                            {
                                subNames[i] = subTypes[i].FullName;
                            }
                        }
                    }
                    names = subNames;
                    return subTypes;
                }
                return base.GetSubClassList(field,obj, type, out names);
            }
        }

        protected override DrawBase CreateDrawBase()
        {
            return new StoryClipNodeViewDrawBase();
        }
    }
}
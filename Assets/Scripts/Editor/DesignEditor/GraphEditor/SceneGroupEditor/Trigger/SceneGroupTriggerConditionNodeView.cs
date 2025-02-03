using System;
using System.Collections.Generic;
using System.Reflection;
using DaGenGraph;
using DaGenGraph.Editor;
using Sirenix.OdinInspector;

namespace TaoTie
{
    public class SceneGroupTriggerConditionNodeView: SceneGroupNodeView
    {
        private List<Type> types = new List<Type>();
        protected override List<Type> GetSubClassList(FieldInfo fieldInfo, object obj, Type type, out string[] names)
        {
            if (fieldInfo.Name == "Condition")
            {
                var handleType = (graph as SceneGroupGraph)?.FindTriggerType(node.id);
                if (handleType != null)
                {
                    types.Clear();
                    var list = OdinDropdownHelper.GetFilteredConditionTypeList(handleType);
                    foreach (Type item in list)
                    {
                        types.Add(item);
                    }

                    names = new string[types.Count];
                    for (int i = 0; i < names.Length; i++)
                    {
                        if (types[i].GetCustomAttribute(typeof(LabelTextAttribute)) is LabelTextAttribute labelTextAttribute)
                        {
                            names[i] = labelTextAttribute.Text;
                        }
                        else
                        {
                            names[i] = types[i].FullName;
                        }
                    }

                    return types;
                }
            }
            return base.GetSubClassList(fieldInfo, obj, type, out names);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Reflection;
using TaoTie.Inspector;
using TaoTie.Inspector.Editor;

namespace TaoTie
{
    public class SceneGroupTriggerConditionNodeView: SceneGroupNodeView<SceneGroupTriggerConditionNode>
    {
        public class SceneGroupTriggerConditionNodeViewDrawBase: SceneGroupGraphDrawBase
        {
            private SceneGroupTriggerConditionNode node;
            private List<Type> types = new List<Type>();

            public SceneGroupTriggerConditionNodeViewDrawBase(SceneGroupGraphWindow graphWindow, SceneGroupTriggerConditionNode node): base(graphWindow)
            {
                this.node = node;
            }
            protected override List<Type> GetSubClassList(FieldInfo fieldInfo, object obj, Type type, out string[] names)
            {
                if (fieldInfo.Name == "Condition")
                {
                    var handleType = (graphWindow.m_Graph as SceneGroupGraph)?.FindTriggerType(node.id);
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

        protected override DrawBase CreateDrawBase()
        {
            return new SceneGroupTriggerConditionNodeViewDrawBase(graphWindow as SceneGroupGraphWindow, node);
        }
    }
}
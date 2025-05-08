using System;
using System.Collections.Generic;
using System.Reflection;
using DaGenGraph;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public class SceneGroupGraph: JsonGraphBase
    {
        public ulong Id;
        [LabelText("策划备注")]
        public string Remarks;
        public Vector3 Position;
        public Vector3 Rotation;
        [LabelText("*Actors,实体")][Tooltip("单个模板可支持使用\"通过ActorId创建实体\"Action创建实体的多个实例")]
        public ConfigSceneGroupActor[] Actors;
        [LabelText("Zones,触发区域")]
        public ConfigSceneGroupZone[] Zones;
        [LabelText("是否初始随机一个阶段？")]
        public bool RandSuite;

        [LabelText("初始阶段")]
        [ShowIf("@!"+nameof(RandSuite))]
        public int InitSuite;
        
        public Type FindTriggerType(string nodeId)
        {
            NodeBase node = FindNode(nodeId);
            while (node != null)
            {
                if (node is SceneGroupTriggerNode triggerNode)
                {
                    return triggerNode.Trigger?.GetType();
                }
                if (node is SceneGroupTriggerConditionNode conditionNode)
                {
                    var type = conditionNode.Condition?.GetType()?.GetCustomAttribute(typeof(TriggerTypeAttribute)) as TriggerTypeAttribute;
                    if(type !=null)
                    {
                        return type.Type;
                    }
                }
                if (node.inputPorts != null && node.inputPorts.Count > 0 &&  node.inputPorts[0].edges.Count>0)
                {
                    var edgeId = node.inputPorts[0].edges[0];
                    var edge = GetEdge(edgeId);
                    if (edge != null)
                    {
                        var pre = FindNode(edge.outputNodeId);
                        node = pre;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            return null;
        }
    }
}
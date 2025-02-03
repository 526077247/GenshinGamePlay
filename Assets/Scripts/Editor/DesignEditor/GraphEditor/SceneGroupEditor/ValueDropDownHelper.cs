using System.Collections.Generic;
using System.Reflection;
using DaGenGraph;
using Sirenix.OdinInspector;

namespace TaoTie
{
    public static class ValueDropDownHelper
    {
        private static List<IValueDropdownItem> temp = new();
        public static bool RefreshValueDropDown(SceneGroupGraph m_Graph, FieldInfo field, object obj,
            string valuesGetter,Dictionary<FieldInfo, IValueDropdownItem[]> valueDropdown)
        {
            if (valuesGetter == "@" + nameof(OdinDropdownHelper) + "." +
                nameof(OdinDropdownHelper.GetSceneGroupActorIds) + "()")
            {
                temp.Clear();
                var graph = m_Graph;
                for (int i = 0; i < graph.Actors.Length; i++)
                {
                    temp.Add(new ValueDropdownItem()
                    {
                        Value = graph.Actors[i].LocalId,
                        Text = string.IsNullOrEmpty(graph.Actors[i].Remarks)
                            ? graph.Actors[i].LocalId.ToString()
                            : $"{graph.Actors[i].Remarks}({graph.Actors[i].LocalId})"
                    });
                }

                valueDropdown[field] = temp.ToArray();
                return true;
            }

            if (valuesGetter == "@" + nameof(OdinDropdownHelper) + "." +
                nameof(OdinDropdownHelper.GetSceneGroupRouteIds) + "()")
            {
                temp.Clear();
                for (int i = 0; i < m_Graph.values.Count; i++)
                {
                    if (m_Graph.values[i] is RouteNode routeNode)
                    {
                        temp.Add(new ValueDropdownItem()
                        {
                            Value = routeNode.Route.LocalId,
                            Text = string.IsNullOrEmpty(routeNode.Route.Remarks)
                                ? routeNode.Route.LocalId.ToString()
                                : $"{routeNode.Route.Remarks}({routeNode.Route.LocalId})"
                        });
                    }

                }

                valueDropdown[field] = temp.ToArray();
                return true;
            }

            if (valuesGetter == "@" + nameof(OdinDropdownHelper) + "." +
                nameof(OdinDropdownHelper.GetSceneGroupZoneIds) + "()")
            {
                temp.Clear();
                var graph = m_Graph;
                for (int i = 0; i < graph.Zones.Length; i++)
                {
                    temp.Add(new ValueDropdownItem()
                    {
                        Value = graph.Zones[i].LocalId,
                        Text = string.IsNullOrEmpty(graph.Zones[i].Remarks)
                            ? graph.Zones[i].LocalId.ToString()
                            : $"{graph.Zones[i].Remarks}({graph.Zones[i].LocalId})"
                    });
                }

                valueDropdown[field] = temp.ToArray();
                return true;
            }

            return false;
        }
    }
}
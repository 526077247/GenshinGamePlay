using System;
using System.Collections;
using LitJson.Extensions;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace TaoTie
{
    public class ConfigFsmTable
    {
        private static readonly GUIStyle MiddleCenter = new GUIStyle() {alignment = TextAnchor.MiddleCenter};

        [LabelText("层")]
        public ConfigFsmTableLayer[] Layers = Array.Empty<ConfigFsmTableLayer>();
        
        [PropertySpace(40)][OnValueChanged(nameof(ChangeLayer))][JsonIgnore]
        [LabelText("当前层")][ValueDropdown(nameof(GetLayerIndex))]
        public int CurrentIndex = -1;
        
        [ShowIf("@"+nameof(FsmStates)+"!=null")]
        [OnCollectionChanged(nameof(RefreshTable))][LabelText("状态")][JsonIgnore]
        public ConfigFsmTableState[] FsmStates;
        
        [ShowIf("@"+nameof(DataTable)+"!=null")][JsonIgnore]
        [TableMatrix(DrawElementMethod = nameof(DrawCell), Labels = nameof(GetLabel), IsReadOnly = true,
            HorizontalTitle = "目标状态",VerticalTitle = "源状态")]
        public ConfigFsmTableItem[,] DataTable;

        [JsonIgnore]
        private UnOrderDoubleKeyDictionary<string,string,ConfigTransition[]> oldTable =
            new UnOrderDoubleKeyDictionary<string, string, ConfigTransition[]>();

        //todo: AnyStateTransition
        public void ChangeLayer()
        {
            if (CurrentIndex >=0 && CurrentIndex < Layers.Length)
            {
                if (Layers[CurrentIndex].DataTable == null)
                {
                    oldTable.Clear();
                    FsmStates = Layers[CurrentIndex].FsmStates;
                    DataTable = Layers[CurrentIndex].DataTable = new ConfigFsmTableItem[0, 0];
                }
                else if (DataTable != Layers[CurrentIndex].DataTable)
                {
                    oldTable.Clear();
                    FsmStates = Layers[CurrentIndex].FsmStates;
                    DataTable = Layers[CurrentIndex].DataTable;
                }
            }
            else
            {
                oldTable.Clear();
                DataTable = null;
                FsmStates = null;
            }
            RefreshTable();
        }
        public void RefreshTable()
        {
            if (DataTable != null)
            {
                foreach (var item in DataTable)
                {
                    if (!string.IsNullOrEmpty(item.FromState) && !string.IsNullOrEmpty(item.ToState))
                        oldTable.Add(item.FromState, item.ToState, item.Transitions);
                }
                Layers[CurrentIndex].FsmStates = FsmStates;
                Layers[CurrentIndex].DataTable = DataTable = new ConfigFsmTableItem[FsmStates.Length, FsmStates.Length];
                for (int i = 0; i < DataTable.GetLength(0); i++)
                {
                    for (int j = 0; j < DataTable.GetLength(1); j++)
                    {
                        DataTable[i, j] = new ConfigFsmTableItem()
                        {
                            FromState = FsmStates[j]?.Name,
                            ToState = FsmStates[i]?.Name,
                            Transitions = FindItemData(FsmStates[j], FsmStates[i])
                        };
                    }
                }
            }
        }
        public ConfigFsmTableItem DrawCell(Rect rect, ConfigFsmTableItem value)
        {
            if (value.FromState != value.ToState)
            {
                bool isEmpty = value.Transitions == null || value.Transitions.Length <= 0;
                EditorGUI.DrawRect(rect, !isEmpty ? new Color(0.1f, 0.8f, 0.2f) : Color.gray);
                EditorGUI.LabelField(rect, (isEmpty? "创建" : "编辑") + value.FromState +" => "+ value.ToState, MiddleCenter);
                if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                {
                    var itemE = EditorWindow.GetWindow<FsmTableItemEditor>();
                    itemE.Data = value;
                    itemE.Show();
                    GUI.changed = true;
                    Event.current.Use();
                }
            }
            else
            {
                bool isEmpty = value.Transitions == null || value.Transitions.Length <= 0;
                EditorGUI.DrawRect(rect, !isEmpty ? new Color(0,1f,1f) : new Color(0,0.4f,0.4f));
                EditorGUI.LabelField(rect, (isEmpty? "创建" : "编辑") + "AnyState" +" => "+ value.ToState, MiddleCenter);
                if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                {
                    var itemE = EditorWindow.GetWindow<FsmTableItemEditor>();
                    itemE.Data = value;
                    itemE.Show();
                    GUI.changed = true;
                    Event.current.Use();
                }
            }

            return value;
        }
        
        private (string, LabelDirection) GetLabel(ConfigFsmTableItem[,] array, TableAxis axis, int index)
        {
            switch (axis)
            {
                case TableAxis.Y:
                    for (int i = 0; i < array.GetLength(0); i++)
                    {
                        array[i,index].FromState = FsmStates[index].Name;
                    }
                    return (FsmStates[index].Name, LabelDirection.LeftToRight);
                case TableAxis.X:
                    for (int i = 0; i < array.GetLength(1); i++)
                    {
                        array[index, i].ToState = FsmStates[index].Name;
                    }
                    return (FsmStates[index].Name, LabelDirection.TopToBottom);
                default:
                    return (index.ToString(), LabelDirection.LeftToRight);
            }
        }

        private ConfigTransition[] FindItemData(ConfigFsmTableState from, ConfigFsmTableState to)
        {
            if (string.IsNullOrEmpty(from?.Name) || string.IsNullOrEmpty(to?.Name))
            {
                return null;
            }
            if (oldTable.TryGetValue(from.Name, to.Name, out var res))
            {
                return res;
            }
            return null;
        }
        
        public IEnumerable GetLayerIndex()
        {
            ValueDropdownList<int> list = new ValueDropdownList<int>();
            list.Add($"无", -1);
            if (Layers!=null && Layers.Length > 0)
            {
                for (int i = 0; i < Layers.Length; i++)
                {
                    list.Add($"{Layers[i].Name}({i})", i);
                }
            }
            return list;
        }
    }
}
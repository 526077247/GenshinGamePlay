using System.Collections;
using System.Collections.Generic;
using System;
using SuperScrollView;
using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{
	public class UIOpView : UIBaseView, IOnCreate, IOnEnable,IOnDisable
	{

		public static string PrefabPath => "UIGame/UIMain/Prefabs/UIOpView.prefab";
		public UILoopListView2 ScrollView;

		private UnOrderDoubleKeyDictionary<long, int, (long, int,ConfigInteeItem)> showMap;
		private List<(long, int,ConfigInteeItem)> showList;
		private int activeIndex = 0;
		#region override
		public void OnCreate()
		{
			ScrollView = this.AddComponent<UILoopListView2>("ScrollView");
			showMap = new UnOrderDoubleKeyDictionary<long, int, (long, int,ConfigInteeItem)>();
			showList = new List<(long, int,ConfigInteeItem)>();
		}
		public void OnEnable()
		{
			ScrollView.SetActive(showList.Count>0);
			ScrollView.InitListView(showList.Count,GetScrollViewItemByIndex);
			Messager.Instance.AddListener<long,bool,ConfigInteeItem>(0, MessageId.ShowIntee, OnShowIntee);
			Messager.Instance.AddListener<int,int>(0, MessageId.OnKeyInput, OnKeyInput);
		}

		public void OnDisable()
		{
			Messager.Instance.RemoveListener<int,int>(0, MessageId.OnKeyInput, OnKeyInput);
			Messager.Instance.RemoveListener<long,bool,ConfigInteeItem>(0, MessageId.ShowIntee, OnShowIntee);
			showMap.Clear();
			showList.Clear();
		}
		
		#endregion

		#region 事件绑定
		public LoopListViewItem2 GetScrollViewItemByIndex(LoopListView2 listView, int index)
		{
			if (index < 0 || index >= showList.Count) return null;
			var item = listView.NewListViewItem("UIOpItem");
			UIOpItem uiitem;
			if (!item.IsInitHandlerCalled)
			{
				item.IsInitHandlerCalled = true;
				uiitem = ScrollView.AddItemViewComponent<UIOpItem>(item);
			}
			else
			{
				uiitem = ScrollView.GetUIItemView<UIOpItem>(item);
			}

			uiitem.SetData(showList[index].Item3, activeIndex == index);
			return item;
		}

		private void OnShowIntee(long entityId, bool show, ConfigInteeItem config)
		{
			if (showMap.TryGetValue(entityId, config.LocalId,out var conf))
			{
				if (!show)
				{
					showMap.Remove(entityId, config.LocalId);
					showList.Remove(conf);
					ScrollView.SetActive(showList.Count>0);
					ScrollView.SetListItemCount(showList.Count);
					ScrollView.RefreshAllShownItem();
				}
			}
			else
			{
				if (show)
				{
					var data = (entityId, config.LocalId, config);
					showMap.Add(entityId, config.LocalId,data);
					showList.Add(data);
					ScrollView.SetActive(showList.Count>0);
					ScrollView.SetListItemCount(showList.Count);
					ScrollView.RefreshAllShownItem();
				}
			}
		}

		private void OnKeyInput(int key, int state)
		{
			if (key == (int) GameKeyCode.Opera1 && (state | InputManager.Key) != 0)
			{
				if (showList.Count > activeIndex)
				{
					var data = showList[activeIndex];
					if (SceneManager.Instance.GetCurrentScene() is BaseMapScene map)
					{
						var unit = map.GetManager<EntityManager>()?.Get<Unit>(data.Item1);
						if (unit != null)
						{
							unit.GetComponent<InteeComponent>()?.OnClickIntee(data.Item2);
						}
					}
				}
			}
		}

		#endregion
	}
}

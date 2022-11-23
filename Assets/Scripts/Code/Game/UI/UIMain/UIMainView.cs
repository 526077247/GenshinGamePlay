using System.Collections.Generic;
using System;
using SuperScrollView;

namespace TaoTie
{
	public class UIMainView : UIBaseView, IOnCreate, IOnEnable
	{
		public static string PrefabPath => "UI/UIMain/Prefabs/UIMainView.prefab";
		public UIImage Image;
		public UIText Text;
		public UIMenu Menu;

		public UILoopGridView LoopGridView;
		public UILoopListView2 LoopListView2;
		public UIBaseContainer Welcome;

		public int CurId;
		
		public DateTime FirstDay;
		public int TotalDay;
		
		private Dictionary<int, string> Config = new Dictionary<int, string>()
		{
			{1, "欢迎"},
			{2, "网格列表"},
			{3, "无限循环滚动列表"},
		};
		#region override
		public void OnCreate()
		{
			this.Image = this.AddComponent<UIImage>("Image");
			this.Text = this.AddComponent<UIText>("Text");
			Menu = AddComponent<UIMenu>("UIMenu");
			this.LoopGridView = this.AddComponent<UILoopGridView>("ScrollList/LoopGrid");
			this.LoopGridView.InitGridView(0,OnGetGridItemByIndex);
			this.LoopListView2 = this.AddComponent<UILoopListView2>("ScrollList/LoopList");
			this.LoopListView2.InitListView(0,OnGetListItemByIndex);
			Welcome = this.AddComponent<UIEmptyView>("ScrollList/Welcome");
			//模拟读配置
			List<MenuPara> paras = new List<MenuPara>();
			foreach (var item in Config)
			{
				paras.Add(new MenuPara()
				{
					Id = item.Key,
					Name = item.Value
				});
			}
			Menu.SetData(paras,OnMenuIndexChanged);
		}
		public void OnEnable()
		{
			Menu.SetActiveIndex(0,true);
		}
		#endregion

		#region 事件绑定
		
		public void OnMenuIndexChanged(MenuPara para)
		{
			CurId = para.Id;
			RefreshItemSpaceShow();
		}

		public LoopGridViewItem OnGetGridItemByIndex(LoopGridView gridView, int index, int row, int column)
		{
			if (index < 0 || index >= TotalDay) return null;
			LoopGridViewItem item;
			var date = FirstDay.AddDays(index - (int) FirstDay.DayOfWeek);
			if (index < (int) FirstDay.DayOfWeek)
			{
				item = gridView.NewListViewItem("EmptyItem");
				if (!item.IsInitHandlerCalled)
				{
					item.IsInitHandlerCalled = true;
				}
			}
			else
			{
				item = gridView.NewListViewItem("CellItem");
				CellItem cellItem;
				if (!item.IsInitHandlerCalled)
				{
					item.IsInitHandlerCalled = true;
					cellItem = this.LoopGridView.AddItemViewComponent<CellItem>(item);
				}
				else
				{
					cellItem = this.LoopGridView.GetUIItemView<CellItem>(item);
				}
				cellItem.SetData(date);
			}
			
			return item;
		}
		
		public LoopListViewItem2 OnGetListItemByIndex(LoopListView2 listView, int index)
		{
			LoopListViewItem2 item = listView.NewListViewItem("DateItem");
			DateItem dateItem;
			if (!item.IsInitHandlerCalled)
			{
				item.IsInitHandlerCalled = true;
				dateItem = this.LoopListView2.AddItemViewComponent<DateItem>(item);
			}
			else
			{
				dateItem = this.LoopListView2.GetUIItemView<DateItem>(item);
			}
			dateItem.SetData(index);
			return item;
		}
		#endregion

		public void RefreshItemSpaceShow()
		{
			var conf = Config[CurId];
			this.Text.SetText(conf);
			switch (CurId)
			{
				case 1:
					this.Welcome.SetActive(true);
					this.LoopGridView.SetActive(false);
					this.LoopListView2.SetActive(false);
					break;
				case 2:
					this.Welcome.SetActive(false);
					this.LoopGridView.SetActive(true);
					this.LoopListView2.SetActive(false);
					DateTime dtNow = DateTime.Now;     
					FirstDay = DateTime.Now.AddDays(1 - DateTime.Now.Day).Date;
					TotalDay = DateTime.DaysInMonth(dtNow.Year ,dtNow.Month)+(int) FirstDay.DayOfWeek;
					this.LoopGridView.SetListItemCount(TotalDay);
					this.LoopGridView.RefreshAllShownItem();
					break;
				case 3:
					this.Welcome.SetActive(false);
					this.LoopGridView.SetActive(false);
					this.LoopListView2.SetActive(true);
					this.LoopListView2.SetListItemCount(-1);
					this.LoopListView2.RefreshAllShownItem();
					break;
				default:
					this.Welcome.SetActive(false);
					this.LoopGridView.SetActive(false);
					this.LoopListView2.SetActive(false);
					break;
			}
		}
	}
}

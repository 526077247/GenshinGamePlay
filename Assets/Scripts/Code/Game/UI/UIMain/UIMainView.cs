using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{
	public class UIMainView : UIBaseView, IOnCreate, IOnEnable
	{
		public static string PrefabPath => "UI/UIMain/Prefabs/UIMainView.prefab";
		public UIImage Image;
		public UIText Text;
		public UIMenu Menu;

		public int CurId;
		
		private Dictionary<int, string> Config = new Dictionary<int, string>()
		{
			{1, "Menu1"},
			{2, "Menu2"},
			{3, "Menu3"},
			{4, "Menu4"},
		};
		#region override
		public void OnCreate()
		{
			this.Image = this.AddComponent<UIImage>("Image");
			this.Text = this.AddComponent<UIText>("Text");
			Menu = AddComponent<UIMenu>("UIMenu");
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
		#endregion

		public void RefreshItemSpaceShow()
		{
			var conf = Config[CurId];
			this.Text.SetText(conf);
		}
	}
}

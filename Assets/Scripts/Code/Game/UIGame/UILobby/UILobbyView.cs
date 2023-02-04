using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{
	public class UILobbyView : UIBaseView, IOnCreate, IOnEnable
	{
		public static string PrefabPath => "UIGame/UILobby/Prefab/UILobbyView.prefab";
		public UIText txt_gold;
		public UIButton btn_option;
		public UIButton btn_start;
		public UIButton btn_collection;
		public UIButton btn_record;
		public UIButton btn_strength;
		public UIButton btn_production;
		

		#region override
		public void OnCreate()
		{
			this.txt_gold = this.AddComponent<UIText>("bg_txt_gold/txt_gold");
			this.btn_option = this.AddComponent<UIButton>("btn_option");
			this.btn_start = this.AddComponent<UIButton>("btn_start");
			this.btn_collection = this.AddComponent<UIButton>("btn_collection");
			this.btn_record = this.AddComponent<UIButton>("btn_record");
			this.btn_strength = this.AddComponent<UIButton>("btn_strength");
			this.btn_production = this.AddComponent<UIButton>("btn_production");
		}
		public void OnEnable()
		{
			this.btn_option.SetOnClick(OnClickbtn_option);
			this.btn_start.SetOnClick(OnClickbtn_start);
			this.btn_collection.SetOnClick(OnClickbtn_collection);
			this.btn_record.SetOnClick(OnClickbtn_record);
			this.btn_strength.SetOnClick(OnClickbtn_strength);
			this.btn_production.SetOnClick(OnClickbtn_production);
		}
		#endregion

		#region 事件绑定
		public void OnClickbtn_option()
		{

		}
		public void OnClickbtn_start()
		{

		}
		public void OnClickbtn_collection()
		{

		}
		public void OnClickbtn_record()
		{

		}
		public void OnClickbtn_strength()
		{

		}
		public void OnClickbtn_production()
		{

		}
		#endregion
	}
}

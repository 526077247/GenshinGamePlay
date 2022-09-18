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
		

		#region override
		public void OnCreate()
		{
			this.Image = this.AddComponent<UIImage>("Image");
			this.Text = this.AddComponent<UIText>("Text");
		}
		public void OnEnable()
		{
		}
		#endregion

		#region 事件绑定
		#endregion
	}
}

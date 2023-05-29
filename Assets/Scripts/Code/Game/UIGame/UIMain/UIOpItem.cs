using System.Collections;
using System.Collections.Generic;
using System;
using SuperScrollView;
using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{
	public class UIOpItem : UIBaseContainer, IOnCreate, IOnEnable
	{
		public UIImage Active;
		public UIImage Icon;
		public UITextmesh Content;
		

		#region override
		public void OnCreate()
		{
			this.Active = this.AddComponent<UIImage>("Active");
			this.Icon = this.AddComponent<UIImage>("Icon");
			this.Content = this.AddComponent<UITextmesh>("Content");
		}
		public void OnEnable()
		{
		}
		#endregion

		#region 事件绑定
		#endregion

		public void SetData(ConfigInteeItem config, bool active)
		{
			Active.SetActive(active);
			Icon.SetActive(false);//Icon先屏蔽
			Content.SetI18NKey(config.I18NKey, config.I18NParams);
		}
	}
}

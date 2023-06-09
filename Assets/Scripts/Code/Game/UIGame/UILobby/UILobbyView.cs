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
		public UIButton btn_start;
		
		#region override
		public void OnCreate()
		{
			this.btn_start = this.AddComponent<UIButton>("root/Image/btn_start");
		}
		public void OnEnable()
		{
			this.btn_start.SetOnClick(OnClickbtn_start);
		}
		#endregion

		#region 事件绑定
		public void OnClickbtn_start()
		{
			SceneManager.Instance.SwitchMapScene("Sample").Coroutine();
		}
		#endregion
	}
}

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{
	public class UILobbyView : UIBaseView, IOnCreate, IOnEnable, IOnDisable
	{
		public static string PrefabPath => "UIGame/UILobby/Prefabs/UILobbyView.prefab";
		public UIButton btn_start;
		
		#region override
		public void OnCreate()
		{
			this.btn_start = this.AddComponent<UIButton>("root/Image/btn_start");
		}
		public void OnEnable()
		{
			this.btn_start.SetOnClick(OnClickbtn_start);
			this.btn_start.DOScale(1.1f, 800).SetLoops(-1, LoopType.Yoyo).SetEase(EasingFunction.Ease.EaseInOutSine);
		}
		#endregion

		#region 事件绑定
		public void OnClickbtn_start()
		{
			SceneManager.Instance.SwitchMapScene("Sample",Vector3.zero,Vector3.zero).Coroutine();
		}

		public void OnDisable()
		{
			TweenManager.Instance.KillTweens(this.btn_start);
		}
		#endregion
	}
}

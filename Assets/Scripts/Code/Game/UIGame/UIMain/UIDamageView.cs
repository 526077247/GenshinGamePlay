using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using Random = UnityEngine.Random;

namespace TaoTie
{
	public class UIDamageView : UIBaseView, IOnCreate, IOnEnable, IOnDisable
	{
		public static string PrefabPath => "UIGame/UIMain/Prefabs/UIDamageView.prefab";
		private List<DamageText> showFightTexts = new();
		private long fightTextExpireTime = 1000; //毫秒

		#region override

		public void OnCreate()
		{

		}

		public void OnEnable()
		{
			Messager.Instance.AddListener<AttackResult>(0, MessageId.ShowDamageText, ShowFightText);
		}

		public void OnDisable()
		{
			Messager.Instance.RemoveListener<AttackResult>(0, MessageId.ShowDamageText, ShowFightText);
		}

		public void Update()
		{
			if (showFightTexts.Count == 0) return;
			for (int i = showFightTexts.Count - 1; i >= 0; i--)
			{
				showFightTexts[i].UpdateText();
				if (showFightTexts[i].ExpireTime < GameTimerManager.Instance.GetTimeNow())
				{
					showFightTexts[i].Dispose();
					showFightTexts.RemoveAt(i);
				}
			}
		}

		#endregion

		public void ShowFightText(AttackResult ar)
		{
			DamageText ft = DamageText.Create(this);
			long expireTime = GameTimerManager.Instance.GetTimeNow() + fightTextExpireTime;
			ft.SetData(ar.FinalRealDamage, ar.HitInfo.HitPos + Random.onUnitSphere / 10, expireTime);
			showFightTexts.Add(ft);
		}
	}
}
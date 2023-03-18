using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

namespace TaoTie
{
	public class UIHudView : UIBaseView, IOnCreate, IOnEnable, IUpdateManager
    {
		public static string PrefabPath => "UIGame/UIBattle/Prefabs/UIHudView.prefab";
		private  Queue<FightText> fightTexts = new();
		private List<FightText>showFightTexts = new();
		private long fightTextExpireTime = 1000;	//毫秒
		
		#region override
		public void OnCreate()
		{

		}
		public void OnEnable()
		{

		}

		public void Update()
		{
            if (showFightTexts.Count == 0) return;
            for (int i = showFightTexts.Count - 1; i >= 0; i--)
            {
                if (showFightTexts[i].expire_time < GameTimerManager.Instance.GetTimeNow())
                {
					showFightTexts[i].SetActive(false);
					fightTexts.Enqueue(showFightTexts[i]);
					showFightTexts.RemoveAt(i);
                }
			}
		}
		#endregion

		public void ShowFightText(AttackResult ar)
		{
            if (fightTexts.Count == 0)
            {
				FightText new_ft = new();
				new_ft.OnInit(this).Coroutine();
				fightTexts.Enqueue(new_ft);
			}
            FightText ft = fightTexts.Dequeue();
			ft.SetActive(true);
			long expire_time = GameTimerManager.Instance.GetTimeNow() + fightTextExpireTime;
			ft.SetData(ar.FinalRealDamage, ar.HitInfo.HitPos,expire_time);
			showFightTexts.Add(ft);
        }
	}
}

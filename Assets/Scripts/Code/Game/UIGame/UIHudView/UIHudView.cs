using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

namespace TaoTie
{
	public class UIHudView : UIBaseView, IOnCreate, IOnEnable
	{
		public static string PrefabPath => "UIGame/UIBattle/Prefabs/UIHudView.prefab";
		private  Queue<FightText> fightTexts = new();
		private List<FightText>showFightTexts = new();

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
            for (int i = showFightTexts.Count; i > 0; i--)
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
				GCHandle h = GCHandle.Alloc(new_ft);
				IntPtr addr = GCHandle.ToIntPtr(h);
				Debug.Log("zzz new add " + addr.ToString());
				GCHandle h3 = GCHandle.Alloc(fightTexts);
				IntPtr addr3 = GCHandle.ToIntPtr(h3);
				Debug.Log("zzz new queue " + addr3.ToString());
			}
			FightText ft = fightTexts.Dequeue();
			GCHandle h2 = GCHandle.Alloc(ft);
			IntPtr addr2 =GCHandle.ToIntPtr(h2);
			Debug.Log("zzz deq add " + addr2.ToString());
			ft.SetActive(true);
			long expire_time = GameTimerManager.Instance.GetTimeNow() + 1;
			//ft.SetData(ar,expire_time);
		}
	}
}

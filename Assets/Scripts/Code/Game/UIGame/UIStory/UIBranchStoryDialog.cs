using System.Collections;
using System.Collections.Generic;
using System;
using SuperScrollView;
using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{
	public class UIBranchStoryDialog : UIBaseView, IOnCreate, IOnEnable<ConfigStoryBranchClip>
	{
		public static string PrefabPath => "UIGame/UIStory/Prefabs/UIBranchStoryDialog.prefab";
		public UICopyGameObject Panel;

		private ConfigStoryBranchClip data;
		private int choice = -1;
		private ETTask<int> waitTask;
		#region override
		public void OnCreate()
		{
			this.Panel = this.AddComponent<UICopyGameObject>("Panel");
		}
		public void OnEnable(ConfigStoryBranchClip clip)
		{
			this.waitTask = null;
			this.choice = -1;
			this.data = clip;
			this.Panel.InitListView(data.Branchs.Length,GetPanelItemByIndex);
		}
		#endregion

		#region 事件绑定
		public void GetPanelItemByIndex(int index, GameObject obj)
		{
			var item = Panel.GetUIItemView<StoryChoiceItem>(obj);
			if (item == null)
			{
				item = Panel.AddItemViewComponent<StoryChoiceItem>(obj);
			}
			item.SetData(data.Branchs[index],index,OnClickChoice);
		}
		#endregion

		private void OnClickChoice(int index)
		{
			if (this.waitTask != null)
			{
				var task = this.waitTask;
				this.waitTask = null;
				task.SetResult(index);
			}
			else
			{
				this.choice = index;
			}
		}

		public async ETTask<int> WaitChoose()
		{
			if (this.choice >= 0) return this.choice;
			waitTask = ETTask<int>.Create(true);
			return await waitTask;
		}
	}
}

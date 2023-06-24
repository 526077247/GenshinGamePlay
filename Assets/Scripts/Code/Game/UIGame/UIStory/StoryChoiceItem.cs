using System.Collections;
using System.Collections.Generic;
using System;
using SuperScrollView;
using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{
	public class StoryChoiceItem : UIBaseContainer, IOnCreate
	{
		public UITextmesh Text;
		public UIPointerClick Btn;
		
		private Action<int> onClick;
		private int index;
		
		#region override
		public void OnCreate()
		{
			this.Text = this.AddComponent<UITextmesh>("Text");
			this.Btn = this.AddComponent<UIPointerClick>();
			this.Btn.SetOnClick(OnClick);
		}

		#endregion

		#region 事件绑定

		
		public void OnClick()
		{
			this.onClick.Invoke(this.index);
		}
		#endregion

		public void SetData(ConfigStoryBranchClipItem data,int index,Action<int> onClick)
		{
			this.onClick = onClick;
			this.index = index;
			this.Text.SetText(data.Text.GetShowText());
		}
	}
}

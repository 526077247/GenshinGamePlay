using System.Collections;
using System.Collections.Generic;
using System;
using SuperScrollView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TaoTie
{
	public class UICommonStoryDialog : UIBaseView, IOnCreate, IOnEnable<ConfigCommonDialogClip>
	{
		public static string PrefabPath => "UIGame/UIStory/Prefabs/UICommonStoryDialog.prefab";
		public UIRawImage Bg;
		public UITextmesh Content;

		private ConfigCommonDialogClip data;
		private string showText;
		#region override
		public void OnCreate()
		{
			this.Bg = this.AddComponent<UIRawImage>("Bg");
			this.Content = this.AddComponent<UITextmesh>("Dialog/Viewport/Content");
		}

		public void OnEnable(ConfigCommonDialogClip clip)
		{
			data = clip;
			this.Bg.SetActive(data.BackgroundBlur);
			showText = data.Text.GetShowText();
			this.Content.SetText(showText);
			this.Content.SetMaxVisibleCharacters(0);
		}
		#endregion


		public async ETTask Play()
		{
			if (data != null)
			{
				if (data.Typewriter)
				{
					ETCancellationToken token = new ETCancellationToken();
					for (int i = 0; i < showText.Length; i++)
					{
						this.Content.SetMaxVisibleCharacters(i + 1);
						await TimerManager.Instance.WaitAsync(50, token);
					}
				}
				else
				{
					this.Content.SetMaxVisibleCharacters(showText.Length + 1);
				}

				if (!data.WaitClick)
				{
					await TimerManager.Instance.WaitAsync(data.WaitTime);
				}
				else
				{
					while (data.WaitClick)
					{
						await TimerManager.Instance.WaitAsync(1);
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
			            if (Input.touchCount>=1)
			            {
			                var pos = Input.GetTouch(0).position;
			                if (InputManager.IsPointerOverGameObject(pos))
			                {
#else
						if (Input.GetKeyDown(KeyCode.Mouse0))
						{
							if (EventSystem.current.IsPointerOverGameObject())
							{
#endif

								return;
							}
						}
					}
				}
				
			}
		}
	}
}

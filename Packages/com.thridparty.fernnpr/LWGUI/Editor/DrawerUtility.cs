// Copyright (c) 2022 Jason Ma

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace LWGUI
{
	/// when LwguiEventType.Init:		get all metadata from drawer
	/// when LwguiEventType.Repaint:	LWGUI decides how to draw each prop according to metadata
	internal enum LwguiEventType
	{
		Init,
		Repaint
	}
	
	internal enum SearchMode
	{
		All,
		Modified
	}
	
	internal class LWGUI : ShaderGUI
    {
		public static readonly float helpboxSingleLineHeight = 12.5f;

		public MaterialProperty[]                                props;
		public MaterialEditor                                    materialEditor;
		public Dictionary<string /*PropName*/, bool /*Display*/> searchResult;
		public string                                            searchingText     = String.Empty;
		public string                                            lastSearchingText = String.Empty;
		public SearchMode                                        searchMode        = SearchMode.All;
		public SearchMode                                        lastSearchMode    = SearchMode.All;
		public bool                                              updateSearchMode  = false;
		public LwguiEventType                                    lwguiEventType    = LwguiEventType.Init;
		public Shader                                            shader;

		/// <summary>
		/// Called when switch to a new Material Window, each window has a LWGUI instance
		/// </summary>
		public LWGUI() { }
		
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            this.props = props;
            this.materialEditor = materialEditor;
			this.shader = (materialEditor.target as Material).shader;
			this.lwguiEventType = RevertableHelper.InitAndHasShaderChanged(shader, materialEditor.target) ? LwguiEventType.Init : LwguiEventType.Repaint;

			// drawer register metadata
			if (lwguiEventType == LwguiEventType.Init)
			{
				// reset all caches
				MetaDataHelper.ClearCaches(shader);
				searchResult = null;
				lastSearchingText = searchingText = string.Empty;
				lastSearchMode = searchMode = SearchMode.All;
				updateSearchMode = false;

				if (Event.current.type != UnityEngine.EventType.Repaint)
				{
					foreach (var prop in props)
					{
						var height = materialEditor.GetPropertyHeight(prop, prop.displayName);
						var rect = EditorGUILayout.GetControlRect(true, height, EditorStyles.layerMaskField);
						materialEditor.ShaderProperty(rect, prop, prop.displayName);
					}
				}
			}
			// draw with metadata and searchingText
			else if (lwguiEventType == LwguiEventType.Repaint)
			{
				// Search Field
				if (searchResult == null)
					searchResult = MetaDataHelper.SearchProperties(shader, props, String.Empty, searchMode);
				if (Helper.DrawSearchField(ref searchingText, ref searchMode, this) || updateSearchMode)
				{
					// change anything to expand all group
					if ((string.IsNullOrEmpty(lastSearchingText) && lastSearchMode == SearchMode.All))	// last == init 
						GroupStateHelper.SetAllGroupFoldingAndCache(materialEditor.target, false);
					// restore to the cached state
					else if ((string.IsNullOrEmpty(searchingText) && searchMode == SearchMode.All))	// now == init
						GroupStateHelper.RestoreCachedFoldingState(materialEditor.target);
					
					searchResult = MetaDataHelper.SearchProperties(shader, props, searchingText, searchMode);
					lastSearchingText = searchingText;
					lastSearchMode = searchMode;
					updateSearchMode = false;
				}

				// move fields left to make rect for Revert Button
				materialEditor.SetDefaultGUIWidths();
				EditorGUIUtility.fieldWidth += RevertableHelper.revertButtonWidth;
				EditorGUIUtility.labelWidth -= RevertableHelper.revertButtonWidth;
				RevertableHelper.fieldWidth = EditorGUIUtility.fieldWidth;
				RevertableHelper.labelWidth = EditorGUIUtility.labelWidth;

				foreach (var prop in props)
				{
					if ((prop.flags & MaterialProperty.PropFlags.HideInInspector) == 0 && searchResult[prop.name])
					{
						var height = materialEditor.GetPropertyHeight(prop, prop.displayName);
						
						// ignored when in Folding Group
						if (height <= 0) continue;
						
						// helpbox
						int lineCount;
						var helpboxStr = MetaDataHelper.GetPropertyHelpbox(shader, prop, out lineCount);
						if (!string.IsNullOrEmpty(helpboxStr))
						{
							var helpboxRect = EditorGUILayout.GetControlRect(true, 30f + Mathf.Max(0, lineCount - 2f) * helpboxSingleLineHeight);
							if (MetaDataHelper.IsSubProperty(shader, prop))
							{
								EditorGUI.indentLevel++;
								helpboxRect = EditorGUI.IndentedRect(helpboxRect);
								EditorGUI.indentLevel--;
							}
							helpboxRect.xMax -= RevertableHelper.revertButtonWidth;
							EditorGUI.HelpBox(helpboxRect, helpboxStr, MessageType.Info);
						}
						
						var rect = EditorGUILayout.GetControlRect(true, height, EditorStyles.layerMaskField);
						var revertButtonRect = RevertableHelper.GetRevertButtonRect(prop, rect);
						rect.xMax -= RevertableHelper.revertButtonWidth;

						// Process some builtin types display misplaced
						switch (prop.type)
						{
							case MaterialProperty.PropType.Texture:
							case MaterialProperty.PropType.Range:
								materialEditor.SetDefaultGUIWidths();
								break;
							default:
								RevertableHelper.SetRevertableGUIWidths();
								break;
						}
						
						RevertableHelper.DrawRevertableProperty(revertButtonRect, prop, materialEditor, shader);
						var label = new GUIContent(prop.displayName, MetaDataHelper.GetPropertyTooltip(shader, prop));
						materialEditor.ShaderProperty(rect, prop, label);
					}
				}
			}
			
			materialEditor.SetDefaultGUIWidths();

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			if (SupportedRenderingFeatures.active.editableMaterialRenderQueue)
			{
				materialEditor.RenderQueueField();
			}
			materialEditor.EnableInstancingField();
			materialEditor.DoubleSidedGIField();
			
			// LWGUI logo
			EditorGUILayout.Space();
			Helper.DrawLogo();
		}

		/// <summary>
		///   <para>Find shader properties.</para>
		/// </summary>
		/// <param name="propertyName">The name of the material property.</param>
		/// <param name="properties">The array of available material properties.</param>
		/// <param name="propertyIsMandatory">If true then this method will throw an exception if a property with propertyName was not found.</param>
		/// <returns>
		///   <para>The material property found, otherwise null.</para>
		/// </returns>
		public static MaterialProperty FindProp(string propertyName, MaterialProperty[] properties, bool propertyIsMandatory = false)
        {
	        if (properties == null)
	        {
				Debug.LogWarning("Get other properties form Drawer is only support Unity 2019.2+!");
		        return null;
	        }
	        else if (!string.IsNullOrEmpty(propertyName) && propertyName != "_")
				return FindProperty(propertyName, properties, propertyIsMandatory);
			else
				return null;
        }
    }
	
	internal class GroupStateHelper
    {
		// Used to Folding Group, key: group name, value: is folding
		private static Dictionary<Object, Dictionary<string, bool>> _groups = new Dictionary<Object, Dictionary<string, bool>>();
		private static Dictionary<Object, Dictionary<string, bool>> _cachedGroups = new Dictionary<Object, Dictionary<string, bool>>();
		
		// Used to Conditional Display, key: keyword, value: is activated
		private static Dictionary<Object, Dictionary<string, bool>> _keywords = new Dictionary<Object, Dictionary<string, bool>>();

		// TODO: clear, reset to default, expand all, collapse all
		private static void InitPoolPerMaterial(Object material)
		{
			if (!_groups.ContainsKey(material)) _groups.Add(material, new Dictionary<string, bool>());
			if (!_cachedGroups.ContainsKey(material)) _cachedGroups.Add(material, new Dictionary<string, bool>());
			if (!_keywords.ContainsKey(material)) _keywords.Add(material, new Dictionary<string, bool>());
		}

		public static bool ContainsGroup(Object material, string group)
		{
			InitPoolPerMaterial(material);
			return _groups[material].ContainsKey(group);
		}

		public static void SetGroupFolding(Object material, string group, bool isFolding)
		{
			InitPoolPerMaterial(material);
			if (_groups[material].ContainsKey(group))
				_groups[material][group] = isFolding;
			else 
				_groups[material].Add(group, isFolding);
		}
		
		public static bool GetGroupFolding(Object material, string group)
		{
			InitPoolPerMaterial(material);
			Debug.Assert(_groups[material].ContainsKey(group), "Unknown Group: " + group);
			return _groups[material][group];
		}
		
		public static void SetAllGroupFoldingAndCache(Object material, bool isFolding)
		{
			InitPoolPerMaterial(material);
			_cachedGroups[material] = new Dictionary<string, bool>(_groups[material]);
			foreach (var group in _groups[material].Keys.ToArray())
			{
				_groups[material][group] = isFolding;
			}
		}

		public static void RestoreCachedFoldingState(Object material)
		{
			InitPoolPerMaterial(material);
			_groups[material] = new Dictionary<string, bool>(_cachedGroups[material]);
		}

		public static bool IsSubVisible(Object material, string group)
		{
			if (string.IsNullOrEmpty(group) || group == "_")
				return true;
			
			InitPoolPerMaterial(material);
			
			// common sub
			if (_groups[material].ContainsKey(group))
			{
				return !_groups[material][group];
			}
			// existing suffix, may be based on the enum conditions sub
			else
			{
				foreach (var prefix in _groups[material].Keys)
				{
					// prefix = group name, suffix = keyWord
					if (group.Contains(prefix))
					{
						string suffix = group.Substring(prefix.Length, group.Length - prefix.Length).ToUpperInvariant();
						string[] suffixSplit = suffix.Split('.');
						bool isKeywordActive = false;
						foreach (var suf in suffixSplit)
						{
							if (_keywords[material].ContainsKey(suf))
							{
								isKeywordActive = _keywords[material][suf];
								if(isKeywordActive) break;
							}
						}
						return isKeywordActive && !_groups[material][prefix];
					}
				}
				return false;
			}
		}

		public static void SetKeywordConditionalDisplay(Object material, string keyword, bool isDisplay)
		{
			if (string.IsNullOrEmpty(keyword) || keyword == "_") return;
			InitPoolPerMaterial(material);

			if (_keywords[material].ContainsKey(keyword))
				_keywords[material][keyword] = isDisplay;
			else
				_keywords[material].Add(keyword, isDisplay);
		}
    }

	
	/// <summary>
	/// Helpers for drawing Unreal Style Revertable Shader GUI 
	/// </summary>
	internal class RevertableHelper
	{
		public static readonly float    revertButtonWidth = 15f;
		public static          float    fieldWidth;
		public static          float    labelWidth;

		private static Dictionary<Shader /*Shader*/, Dictionary<string /*Prop Name*/, MaterialProperty /*Prop*/>> 
			_defaultProps = new Dictionary<Shader, Dictionary<string, MaterialProperty>>();

		private static Dictionary<Shader, int>    _initTimers     = new Dictionary<Shader, int>();
		private static Dictionary<Object, Shader> _lastShaders    = new Dictionary<Object, Shader>();
		private const  int                        INIT_PER_FRAMES = 1;


		#region Init

		private static void CheckProperty(Shader shader, MaterialProperty prop)
		{
			if (!_defaultProps.ContainsKey(shader) || !_defaultProps[shader].ContainsKey(prop.name))
			{
				Debug.LogWarning($"Uninitialized Shader:{shader.name} or Prop:{prop.name}");
				InitAndHasShaderChanged(shader);
			}
		}
		
		/// <summary>
		/// Detect Shader changes to know when to initialize
		/// </summary>
		public static bool InitAndHasShaderChanged(Shader shader, Object material = null)
		{
			bool equals = true;
			
			if (material != null && !_lastShaders.ContainsKey(material))
				_lastShaders.Add(material, null);

			// Init every few frames
			if (_initTimers.ContainsKey(shader))
				_initTimers[shader]++;
			else
				_initTimers[shader] = 0;

			// fast refresh can better capture the shadeer modification
			if (_initTimers[shader] >= INIT_PER_FRAMES
			 || !_defaultProps.ContainsKey(shader)
			 || ShaderUtil.GetPropertyCount(shader) != _defaultProps[shader].Count
			 || (material != null && _lastShaders[material] != shader)
			   )
			{
				if (material != null)
				{
					if (_lastShaders[material] != shader) equals = false;
					_lastShaders[material] = shader;
				}
				_initTimers[shader] = 0;
			}
			else
			{
				if (material != null) _lastShaders[material] = shader;
				return false;
			}
			
			// Get and cache new props
			var newProps = MaterialEditor.GetMaterialProperties(new[] { new Material(shader) });

			Dictionary<string, MaterialProperty> lastPropsDic;
			
			if (_defaultProps.ContainsKey(shader))
				lastPropsDic = _defaultProps[shader];
			else
			{
				lastPropsDic = new Dictionary<string, MaterialProperty>();
				equals = false;
			}
			
			_defaultProps[shader] = new Dictionary<string, MaterialProperty>();
			for (int i = 0; i < newProps.Length; i++)
			{
				if (_defaultProps[shader].ContainsKey(newProps[i].name)) 
					Debug.LogError($"Shader:{shader.name} has repeated property name:{newProps[i].name}!");
				_defaultProps[shader][newProps[i].name] = newProps[i];
				if (equals)
					equals = lastPropsDic.ContainsKey(newProps[i].name) && Helper.PropertyValueEquals(newProps[i], lastPropsDic[newProps[i].name]);
			}
			return !equals;
		}
		#endregion


		#region GUI Setting

		public static Rect GetRevertButtonRect(MaterialProperty prop, Rect rect, bool isCallInDrawer = false)
		{
			// TODO: use Reflection
			float defaultHeightWithoutDrawers = EditorGUIUtility.singleLineHeight;
			return GetRevertButtonRect(defaultHeightWithoutDrawers, rect, isCallInDrawer);
		}
		
		public static Rect GetRevertButtonRect(float propHeight, Rect rect, bool isCallInDrawer = false)
		{
			if (isCallInDrawer) rect.xMax += revertButtonWidth;
			var revertButtonRect = new Rect(rect.xMax - revertButtonWidth + 2f, 
											rect.yMax - propHeight * 0.5f - (revertButtonWidth - 3f) * 0.5f - 1f,
											revertButtonWidth - 2f, 
											revertButtonWidth - 3f);
			return revertButtonRect;
		}

		public static void SetRevertableGUIWidths()
		{
			EditorGUIUtility.fieldWidth = RevertableHelper.fieldWidth;
			EditorGUIUtility.labelWidth = RevertableHelper.labelWidth;
		}
		#endregion


		#region Property Handle

		public static void SetPropertyToDefault(MaterialProperty defaultProp, MaterialProperty prop)
		{
			prop.vectorValue = defaultProp.vectorValue;
			prop.colorValue = defaultProp.colorValue;
			prop.floatValue = defaultProp.floatValue;
			prop.textureValue = defaultProp.textureValue;
#if UNITY_2021_1_OR_NEWER
			prop.intValue = defaultProp.intValue;
#endif
		}
		
		public static void SetPropertyToDefault(Shader shader, MaterialProperty prop)
		{
			CheckProperty(shader, prop);
			var defaultProp = _defaultProps[shader][prop.name];
			SetPropertyToDefault(defaultProp, prop);
		}

		public static MaterialProperty GetDefaultProperty(Shader shader, MaterialProperty prop)
		{
			CheckProperty(shader, prop);
			return _defaultProps[shader][prop.name];
		}

		public static string GetPropertyDefaultValueText(Shader shader, MaterialProperty prop)
		{
			var defaultProp = GetDefaultProperty(shader, prop);
			string defaultText = String.Empty;
			switch (defaultProp.type)
			{
				case MaterialProperty.PropType.Color:
					defaultText += defaultProp.colorValue;
					break;
				case MaterialProperty.PropType.Float:
				case MaterialProperty.PropType.Range:
					defaultText += defaultProp.floatValue;
					break;
#if UNITY_2021_1_OR_NEWER
				case MaterialProperty.PropType.Int:
					defaultText += defaultProp.intValue;
					break;
#endif
				case MaterialProperty.PropType.Texture:
					defaultText += defaultProp.textureValue != null ? defaultProp.textureValue.name : "None";
					break;
				case MaterialProperty.PropType.Vector:
					defaultText += defaultProp.vectorValue;
					break;
			}
			return defaultText;
		}

		public static bool IsDefaultProperty(Shader shader, MaterialProperty prop)
		{
			CheckProperty(shader, prop);
			return Helper.PropertyValueEquals(prop, _defaultProps[shader][prop.name]);
		}
		#endregion


		#region Draw revert button

		public static bool DrawRevertableProperty(Rect position, MaterialProperty prop, MaterialEditor materialEditor, Shader shader)
		{
			CheckProperty(shader, prop);
			var defaultProp = _defaultProps[shader][prop.name];
			Rect rect = position;
			if (Helper.PropertyValueEquals(prop, defaultProp) && !prop.hasMixedValue)
				return false;
			if (DrawRevertButton(rect))
			{
				AddPropertyShouldRevert(prop.targets, prop.name);
				SetPropertyToDefault(defaultProp, prop);
				// refresh keywords
				MaterialEditor.ApplyMaterialPropertyDrawers(materialEditor.targets);
				return true;
			}
			return false;
		}

		private static readonly Texture _icon = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("e7bc1130858d984488bca32b8512ca96"));
		public static bool DrawRevertButton(Rect rect)
		{
			if (_icon == null) Debug.LogError("RevertIcon.png + meta is missing!");
			GUI.DrawTexture(rect, _icon);
			var e = Event.current;
			if (e.type == UnityEngine.EventType.MouseDown && rect.Contains(e.mousePosition))
			{
				e.Use();
				return true;
			}
			return false;
		}
		#endregion


		#region Call drawers to do revert and refresh keywords

		private static Dictionary<Object, List<string>> _shouldRevertPropsPool;

		public static void AddPropertyShouldRevert(Object[] materials, string propName)
		{
			if (_shouldRevertPropsPool == null)
				_shouldRevertPropsPool = new Dictionary<Object, List<string>>();
			foreach (var material in materials)
			{
				if (_shouldRevertPropsPool.ContainsKey(material))
				{
					if (!_shouldRevertPropsPool[material].Contains(propName))
						_shouldRevertPropsPool[material].Add(propName);
				}
				else
				{
					_shouldRevertPropsPool.Add(material, new List<string> { propName });
				}
			}
		}

		public static void RemovePropertyShouldRevert(Object[] materials, string propName)
		{
			if (_shouldRevertPropsPool == null) return;
			foreach (var material in materials)
			{
				if (_shouldRevertPropsPool.ContainsKey(material))
				{
					if (_shouldRevertPropsPool[material].Contains(propName))
						_shouldRevertPropsPool[material].Remove(propName);
				}
			}
		}

		public static bool IsPropertyShouldRevert(Object material, string propName)
		{
			if (_shouldRevertPropsPool == null) return false;
			if (_shouldRevertPropsPool.ContainsKey(material))
			{
				return _shouldRevertPropsPool[material].Contains(propName);
			}
			else
			{
				return false;
			}
		}
		#endregion

	}

	/// <summary>
	/// Misc Function
	/// </summary>
	internal class Helper
    {
#region Engine Misc

		public static void ObsoleteWarning(string obsoleteStr, string newStr)
		{
			Debug.LogWarning("'"+obsoleteStr+"' is Obsolete! Please use '"+newStr+"'!");
		}
		public static bool PropertyValueEquals(MaterialProperty prop1, MaterialProperty prop2)
		{
			if (prop1.textureValue == prop2.textureValue
			 && prop1.vectorValue == prop2.vectorValue
			 && prop1.colorValue == prop2.colorValue
			 && prop1.floatValue == prop2.floatValue
#if UNITY_2021_1_OR_NEWER
			 && prop1.intValue == prop2.intValue
#endif
			   )
				return true;
			else
				return false;
		}

		public static string GetKeyWord(string keyWord, string propName)
		{
			string k;
			if (string.IsNullOrEmpty(keyWord) || keyWord == "__")
			{
				k = propName.ToUpperInvariant() + "_ON";
			}
			else
			{
				k = keyWord.ToUpperInvariant();
			}
			return k;
		}

		public static void SetShaderKeyWord(Object[] materials, string keyWord, bool isEnable)
		{
			if (string.IsNullOrEmpty(keyWord) || string.IsNullOrEmpty(keyWord)) return;
			
			foreach (Material m in materials)
			{
				// delete "_" keywords
				if(keyWord == "_")
				{
					if (m.IsKeywordEnabled(keyWord))
					{
						m.DisableKeyword(keyWord);
					}
					continue;
				}
                
				if (m.IsKeywordEnabled(keyWord))
				{
					if (!isEnable) m.DisableKeyword(keyWord);
				}
				else
				{
					if (isEnable) m.EnableKeyword(keyWord);
				}
			}
		}

		public static void SetShaderKeyWord(Object[] materials, string[] keyWords, int index)
		{
		    if(!(keyWords.Length >= 1 && index < keyWords.Length && index >= 0)) return;
			for (int i = 0; i < keyWords.Length; i++)
			{
				SetShaderKeyWord(materials, keyWords[i], index == i);
			}
		}

		public class PropertyInfo
		{
			public static readonly string Surface = "_Surface";
			public static readonly string Blend = "_Blend";
			public static readonly string Cull = "_Cull";
			public static readonly string AlphaTest = "_AlphaClip";
			public static readonly string Cutoff = "_Cutoff";
			public static readonly string ZTest = "_ZTest";
			public static readonly string ZWrite = "_ZWrite";
			public static readonly string SrcBlend = "_SrcBlend";
			public static readonly string DstBlend = "_DstBlend";
			public static readonly string QueueOffset = "_QueueOffset";
		}

		public static void SetQueueOffset(UnityEngine.Object[] materials)
		{
			foreach (Material m in materials)
			{
				var queueOffset = m.GetFloat(PropertyInfo.QueueOffset);
				m.SetInt(PropertyInfo.QueueOffset, (int)queueOffset);
				var renderTypeTag = m.GetTag("RenderType", false, "Opaque");
				switch (renderTypeTag)
				{
					case "Opaque":
						m.renderQueue = (int)queueOffset + 2000;
						break;
					case "TransparentCutout":
						m.renderQueue = (int)queueOffset + 2450;
						break;
					case "Transparent":
						m.renderQueue = (int)queueOffset + 3000;
						break;
				}
			}
		}
		
		public static void SetSurfaceType(UnityEngine.Object[] materials, int surfaceType)
		{
			foreach (Material m in materials)
			{
				if (surfaceType == 0) // Opaque
                {
                    m.renderQueue = (int) RenderQueue.Geometry;
                    m.SetOverrideTag("RenderType", "Opaque");
                    m.SetInt(PropertyInfo.SrcBlend, (int) UnityEngine.Rendering.BlendMode.One);
                    m.SetInt(PropertyInfo.DstBlend, (int) UnityEngine.Rendering.BlendMode.Zero);
                    m.DisableKeyword("_ALPHABLEND_ON");
                    m.DisableKeyword("_ALPHATEST_ON");
                }
                else if(surfaceType == 1) // Transparent
				{
					m.renderQueue = (int) RenderQueue.Transparent;
					m.SetOverrideTag("RenderType", "Transparent");
					m.SetInt(PropertyInfo.SrcBlend, (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
					m.SetInt(PropertyInfo.DstBlend, (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
					m.EnableKeyword("_ALPHABLEND_ON");
					m.DisableKeyword("_ALPHATEST_ON");
				}
				else if (surfaceType == 2) // Clip
				{
					m.renderQueue = (int) RenderQueue.AlphaTest;
					m.SetOverrideTag("RenderType", "TransparentCutout");
					m.SetInt(PropertyInfo.SrcBlend, (int)UnityEngine.Rendering.BlendMode.One);
					m.SetInt(PropertyInfo.DstBlend, (int)UnityEngine.Rendering.BlendMode.Zero);
					m.EnableKeyword("_ALPHATEST_ON");
					m.DisableKeyword("_ALPHABLEND_ON");
				}

				if (m.HasProperty(PropertyInfo.QueueOffset))
				{
					m.renderQueue += (int)m.GetFloat(PropertyInfo.QueueOffset);
				}
			}
		}


		/// <summary>
		/// make Drawer can get all current Material props by customShaderGUI
		/// Unity 2019.2+
		/// </summary>
		public static LWGUI GetLWGUI(MaterialEditor editor)
		{
			if (editor.customShaderGUI != null && editor.customShaderGUI is LWGUI)
			{
				LWGUI gui = editor.customShaderGUI as LWGUI;
				return gui;
			}
			else
			{
				Debug.LogWarning("Please add \"CustomEditor \"LWGUI.LWGUI\"\" to the end of your shader!");
				return null;
			}
		}

#endregion


#region Math
public static float PowPreserveSign(float f, float p)
		{
			float num = Mathf.Pow(Mathf.Abs(f), p);
			if ((double)f < 0.0)
				return -num;
			return num;
		}

#endregion


#region Reflection

		

#endregion

#region Draw GUI for Drawer

		// TODO: use Reflection
		// copy and edit of https://github.com/GucioDevs/SimpleMinMaxSlider/blob/master/Assets/SimpleMinMaxSlider/Scripts/Editor/MinMaxSliderDrawer.cs
		public static Rect[] SplitRect(Rect rectToSplit, int n)
		{
			Rect[] rects = new Rect[n];

			for (int i = 0; i < n; i++)
			{
				rects[i] = new Rect(rectToSplit.position.x + (i * rectToSplit.width / n), rectToSplit.position.y, rectToSplit.width / n, rectToSplit.height);
			}

			int padding = (int)rects[0].width - 50; // use 50, enough to show 0.xx (2 digits)
			int space = 5;

			rects[0].width -= padding + space;
			rects[2].width -= padding + space;

			rects[1].x -= padding;
			rects[1].width += padding * 2;

			rects[2].x += padding + space;

			return rects;
		}

        public static bool Foldout(Rect position, ref bool isFolding, bool toggleValue, bool hasToggle, GUIContent label)
        {
			var style = new GUIStyle("ShurikenModuleTitle");
			style.border = new RectOffset(15, 7, 4, 4);
			style.fixedHeight = 30;
			// Text
			style.font = new GUIStyle(EditorStyles.boldLabel).font;
			style.fontSize = (int)(style.fontSize * 1.5f);
			style.contentOffset = new Vector2(30f, -2f);

			var rect = position;//GUILayoutUtility.GetRect(position.width, 24f, style);

			GUI.backgroundColor = isFolding ? Color.white : new Color(0.85f, 0.85f, 0.85f);
			GUI.Box(rect, label, style);
			GUI.backgroundColor = Color.white;

            var toggleRect = new Rect(rect.x + 8f, rect.y + 7f, 13f, 13f);

			if (hasToggle)
			{
				EditorGUI.BeginChangeCheck();
				GUI.Toggle(toggleRect, EditorGUI.showMixedValue ? false : toggleValue, String.Empty, new GUIStyle(EditorGUI.showMixedValue ? "ToggleMixed" : "Toggle"));
				if (EditorGUI.EndChangeCheck())
					toggleValue = !toggleValue;
			}

			var e = Event.current;
            if (e.type == UnityEngine.EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                isFolding = !isFolding;
                e.Use();
            }
            return toggleValue;
        }

		// TODO: use Reflection
        public static void PowerSlider(MaterialProperty prop, float power, Rect position, GUIContent label)
        {
            int controlId = GUIUtility.GetControlID("EditorSliderKnob".GetHashCode(), FocusType.Passive, position);
            float left = prop.rangeLimits.x;
            float right = prop.rangeLimits.y;
            float start = left;
            float end = right;
            float value = prop.floatValue;
            float originValue = prop.floatValue;

            if ((double)power != 1.0)
            {
                start = Helper.PowPreserveSign(start, 1f / power);
                end = Helper.PowPreserveSign(end, 1f / power);
                value = Helper.PowPreserveSign(value, 1f / power);
            }

            EditorGUI.BeginChangeCheck();

            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 0;

            var rectAfterLabel = EditorGUI.PrefixLabel(position, label);
			
			Rect sliderRect = MaterialEditor.GetFlexibleRectBetweenLabelAndField(position);
			sliderRect.xMin += 2;
            if (sliderRect.width >= 50f)
				// TODO: Slider Focus
                value = GUI.Slider(sliderRect, value, 0.0f, start, end, GUI.skin.horizontalSlider, !EditorGUI.showMixedValue ? GUI.skin.horizontalSliderThumb : (GUIStyle)"SliderMixed", true, controlId);

            if ((double)power != 1.0)
                value = Helper.PowPreserveSign(value, power);

            position.xMin = Mathf.Max(rectAfterLabel.xMin, sliderRect.xMax - 10f);
			var floatRect = position;
            value = EditorGUI.FloatField(floatRect, value);

            if (value != originValue)
                prop.floatValue = Mathf.Clamp(value, Mathf.Min(left, right), Mathf.Max(left, right));
			
            EditorGUIUtility.labelWidth = labelWidth;
        }
#endregion


#region Draw GUI for Material


		private static Texture _logo = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("26b9d845eb7b1a747bf04dc84e5bcc2c"));
		public static void DrawLogo()
		{
			var logoRect = EditorGUILayout.GetControlRect(false, _logo.height);
			var w = logoRect.width;
			logoRect.xMin += w * 0.5f - _logo.width * 0.5f;
			logoRect.xMax -= w * 0.5f - _logo.width * 0.5f;

			if (EditorGUIUtility.currentViewWidth >= logoRect.width)
			{
				var c = GUI.color;
				GUI.color = new Color(c.r, c.g, c.b, 0.4f);
				if (logoRect.Contains(Event.current.mousePosition))
				{
					GUI.color = new Color(c.r, c.g, c.b, 0.8f);
					if (Event.current.type == UnityEngine.EventType.MouseDown)
						Application.OpenURL("https://github.com/JasonMa0012/LWGUI");
				}
				GUI.DrawTexture(logoRect, _logo);
				GUI.color = c;
				GUI.Label(logoRect, new GUIContent(String.Empty, "LWGUI (Light Weight Shader GUI)\n\n"
													 + "A Lightweight, Flexible, Powerful Unity Shader GUI system.\n\n"
													 + "Copyright (c) Jason Ma"));
			}
		}

		private static readonly int s_TextFieldHash = "EditorTextField".GetHashCode();
		private static readonly GUIContent[] _searchModeMenus = new[]
		{
			new GUIContent(SearchMode.All.ToString()),
			new GUIContent(SearchMode.Modified.ToString())
		};

		/// <returns>is has changed?</returns>
		public static bool DrawSearchField(ref string searchingText, ref SearchMode searchMode, LWGUI lwgui)
		{
			var toolbarSeachTextFieldPopup = new GUIStyle("ToolbarSeachTextFieldPopup");

			bool isHasChanged = false;
			EditorGUI.BeginChangeCheck();
			
			var rect = EditorGUILayout.GetControlRect();
			var revertButtonRect = RevertableHelper.GetRevertButtonRect(EditorGUIUtility.singleLineHeight, rect);
			rect.xMax -= RevertableHelper.revertButtonWidth;
			// Get internal TextField ControlID
			int controlId = GUIUtility.GetControlID(s_TextFieldHash, FocusType.Keyboard, rect) + 1;
			
			// searching mode
			Rect modeRect = new Rect(rect);
			modeRect.width = 20f;
			if (Event.current.type == UnityEngine.EventType.MouseDown && modeRect.Contains(Event.current.mousePosition))
			{
				EditorUtility.DisplayCustomMenu(rect, _searchModeMenus, (int)searchMode, 
												(data, options, selected) =>
												{
													if (lwgui.searchMode != (SearchMode)selected)
													{
														lwgui.searchMode = (SearchMode)selected;
														lwgui.updateSearchMode = true;
													}
												}, null);
				Event.current.Use();
			}
			
			// TODO: use Reflection -> controlId
			searchingText = EditorGUI.TextField(rect, String.Empty, searchingText, toolbarSeachTextFieldPopup);

			
			if (EditorGUI.EndChangeCheck())
				isHasChanged = true;
			
			// revert button
			if ((!string.IsNullOrEmpty(searchingText) || searchMode != SearchMode.All) && 
				RevertableHelper.DrawRevertButton(revertButtonRect))
			{
				searchingText = string.Empty;
				searchMode = SearchMode.All;
				isHasChanged = true;
				GUIUtility.keyboardControl = 0;
			}

			// display search mode
			if (GUIUtility.keyboardControl != controlId && string.IsNullOrEmpty(searchingText) && Event.current.type == UnityEngine.EventType.Repaint)
			{
				using (new EditorGUI.DisabledScope(true))
				{
					Rect position1 = toolbarSeachTextFieldPopup.padding.Remove(new Rect(rect.x, rect.y, rect.width, toolbarSeachTextFieldPopup.fixedHeight > 0.0 ? toolbarSeachTextFieldPopup.fixedHeight : rect.height));
					int fontSize = EditorStyles.label.fontSize;
					EditorStyles.label.fontSize = toolbarSeachTextFieldPopup.fontSize;
					EditorStyles.label.Draw(position1, new GUIContent(searchMode.ToString()), false, false, false, false);
					EditorStyles.label.fontSize = fontSize;
				}
			}
			
			return isHasChanged;
		}
#endregion
	}


	internal class RampHelper
	{
		
		[Serializable]
		public class GradientObject : ScriptableObject
		{
			[SerializeField] public Gradient gradient = new Gradient();
		}

		private static readonly GUIContent _iconAdd     = new GUIContent(EditorGUIUtility.IconContent("d_Toolbar Plus").image, "Add"),
										   _iconEdit    = new GUIContent(EditorGUIUtility.IconContent("editicon.sml").image, "Edit"),
										   _iconDiscard = new GUIContent(EditorGUIUtility.IconContent("d_TreeEditor.Refresh").image, "Discard"),
										   _iconSave    = new GUIContent(EditorGUIUtility.IconContent("SaveActive").image, "Save");

		private static readonly GUIStyle _styleEdit = new GUIStyle("button");

		public static bool RampEditor(
			MaterialProperty   prop,
			Rect               buttonRect,
			SerializedProperty gradientProperty,
			bool               isDirty,
			string             defaultFileName,
			int                defaultWidth,
			int                defaultHeight,
			out Texture        newTexture,
			out bool           doSave,
			out bool           doDiscard)
		{
			newTexture = null;
			var hasChange = false;
			var shouldCreate = false;
			var singleButtonWidth = buttonRect.width * 0.25f;
			var editRect = new Rect(buttonRect.x + singleButtonWidth * 0, buttonRect.y, singleButtonWidth, buttonRect.height);
			var saveRect = new Rect(buttonRect.x + singleButtonWidth * 1, buttonRect.y, singleButtonWidth, buttonRect.height);
			var addRect = new Rect(buttonRect.x + singleButtonWidth * 2, buttonRect.y, singleButtonWidth, buttonRect.height);
			var discardRect = new Rect(buttonRect.x + singleButtonWidth * 3, buttonRect.y, singleButtonWidth, buttonRect.height);

			// if the current edited texture is null, create new one
			var currEvent = Event.current;
			if (prop.textureValue == null && currEvent.type == UnityEngine.EventType.MouseDown && editRect.Contains(currEvent.mousePosition))
			{
				shouldCreate = true;
				currEvent.Use();
			}
			
			// Gradient Editor
			var gradientPropertyRect = new Rect(editRect.x + 2, editRect.y + 2, editRect.width - 2, editRect.height - 2);
			EditorGUI.BeginChangeCheck();
			EditorGUI.PropertyField(gradientPropertyRect, gradientProperty, GUIContent.none);
			if (EditorGUI.EndChangeCheck()) hasChange = true;
			
			// Edit Icon override
			if (currEvent.type == UnityEngine.EventType.Repaint)
			{
				var isHover = editRect.Contains(currEvent.mousePosition);
				_styleEdit.Draw(editRect, _iconEdit, isHover, false, false, false);
			}
				
			// Create Ramp Texture
            if (GUI.Button(addRect, _iconAdd) || shouldCreate)
            {
                var path = EditorUtility.SaveFilePanel("Create New Ramp Texture", lastSavePath, defaultFileName, "png");
                if (path.Contains(projectPath))
                {
                    lastSavePath = Path.GetDirectoryName(path);

                    //Create texture and save PNG
                    var saveUnityPath = path.Replace(projectPath, String.Empty);
                    CreateAndSaveNewGradientTexture(defaultWidth, defaultHeight, saveUnityPath);

                    //Load created texture
					newTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(saveUnityPath);
				}
				else if(!string.IsNullOrEmpty(path))
				{
					Debug.LogError("Invalid Path: "+path+"\n"
								 + "Please make sure you chosen Unity Project Relative Path");
				}
            }

			var color = GUI.color;
			if (isDirty) GUI.color = Color.yellow;
			doSave = GUI.Button(saveRect, _iconSave);
			GUI.color = color;
			
			doDiscard = GUI.Button(discardRect, _iconDiscard);
			
			return hasChange;
		}
		
		
		public static readonly string projectPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
		public static string lastSavePath
		{
			get { return EditorPrefs.GetString("LWGUI_GradientSavePath_" + Application.version, Application.dataPath); }
			set
			{
				if (value.Contains(projectPath))
					EditorPrefs.SetString("LWGUI_GradientSavePath_" + Application.version, value);
			}
		}

		
		public static Gradient GetGradientFromTexture(Texture texture, out bool isDirty, bool doReimporte = false)
		{
			isDirty = false;
			if (texture == null) return null;
			
			var assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));
			if (assetImporter != null && assetImporter.userData.Contains("LWGUI"))
			{
				GradientObject savedGradientObject, editingGradientObject;
				isDirty = DecodeGradientFromJSON(assetImporter.userData, out savedGradientObject, out editingGradientObject);
				return doReimporte ? savedGradientObject.gradient : editingGradientObject.gradient;
			}
			else
			{
				Debug.LogError("Can not find texture: "+texture.name+" or it's userData on disk! \n"
							 + "If you are moving or copying the Ramp Map, make sure your .meta file is not lost!");
				return null;
			}
		}

		public static void SetGradientToTexture(Texture texture, GradientObject gradientObject, bool doSaveToDisk = false)
		{
			if (texture == null || gradientObject.gradient == null) return;

			var texture2D = (Texture2D)texture;
			// Save to texture
			var path = AssetDatabase.GetAssetPath(texture);
			var pixels = GetPixelsFromGradient(gradientObject.gradient, texture.width, texture.height);
			texture2D.SetPixels(pixels);
			texture2D.Apply(true, false);
			
			// Save gradient JSON to userData
			var assetImporter = AssetImporter.GetAtPath(path);
			GradientObject savedGradientObject, editingGradientObject;
			DecodeGradientFromJSON(assetImporter.userData, out savedGradientObject, out editingGradientObject);
			assetImporter.userData = EncodeGradientToJSON(doSaveToDisk ? gradientObject : savedGradientObject, gradientObject);
				
			// Save texture to disk
			if (doSaveToDisk)
			{
				var systemPath = projectPath + path;
				File.WriteAllBytes(systemPath, texture2D.EncodeToPNG());
				assetImporter.SaveAndReimport();
			}
		}

		private static string EncodeGradientToJSON(GradientObject savedGradientObject, GradientObject editingGradientObject)
		{
			string savedJSON = " ", editingJSON = " ";
			if (savedGradientObject != null)
				savedJSON = EditorJsonUtility.ToJson(savedGradientObject);
			if (editingGradientObject != null)
				editingJSON = EditorJsonUtility.ToJson(editingGradientObject);

			return savedJSON + "#" + editingJSON;
		}

		private static bool DecodeGradientFromJSON(string json, out GradientObject savedGradientObject, out GradientObject editingGradientObject)
		{
			var subJSONs = json.Split('#');
			savedGradientObject = ScriptableObject.CreateInstance<GradientObject>();
			if (subJSONs[0] != " ")
				EditorJsonUtility.FromJsonOverwrite(subJSONs[0], savedGradientObject);
			editingGradientObject = ScriptableObject.CreateInstance<GradientObject>();
			if (subJSONs[1] != " ")
				EditorJsonUtility.FromJsonOverwrite(subJSONs[1], editingGradientObject);
			return subJSONs[0] != subJSONs[1];
		}
		

		public static bool CreateAndSaveNewGradientTexture(int width, int height, string unityPath)
		{
			var gradientObject = ScriptableObject.CreateInstance<GradientObject>();
			gradientObject.gradient = new Gradient();
			gradientObject.gradient.colorKeys = new[] { new GradientColorKey(Color.gray, 0.0f), new GradientColorKey(Color.white, 1.0f) };
			gradientObject.gradient.alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };

			var ramp = CreateGradientTexture(gradientObject.gradient, width, height);
			var png = ramp.EncodeToPNG();
			Object.DestroyImmediate(ramp);

			var systemPath = projectPath + unityPath;
			File.WriteAllBytes(systemPath, png);

			AssetDatabase.ImportAsset(unityPath);
			var textureImporter = AssetImporter.GetAtPath(unityPath) as TextureImporter;
			textureImporter.wrapMode = TextureWrapMode.Clamp;
			textureImporter.isReadable = true;
			textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
			textureImporter.alphaSource = TextureImporterAlphaSource.FromInput;
			
			//Gradient data embedded in userData
			textureImporter.userData = EncodeGradientToJSON(gradientObject, gradientObject);
			textureImporter.SaveAndReimport();

			return true;
		}
		
		private static Texture2D CreateGradientTexture(Gradient gradient, int width, int height)
		{
			var ramp = new Texture2D(width, height, TextureFormat.RGBA32, true, true);
			var colors = GetPixelsFromGradient(gradient, width, height);
			ramp.SetPixels(colors);
			ramp.Apply(true);
			return ramp;
		}

		private static Color[] GetPixelsFromGradient(Gradient gradient, int width, int height)
		{
			var pixels = new Color[width * height];
			for (var x = 0; x < width; x++)
			{
				var delta = x / (float)width;
				if (delta < 0) delta = 0;
				if (delta > 1) delta = 1;
				var col = gradient.Evaluate(delta);
				for (int i = 0; i < height; i++)
				{
					pixels[x + i * width] = col;
				}
			}
			return pixels;
		}
	}


	/// <summary>
	/// Provide Metadata for drawing
	/// </summary>
	internal class MetaDataHelper
	{
		private static Dictionary<Shader, Dictionary<string /*MainProp*/,	List<string /*SubProp*/>>>		_mainSubDic       = new Dictionary<Shader, Dictionary<string, List<string>>>();
		private static Dictionary<Shader, Dictionary<string /*GroupName*/,	     string /*MainProp*/>>		_mainGroupNameDic = new Dictionary<Shader, Dictionary<string, string>>();
		private static Dictionary<Shader, Dictionary<string /*PropName*/,	     string /*GroupName*/>>		_propParentDic	  = new Dictionary<Shader, Dictionary<string, string>>();
		
		private static Dictionary<Shader, Dictionary<string /*PropName*/,	List<string /*ExtraPropName*/>>>_extraPropDic     = new Dictionary<Shader, Dictionary<string, List<string>>>();
		private static Dictionary<Shader, Dictionary<string /*PropName*/,	List<string /*Tooltip*/>>>		_tooltipDic       = new Dictionary<Shader, Dictionary<string, List<string>>>();
		private static Dictionary<Shader, Dictionary<string /*PropName*/,	List<string /*DefaultValue*/>>>	_defaultDic       = new Dictionary<Shader, Dictionary<string, List<string>>>();
		private static Dictionary<Shader, Dictionary<string /*PropName*/,	List<string /*Helpbox*/>>>		_helpboxDic       = new Dictionary<Shader, Dictionary<string, List<string>>>();

		public static void ClearCaches(Shader shader)
		{
			if (_mainSubDic.ContainsKey(shader)) _mainSubDic[shader].Clear();
			if (_mainGroupNameDic.ContainsKey(shader)) _mainGroupNameDic[shader].Clear();
			if (_propParentDic.ContainsKey(shader)) _propParentDic[shader].Clear();
			
			if (_extraPropDic.ContainsKey(shader)) _extraPropDic[shader].Clear();
			if (_tooltipDic.ContainsKey(shader)) _tooltipDic[shader].Clear();
			if (_defaultDic.ContainsKey(shader)) _defaultDic[shader].Clear();
			if (_helpboxDic.ContainsKey(shader)) _helpboxDic[shader].Clear();
		}


		#region Main - Sub

		public static void RegisterMainProp(Shader shader, MaterialProperty prop, string group)
		{
			if (_mainSubDic.ContainsKey(shader))
			{
				if (!_mainSubDic[shader].ContainsKey(prop.name))
				{
					_mainSubDic[shader].Add(prop.name, new List<string>());
				}
			}
			else
			{
				_mainSubDic.Add(shader, new Dictionary<string, List<string>>());
				_mainSubDic[shader].Add(prop.name, new List<string>());
			}

			if (_mainGroupNameDic.ContainsKey(shader))
			{
				if (!_mainGroupNameDic[shader].ContainsKey(group))
				{
					_mainGroupNameDic[shader].Add(group, prop.name);
				}
			}
			else
			{
				_mainGroupNameDic.Add(shader, new Dictionary<string, string>());
				_mainGroupNameDic[shader].Add(group, prop.name);
			}
		}

		public static void RegisterSubProp(Shader shader, MaterialProperty prop, string group, MaterialProperty[] extraProps = null)
		{
			if (!string.IsNullOrEmpty(group) && group != "_")
			{
				// add to _mainSubDic
				if (_mainGroupNameDic.ContainsKey(shader))
				{
					var groupName = _mainGroupNameDic[shader].Keys.First((s => group.Contains(s)));
					if (!string.IsNullOrEmpty(groupName))
					{
						var mainPropName = _mainGroupNameDic[shader][groupName];
						if (_mainSubDic[shader].ContainsKey(mainPropName))
						{
							if (!_mainSubDic[shader][mainPropName].Contains(prop.name))
								_mainSubDic[shader][mainPropName].Add(prop.name);
						}
						else
							Debug.LogError($"Unregistered Main Property:{mainPropName}");

						// add to _propParentDic
						if (!_propParentDic.ContainsKey(shader))
							_propParentDic.Add(shader, new Dictionary<string, string>());
						if (!_propParentDic[shader].ContainsKey(prop.name))
							_propParentDic[shader].Add(prop.name, groupName);
					}
					else
						Debug.LogError($"Unregistered Main Group Name:{group}");
				}
				else
					Debug.LogError($"Unregistered Shader:{shader.name}");
			}
			
			// add to _extraPropDic
			if (extraProps != null)
			{
				if (!_extraPropDic.ContainsKey(shader))
					_extraPropDic.Add(shader, new Dictionary<string, List<string>>());
				if (!_extraPropDic[shader].ContainsKey(prop.name))
					_extraPropDic[shader].Add(prop.name, new List<string>());
				foreach (var extraProp in extraProps)
				{
					if (extraProp != null)
					{
						if (!_extraPropDic[shader][prop.name].Contains(extraProp.name))
							_extraPropDic[shader][prop.name].Add(extraProp.name);
					}
				}
			}
		}
		#endregion


		#region Tooltip / Helpbox

		private static void RegisterPropertyString(Shader shader, MaterialProperty prop, string str, Dictionary<Shader, Dictionary<string, List<string>>> dst)
		{
			if (!dst.ContainsKey(shader))
				dst.Add(shader, new Dictionary<string, List<string>>());
			if (!dst[shader].ContainsKey(prop.name))
				dst[shader].Add(prop.name, new List<string>());
			dst[shader][prop.name].Add(str); 
		}

		private static string GetPropertyString(Shader shader, MaterialProperty prop, Dictionary<Shader, Dictionary<string, List<string>>> src, out int lineCount)
		{
			var str = string.Empty;
			lineCount = 0;
			if (src.ContainsKey(shader) && src[shader].ContainsKey(prop.name))
			{
				foreach (var tooltip in src[shader][prop.name])
				{
					str += tooltip + "\n";
					lineCount++;
				}
			}
			return str;
		}
		
		public static void RegisterPropertyDefaultValueText(Shader shader, MaterialProperty prop, string text)
		{
			RegisterPropertyString(shader, prop, text, _defaultDic);
		}

		public static void RegisterPropertyTooltip(Shader shader, MaterialProperty prop, string tooltip)
		{
			RegisterPropertyString(shader, prop, tooltip, _tooltipDic);
		}

		private static string GetPropertyDefaultValueText(Shader shader, MaterialProperty prop)
		{
			var defaultText = GetPropertyString(shader, prop, _defaultDic, out _);
			if (string.IsNullOrEmpty(defaultText))
				// TODO: use Reflection - handle builtin Toggle / Enum
				defaultText = RevertableHelper.GetPropertyDefaultValueText(shader, prop);
			else
				defaultText = defaultText.Remove(defaultText.Length - 1);

			return defaultText;
		}
		
		public static string GetPropertyTooltip(Shader shader, MaterialProperty prop)
		{
			var str = GetPropertyString(shader, prop, _tooltipDic, out _);
			if (!string.IsNullOrEmpty(str))
				str += "\n";
			str += $"Name: {prop.name}\n";
			str += $"Default: " + GetPropertyDefaultValueText(shader, prop);
			return str;
		}
		
		public static void RegisterPropertyHelpbox(Shader shader, MaterialProperty prop, string tooltip)
		{
			RegisterPropertyString(shader, prop, tooltip, _helpboxDic);
		}

		public static string GetPropertyHelpbox(Shader shader, MaterialProperty prop, out int lineCount)
		{
			var str = GetPropertyString(shader, prop, _helpboxDic, out lineCount);
			return str;
		}
		#endregion

		
		public static Dictionary<string, bool> SearchProperties(Shader shader, MaterialProperty[] props, string searchingText, SearchMode searchMode)
		{
			var result = new Dictionary<string, bool>();
			var isDefaultProps = new Dictionary<string, bool>();

			if (searchMode == SearchMode.Modified)
			{
				foreach (var prop in props)
				{
					isDefaultProps.Add(prop.name, RevertableHelper.IsDefaultProperty(shader, prop));
				}
			}
			
			if (string.IsNullOrEmpty(searchingText) && searchMode == SearchMode.All)
			{
				foreach (var prop in props)
				{
					result.Add(prop.name, true);
				}
			}
			else
			{
				foreach (var prop in props)
				{
					bool contains = true;
					
					// filter props
					if (searchMode == SearchMode.Modified)
					{
						contains = !isDefaultProps[prop.name];
						if (!contains && _extraPropDic.ContainsKey(shader) && _extraPropDic[shader].ContainsKey(prop.name))
						{
							foreach (var extraPropName in _extraPropDic[shader][prop.name])
							{
								contains = !isDefaultProps[extraPropName];
								if (contains) break;
							}
						}
					}
					
					// whole word match search
					var displayName = prop.displayName.ToLower();
					var name = prop.name.ToLower();
					searchingText = searchingText.ToLower();

					var keywords = searchingText.Split(' ', ',', ';', '|', '*', '&');// Some possible separators

					foreach (var keyword in keywords)
					{
						var isMatch = false;
						isMatch |= displayName.Contains(keyword);
						isMatch |= name.Contains(keyword);
						contains &= isMatch;
					}
					
					result.Add(prop.name, contains);
				}

				// when a SubProp display, MainProp will also display
				if (_mainSubDic.ContainsKey(shader))
				{
					foreach (var prop in props)
					{
						if (_mainSubDic[shader].ContainsKey(prop.name))
						{
							// foreach sub prop in main
							foreach (var subPropName in _mainSubDic[shader][prop.name])
							{
								if (result.ContainsKey(subPropName))
								{
									if (result[subPropName])
									{
										result[prop.name] = true;
										break;
									}
								}
							}
						}
					}
				}
			}

			return result;
		}

		public static bool IsSubProperty(Shader shader, MaterialProperty prop)
		{
			var isSubProp = false;
			if (_propParentDic.ContainsKey(shader) && _propParentDic[shader].ContainsKey(prop.name))
				isSubProp = true;
			return isSubProp;
		}
		
	}

} //namespace LWGUI

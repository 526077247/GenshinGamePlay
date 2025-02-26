using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace TaoTie
{
    public class TextMeshFontAssetManager
    {
        public static TextMeshFontAssetManager Instance { get; } = new TextMeshFontAssetManager();
        private Dictionary<string, TMP_FontAsset> addFontWithPathList = new Dictionary<string, TMP_FontAsset>();

        private void AddFontAsset(ScriptableObject fontAsset)
        {
            if (CheckFontAsset(fontAsset)) return;
            var def = TMP_Settings.defaultFontAsset;
            def.fallbackFontAssetTable.Add(fontAsset as TMP_FontAsset);
        }

        private void RemoveFontAsset(ScriptableObject fontAsset)
        {
            if (!CheckFontAsset(fontAsset)) return;
            var def = TMP_Settings.defaultFontAsset;
            def.fallbackFontAssetTable.Remove(fontAsset as TMP_FontAsset);
            GameObject.Destroy((fontAsset as TMP_FontAsset).sourceFontFile);
            GameObject.Destroy(fontAsset);
        }

        private bool CheckFontAsset(ScriptableObject fontAsset)
        {
            TMP_FontAsset font = fontAsset as TMP_FontAsset;
            var def = TMP_Settings.defaultFontAsset;
            return def.fallbackFontAssetTable.Contains(font);
        }

        public void AddWithOSFont(string[] tb)
        {
            using (DictionaryComponent<string, string> fontPaths = DictionaryComponent<string, string>.Create())
            {
                var tempPaths = Font.GetPathsToOSFonts();
                if (tempPaths == null)
                    return;

                foreach (string path in tempPaths)
                {
                    string key = Path.GetFileNameWithoutExtension(path).ToLower();
                    if (!fontPaths.ContainsKey(key))
                        fontPaths.Add(key, path);
                }

                for (int i = 0; i < tb.Length; i++)
                {
                    string fontName = tb[i].ToLower();
                    if (fontPaths.TryGetValue(fontName, out var path))
                    {
                        Log.Info($"添加字体: {fontName} {path}");
                        AddFontAssetByFontPath(path);
                    }
                }
            }
        }
        
        public void RemoveWithOSFont(string[] tb)
        {
            using (DictionaryComponent<string, string> fontPaths = DictionaryComponent<string, string>.Create())
            {
                var tempPaths = Font.GetPathsToOSFonts();
                if (tempPaths == null)
                    return;

                foreach (string path in tempPaths)
                {
                    string key = Path.GetFileNameWithoutExtension(path).ToLower();
                    fontPaths.TryAdd(key, path);
                }

                for (int i = 0; i < tb.Length; i++)
                {
                    string fontName = tb[i].ToLower();
                    if (fontPaths.TryGetValue(fontName, out var path))
                    {
                        Log.Info($"移除字体: {fontName} {path}");
                        RemoveFontAssetByFontPath(path);
                    }
                }
            }
        }

        /// <summary>
        /// 可以从网上下载字体或获取到本地自带字体
        /// </summary>
        /// <param name="fontPath"></param>
        public void AddFontAssetByFontPath(string fontPath)
        {
            if (addFontWithPathList.ContainsKey(fontPath))
                return;

            Font font = new Font(fontPath);
            TMP_FontAsset tp_font = TMP_FontAsset.CreateFontAsset(font, 20, 2, GlyphRenderMode.SDFAA, 512, 512);
            AddFontAsset(tp_font);
            addFontWithPathList.Add(fontPath, tp_font);
            if (TMP_Settings.defaultFontAsset != null)
            {
                TMP_Settings.defaultFontAsset.fallbackFontAssetTable.Add(tp_font);
            }
        }

        public void RemoveFontAssetByFontPath(string fontPath)
        {
            if (!addFontWithPathList.ContainsKey(fontPath))
                return;

            TMP_FontAsset tpFont = addFontWithPathList[fontPath];
            if (TMP_Settings.defaultFontAsset != null)
            {
                TMP_Settings.defaultFontAsset.fallbackFontAssetTable.Remove(tpFont);
            }
            RemoveFontAsset(tpFont);
            addFontWithPathList.Remove(fontPath);
        }
    }
}
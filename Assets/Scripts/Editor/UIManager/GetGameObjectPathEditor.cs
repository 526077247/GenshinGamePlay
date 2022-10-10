using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace TaoTie
{
    public class GetGameObjectPathEditor
    {
        /// <summary>
        /// 将信息复制到剪切板当中
        /// </summary>
        public static void Copy(string result) {
            TextEditor editor = new TextEditor();
            editor.text = new GUIContent(result).text;
            editor.OnFocus();
            editor.Copy();
        }

        [MenuItem("GameObject/复制物体路径", false, 24)]
        static void CopyGameObjectPath() {
            Object obj = Selection.activeObject;
            if (obj == null) {
                Debug.LogError("请先选择一个物体");
                return;
            }

            Transform selectChild = Selection.activeTransform;
            if (selectChild != null) {
                string result = selectChild.name;
                while (selectChild.parent != null && selectChild.parent.parent.parent != null) {
                    selectChild = selectChild.parent;
                    result = string.Format("{0}/{1}", selectChild.name, result);
                }

                Copy(result);
            }
            Debug.Log(string.Format("物体:{0}的路径已经复制到剪切板!", obj.name));
        }
    }
}
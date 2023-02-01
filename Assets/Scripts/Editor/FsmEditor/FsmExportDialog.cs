using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.IO;
using System;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.Collections.Generic;

namespace TaoTie
{
    public class UtilityEditor
    {
        public static readonly string EditDirName = "Edit";
        public static readonly string PublishDirName = "Publish";
    }
    public class FsmExportDialog : OdinEditorWindow
    {
        [MenuItem("Tools/Fsm/状态机导出")]
        private static void OpenWindow()
        {
            GetWindow<FsmExportDialog>().Show();
        }


        #region 导出状态机
        
        [BoxGroup("导出状态机")]
        [LabelText("原始状态机")]
        public AnimatorController controller;
        
        [BoxGroup("导出状态机")]
        [LabelText("原始AI状态机")]
        public AnimatorController aicontroller;

        private const string _controllerConfigName = "FsmConfig.bytes";
        private const string _aicontrollerConfigName = "PoseConfig.bytes";
        [BoxGroup("导出状态机")]
        [Button("导出状态机")]
        private void Export()
        {
            if (controller != null)
            {
                var path = AssetDatabase.GetAssetPath(controller);
                var index = path.LastIndexOf('/');
                var dir = path.Substring(0,index);
                ExportController(controller, _controllerConfigName);
                if (aicontroller != null)
                {
                    ExportController(aicontroller, _aicontrollerConfigName);
                }
            }
        }

        private void ExportController(AnimatorController controller, string name)
        {
            if (controller == null)
            {
                Debug.LogError("AnimatorConroller 不能为空");
                return;
            }

            string controllerPath = AssetDatabase.GetAssetPath(controller);
            string editDir = Path.GetDirectoryName(controllerPath);
            if (Path.GetFileName(editDir) != UtilityEditor.EditDirName)
            {
                Debug.LogError("AnimatorController 不在编辑目录");
                return;
            }
            
            
            string actionPath = editDir.Replace(UtilityEditor.EditDirName, UtilityEditor.PublishDirName);
            FsmExporter exporter = new FsmExporter(controller, actionPath);
            exporter.Generate(name);
        }

        #endregion
    }
}
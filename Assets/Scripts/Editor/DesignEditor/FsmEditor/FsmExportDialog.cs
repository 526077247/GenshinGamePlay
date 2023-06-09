using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace TaoTie
{
    public class FsmExportDialog : OdinEditorWindow
    {
        const string EditDirName = "Edit";
        const string ControllerConfigName = "FsmConfig.json";
        const string AIControllerConfigName = "PoseConfig.json";

        [MenuItem("Tools/Fsm/状态机导出")]
        private static void OpenWindow()
        {
            GetWindow<FsmExportDialog>().Show();
        }

        #region 导出状态机

        [LabelText("原始状态机")] public AnimatorController Controller;

        [LabelText("原始AI状态机")] public AnimatorController AIController;

        [Button("导出状态机")]
        private void Export()
        {
            if (Controller != null)
            {
                ExportController(Controller, ControllerConfigName);
                if (AIController != null)
                {
                    ExportController(AIController, AIControllerConfigName, false);
                }
            }
        }

        private void ExportController(AnimatorController controller, string name, bool publish = true)
        {
            if (controller == null)
            {
                Debug.LogError("AnimatorController 不能为空");
                return;
            }

            string controllerPath = AssetDatabase.GetAssetPath(controller);
            string editDir = Path.GetDirectoryName(controllerPath);
            if (Path.GetFileName(editDir) != EditDirName)
            {
                Debug.LogError("AnimatorController 不在编辑目录");
                return;
            }
            
            FsmExporter exporter = new FsmExporter(controller, editDir, publish);
            exporter.Generate(name);
        }

        #endregion
    }
}
using UnityEditor;
using UnityEditor.SceneManagement;

namespace TaoTie
{
    public class GameTools
    {
        [MenuItem("Tools/帮助/启动场景 #_b")]
        static void ChangeInitScene()
        {
            EditorSceneManager.OpenScene("Assets/AssetsPackage/Scenes/InitScene/Init.unity");
        }
    }
}
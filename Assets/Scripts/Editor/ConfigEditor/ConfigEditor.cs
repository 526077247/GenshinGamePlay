using UnityEditor;

namespace TaoTie
{
    public static class ConfigEditor
    {
        [MenuItem("Tools/配置编辑器/Gear")]
        static void OpenGear()
        {
            EditorWindow.GetWindow<GearEditor>().Show();
        }
        
        [MenuItem("Tools/配置编辑器/Ability")]
        static void OpenAbility()
        {
            EditorWindow.GetWindow<AbilityEditor>().Show();
        }
        [MenuItem("Tools/配置编辑器/AI")]
        static void OpenAI()
        {
            EditorWindow.GetWindow<AIEditor>().Show();
        }
        [MenuItem("Tools/配置编辑器/Entity")]
        static void OpenEntity()
        {
            EditorWindow.GetWindow<EntityEditor>().Show();
        }
    }
}
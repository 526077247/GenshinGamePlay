using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Unity.Code.NinoGen;
namespace TaoTie
{
    public class StoryEditor:BaseEditorWindow<ConfigStory>
    {
        protected override string folderPath => base.folderPath + "/EditConfig/Story";
        protected override byte[] Serialize(ConfigStory data)
        {
            return Serializer.Serialize(data);
        }
        [MenuItem("Tools/配置编辑器/Story")]
        static void OpenSceneGroup()
        {
            EditorWindow.GetWindow<StoryEditor>().Show();
        }
        [OnOpenAsset(0)]
        public static bool OnBaseDataOpened(int instanceID, int line)
        {
            var data = EditorUtility.InstanceIDToObject(instanceID) as TextAsset;
            var path = AssetDatabase.GetAssetPath(data);
            return InitializeData(data,path);
        }

        public static bool InitializeData(TextAsset asset,string path)
        {
            if (asset == null) return false;
            if (path.EndsWith(".json") && JsonHelper.TryFromJson<ConfigStory>(asset.text,out var json))
            {
                var win = EditorWindow.GetWindow<StoryEditor>();
                win.Init(json,path,true);
                return true;
            }
            return false;
        }

        protected override void BeforeSaveData()
        {
            base.BeforeSaveData();
            ProcessStoryClip(data.Clips);
        }

        private void ProcessStoryClip(ConfigStoryClip clip)
        {
            if (clip is ConfigStoryTimeLine timeLine)
            {
                TimelineSerializer.GetStoryFromTimeline(timeLine);
            }
            else if (clip is ConfigStoryParallelClip parallel)
            {
                for (int i = 0; i < parallel.Clips.Length; i++)
                {
                    ProcessStoryClip(parallel.Clips[i]);
                }
            }
            else if (clip is ConfigStoryBranchClip branch)
            {
                for (int i = 0; i < branch.Branchs.Length; i++)
                {
                    ProcessStoryClip(branch.Branchs[i].Clip);
                }
            }
            else if (clip is ConfigStorySerialClip serial)
            {
                for (int i = 0; i < serial.Clips.Length; i++)
                {
                    ProcessStoryClip(serial.Clips[i]);
                }
            }
        }
    }
}
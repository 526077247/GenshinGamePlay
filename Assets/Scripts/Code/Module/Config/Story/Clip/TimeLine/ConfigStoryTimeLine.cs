using System.Collections.Generic;
using TaoTie.LitJson.Extensions;
using ProtoBuf;
using UnityEngine.Timeline;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigStoryTimeLine: ConfigStoryClip
    {
        
#if UNITY_EDITOR
        
        [Sirenix.OdinInspector.OnValueChanged(nameof(SetPath))][Sirenix.OdinInspector.BoxGroup("TimeLine")]
        public TimelineAsset TimeLine;
        
        public void SetPath()
        {
            if (TimeLine == null) Path = null;
            var path = UnityEditor.AssetDatabase.GetAssetPath(TimeLine);
            if (path.StartsWith("Assets/AssetsPackage/"))
            {
                Path = path.Replace("Assets/AssetsPackage/","");
            }
            else
            {
                Path = null;
            }
        }
        [Sirenix.OdinInspector.BoxGroup("TimeLine")][Sirenix.OdinInspector.Button("预览TimeLine")]
        public void Preview()
        {
            if (string.IsNullOrEmpty(Path)) return;
            if (!Path.StartsWith("Assets/AssetsPackage/")) 
                TimeLine = UnityEditor.AssetDatabase.LoadAssetAtPath<TimelineAsset>("Assets/AssetsPackage/" +Path);
            else
                TimeLine = UnityEditor.AssetDatabase.LoadAssetAtPath<TimelineAsset>(Path);
        }
        [Sirenix.OdinInspector.BoxGroup("TimeLine")]
#endif
        [ProtoMember(10)]
        public string Path;
        [ProtoMember(11)]
        public ConfigStoryTimeLineClip[] Clips;
        [ProtoMember(12, IsRequired = true)]
        public Dictionary<string, int> Binding = new Dictionary<string, int>();
        public override async ETTask Process(StorySystem storySystem)
        {
            await storySystem.PlayTimeLine(this);
        }
    }
}
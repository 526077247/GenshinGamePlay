using System.Collections.Generic;
using LitJson.Extensions;
using Nino.Serialization;
using UnityEngine.Timeline;

namespace TaoTie
{
    public partial class ConfigStoryTimeLine: ConfigStoryClip
    {
        
#if UNITY_EDITOR
        
        [JsonIgnore][Sirenix.OdinInspector.OnValueChanged(nameof(SetPath))][Sirenix.OdinInspector.BoxGroup("TimeLine")]
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
            TimeLine = UnityEditor.AssetDatabase.LoadAssetAtPath<TimelineAsset>("Assets/AssetsPackage/" +Path);
        }
        [Sirenix.OdinInspector.BoxGroup("TimeLine")][Sirenix.OdinInspector.ReadOnly]
#endif
        [NinoMember(10)]
        public string Path;
        [NinoMember(11)][Sirenix.OdinInspector.ReadOnly]
        public ConfigStoryTimeLineClip[] Clips;
        [NinoMember(12)]
        public Dictionary<string, int> Binding = new Dictionary<string, int>();
        public override async ETTask Process(StorySystem storySystem)
        {
            await storySystem.PlayTimeLine(this);
        }
    }
}
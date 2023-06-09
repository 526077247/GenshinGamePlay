using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine.Timeline;

namespace TaoTie
{
    public static class TimelineSerializer
    {
        public static ConfigFsmTimeline GetFromTimeline(string path)
        {
            var a = AssetDatabase.LoadAssetAtPath<TimelineAsset>(path);
            if (a == null) return null;
            List<ConfigFsmClip> clips = new List<ConfigFsmClip>();
            for (int i = 0; i < a.rootTrackCount; i++)
            {
                var track = a.GetRootTrack(i);
                for (int j = 0; j < track.GetMarkerCount(); j++)
                {
                    var marker = track.GetMarker(j);
                    if (marker is ISerializableClip signal)
                    {
                        signal.DoSerialize(clips);
                    }
                }
            }
            ConfigFsmTimeline data = new ConfigFsmTimeline();
            data.Clips = clips.ToArray();
            data.Clips.Sort((a, b) =>
            {
                if (a.StartTime == b.StartTime) return 0;
                return a.StartTime - b.StartTime > 0 ? 1 : -1;
            });
            return data;
        }
    }
}
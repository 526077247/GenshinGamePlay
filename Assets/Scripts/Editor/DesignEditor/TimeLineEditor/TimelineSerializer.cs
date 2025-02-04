using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine.Timeline;

namespace TaoTie
{
    public static class TimelineSerializer
    {
        public static ConfigFsmTimeline GetFsmFromTimeline(string path)
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
                    if (marker is IFsmSerializable signal)
                    {
                        signal.DoSerialize(clips);
                    }
                }

                if (track is IFsmSerializable serializableTrack)
                {
                    serializableTrack.DoSerialize(clips);
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
        
        public static void GetStoryFromTimeline(ConfigStoryTimeLine config)
        {
            var a = AssetDatabase.LoadAssetAtPath<TimelineAsset>("Assets/AssetsPackage/"+config.Path);
            if (a == null) return ;
            List<ConfigStoryTimeLineClip> clips = new List<ConfigStoryTimeLineClip>();
            if(config.Binding==null)
                config.Binding = new Dictionary<string, int>();
            for (int i = 0; i < a.rootTrackCount; i++)
            {
                var track = a.GetRootTrack(i);
                for (int j = 0; j < track.GetMarkerCount(); j++)
                {
                    var marker = track.GetMarker(j);
                    if (marker is IStorySerializable signal)
                    {
                        signal.DoSerialize(clips);
                    }
                }
                if (track is IStorySerializable serializableTrack)
                {
                    serializableTrack.DoSerialize(clips);
                }
            }

            foreach (var o in a.outputs)
            {
                if (!config.Binding.ContainsKey(o.streamName))
                {
                    config.Binding[o.streamName] = 0;
                }
            }
            clips.Sort((a, b) =>
            {
                if (a.StartTime == b.StartTime) return 0;
                return a.StartTime - b.StartTime > 0 ? 1 : -1;
            });
            config.Clips = clips.ToArray();
        }
    
    }
}
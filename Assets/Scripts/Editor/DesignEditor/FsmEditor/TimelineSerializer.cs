using System.Collections.Generic;
using System.IO;
using Sirenix.Utilities;
using UnityEngine;
using UnityEditor;
using Slate;

namespace TaoTie
{
    public static class TimelineSerializer
    {
        public static ConfigFsmTimeline GetTimeline(string path)
        {
            var a = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (a != null)
            {
                List<ConfigFsmClip> clips = new List<ConfigFsmClip>();
                var src = a.GetComponent<Cutscene>();
                List<CutsceneGroup> groups = src.groups;
                for (int i = 0; i < groups.Count; i++)
                {
                    CutsceneGroup group = groups[i];
                    if (!(group is ActorGroup))
                    {
                        continue;
                    }

                    for (int j = 0; j < group.tracks.Count; j++)
                    {
                        if (group.tracks[j] is FSMTrack track)
                        {
                            for (int k = 0; k < track.clips.Count; k++)
                            {
                                if (track.clips[k] is ISerializableClip clip)
                                {
                                    clip.DoSerialize(clips);
                                }
                            }
                        }
                    }
                }
                ConfigFsmTimeline data = new ConfigFsmTimeline();
                bool isOld = false;
                
                data.Length = src.length;
                data.Clips = clips.ToArray();
                data.Clips.Sort((a, b) =>
                {
                    if (a.StartTime == b.StartTime) return 0;
                    return a.StartTime - b.StartTime > 0 ? 1 : -1;
                });
                return data;
            }

            return null;
        }
    }
}
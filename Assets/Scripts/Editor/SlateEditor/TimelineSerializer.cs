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
        [MenuItem("Assets/工具/Fsm/导出ConfigFsmTimeline", true)]
        public static bool ValidateFunction()
        {
            var assets = Selection.assetGUIDs;
            for (int i = 0; i < assets.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(assets[i]);
                if (Directory.Exists(path))
                {
                    return true;
                }

                if (path.EndsWith(".prefab"))
                {
                    var a = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (a != null && a.GetComponent<Slate.Cutscene>() != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        [MenuItem("Assets/工具/Fsm/导出ConfigFsmTimeline")]
        public static void DoSerializeTimelineNew()
        {
            var assets = Selection.assetGUIDs;
            for (int i = 0; i < assets.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(assets[i]);
                if (Directory.Exists(path))
                {
                    DoSerializeDirectoryTimeline(path);
                }
                if (path.EndsWith(".prefab"))
                {
                    DoSerializeFileTimeline(path);
                }
            }
        }

        private static void DoSerializeDirectoryTimeline(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            var dirs = dir.GetDirectories();
            for (int i = 0; i < dirs.Length; i++)
            {
                DoSerializeDirectoryTimeline(path + "/" + dirs[i].Name);
            }

            var files = dir.GetFiles();
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Name.EndsWith(".prefab"))
                {
                    DoSerializeFileTimeline(path + "/" + files[i].Name);
                }
            }
        }

        private static void DoSerializeFileTimeline(string path)
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

                // 对于Edit文件夹下的Timeline，会在Publish创建一个相同命名的序列化文件
                // 否则在相同文件夹下创建一个相同命名的序列化文件
                string outpath = path;
                if (outpath.Contains(UtilityEditor.EditDirName))
                {
                    outpath = UtilityEditor.ToPublishFilePath(outpath);
                }
                outpath = outpath.Replace(".prefab", ".bytes");
                ConfigFsmTimeline data = new ConfigFsmTimeline();
                bool isOld = false;
                
                data.length = src.length;
                data.clips = clips.ToArray();
                data.clips.Sort((a, b) =>
                {
                    if (a.StartTime == b.StartTime) return 0;
                    return a.StartTime - b.StartTime > 0 ? 1 : -1;
                });
                File.WriteAllText(outpath,JsonHelper.ToJson(data));
                AssetDatabase.Refresh();
            }
        }
    }
}
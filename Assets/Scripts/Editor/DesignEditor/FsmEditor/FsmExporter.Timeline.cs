using System.Collections.Generic;
using System.IO;

namespace TaoTie
{
    public partial class FsmExporter
    {
        #region Fsm Timeline

        public static Dictionary<string, ConfigFsmTimeline> LoadFsmTimeline(string path)
        {
            var fsmTimelineDict = new Dictionary<string, ConfigFsmTimeline>();
            LoadFsmTimelineInPath(fsmTimelineDict, path);
            return fsmTimelineDict;
        }

        private static void LoadFsmTimelineInPath(Dictionary<string, ConfigFsmTimeline> fsmTimelineDict, string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
                return;

            FileInfo[] fileInfos = dirInfo.GetFiles("*.playable", SearchOption.TopDirectoryOnly);
            foreach (FileInfo fileInfo in fileInfos)
            {
                ConfigFsmTimeline timeline = TimelineSerializer.GetFsmFromTimeline(Path.Combine(path, fileInfo.Name));
                fsmTimelineDict.Add(fileInfo.Name.Split('.')[0], timeline);
            }
        }

        #endregion
    }
}
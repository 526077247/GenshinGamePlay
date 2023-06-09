using System.Collections.Generic;

namespace TaoTie
{
    public partial class SceneConfigCategory
    {
        private Dictionary<string, SceneConfig> nameMap;
        public override void AfterEndInit()
        {
            base.AfterEndInit();
            nameMap = new Dictionary<string, SceneConfig>();
            for (int i = 0; i < list.Count; i++)
            {
                nameMap.Add(list[i].Name, list[i]);
            }
        }

        public bool TryGetByName(string name, out SceneConfig config)
        {
            return nameMap.TryGetValue(name, out config);
        }
    }
}
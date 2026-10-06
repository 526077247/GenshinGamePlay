using System.IO;
using YooAsset.Editor;
using TaoTie;

namespace YooAsset
{
	
    [DisplayName("收集Unit")]
    public class CollectUnit : IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            if (data.AssetPath.Contains("/Edit/")) return false;
            var ext = Path.GetExtension(data.AssetPath);
            if(Define.ConfigType == 1 && ext == ".json") return false;
            if(Define.ConfigType == 0 && ext == ".bytes") return false;
            return ext == ".prefab" || ext == ".bytes"|| ext == ".json" || ext == ".controller" || 
                   data.AssetPath.Contains("Common");
        }
    }
    
    [DisplayName("收集Unit依赖")]
    public class CollectUnitDepend : IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            if (data.AssetPath.Contains("/Edit/")) return false;
            var ext = Path.GetExtension(data.AssetPath);
            if(Define.ConfigType == 1 && ext == ".json") return false;
            if(Define.ConfigType == 0 && ext == ".bytes") return false;
            return !(ext == ".prefab" || ext == ".bytes"|| ext == ".json" || ext == ".controller" || 
                   data.AssetPath.Contains("Common"));
        }
    }
	
    [DisplayName("收集UI")]
    public class CollectUI : IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            if (data.AssetPath.Contains("/Animations/")) return false;
            if (data.AssetPath.Contains("/Atlas/")) return false;
            if (data.AssetPath.Contains("/Spine/")) return data.AssetPath.EndsWith("asset");
            return true;
        }
    }
    
    [DisplayName("收集UI依赖")]
    public class CollectUIDepend : IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            if (data.AssetPath.Contains("/Animations/")) return true;
            if (data.AssetPath.Contains("/Atlas/")) return true;
            if (data.AssetPath.Contains("/Spine/")) return !data.AssetPath.EndsWith("asset");
            return false;
        }
    }
        
    [DisplayName("收集EditorConfig")]
    public class CollectEditorConfig : IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            if(Define.ConfigType == 1)
                return data.AssetPath.EndsWith(".bytes");
            return data.AssetPath.EndsWith(".json");

        }
    }
}
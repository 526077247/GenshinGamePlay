using YooAsset;

namespace TaoTie
{
    public interface IPackageFinder
    {
        public string GetPackageName(string path);
    }

    public class DefaultPackageFinder:IPackageFinder
    {
        public string GetPackageName(string path)
        {
            return YooAssetsMgr.DefaultName;
        }
    }
}
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
            if (path.StartsWith(Define.ResourcesName)) return Define.ResourcesName;
            return Define.DefaultName;
        }
    }
}
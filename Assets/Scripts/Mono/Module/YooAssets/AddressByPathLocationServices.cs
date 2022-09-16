using System.IO;

namespace YooAsset
{
    public class AddressByPathLocationServices : ILocationServices
    {
        private readonly string _resourceRoot;

        public AddressByPathLocationServices(string resourceRoot)
        {
            if (!string.IsNullOrEmpty(resourceRoot))
                _resourceRoot = PathHelper.GetRegularPath(resourceRoot);
        }

        string ILocationServices.ConvertLocationToAssetPath(string location)
        {
            return _resourceRoot+"/"+ location;
        }
    }
}
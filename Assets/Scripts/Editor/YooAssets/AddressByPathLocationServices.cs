using System.IO;

namespace YooAsset.Editor
{
    public class AddressByPathLocationServices : IAddressRule
    {
        static string addressable_path = "Assets/AssetsPackage/";
        public string GetAssetAddress(AddressRuleData data)
        {
            return PathHelper.GetRegularPath(data.AssetPath).Replace(addressable_path,"");
        }
    }
}
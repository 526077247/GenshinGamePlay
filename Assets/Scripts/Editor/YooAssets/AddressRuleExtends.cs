using YooAsset.Editor;

namespace YooAsset
{
    [DisplayName("定位地址: 相对路径")]
    public class AddressByPathLocationServices : IAddressRule
    {
        static string addressablePath = "Assets/AssetsPackage/";
        public string GetAssetAddress(AddressRuleData data)
        {
            return EditorTools.GetRegularPath(data.AssetPath).Replace(addressablePath,"");
        }
    }
}
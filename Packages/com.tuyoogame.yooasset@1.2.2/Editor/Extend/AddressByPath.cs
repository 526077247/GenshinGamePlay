
namespace YooAsset.Editor
{
    //自定义扩展范例
    public class AddressByPath : IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            return data.AssetPath.Replace("Assets/AssetsPackage/","");
        }
    }
}
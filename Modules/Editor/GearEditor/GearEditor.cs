using Sirenix.OdinInspector.Editor;

namespace TaoTie
{
    public class GearEditor: OdinEditorWindow
    {
        public ConfigGear ConfigGear;
        
        public void Update()
        {
            OdinDropdownHelper.gear = ConfigGear;
        }
    }
}
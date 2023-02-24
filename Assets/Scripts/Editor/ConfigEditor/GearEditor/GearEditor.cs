namespace TaoTie
{
    public class GearEditor: BaseEditorWindow<ConfigGear>
    {
        protected override string folderPath => base.folderPath + "/Gear";
        public void Update()
        {
            OdinDropdownHelper.gear = data;
        }
        
    }
}
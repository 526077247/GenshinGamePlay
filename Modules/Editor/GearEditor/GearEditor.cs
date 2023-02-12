namespace TaoTie
{
    public class GearEditor: BaseEditorWindow<ConfigGear>
    {
       
        public void Update()
        {
            OdinDropdownHelper.gear = data;
        }
        
    }
}
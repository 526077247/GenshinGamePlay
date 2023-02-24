using System.Collections.Generic;

namespace TaoTie
{
    public class AbilityEditor: BaseEditorWindow<List<ConfigAbility>>
    {
        protected override string fileName => "Abilities";

        protected override string folderPath => base.folderPath + "/Unit";
    }
}
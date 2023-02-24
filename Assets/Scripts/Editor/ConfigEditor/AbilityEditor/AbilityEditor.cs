using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace TaoTie
{
    public class AbilityEditor: BaseEditorWindow<List<ConfigAbility>>
    {
        protected override string fileName => "Abilities";

        protected override string folderPath => base.folderPath + "/Unit";
    }
}
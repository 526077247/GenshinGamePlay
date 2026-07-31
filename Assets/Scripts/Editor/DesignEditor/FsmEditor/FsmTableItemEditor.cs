#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
#else
using TaoTie.Inspector;
using TaoTie.Inspector.Editor;
using OdinEditorWindow = TaoTie.Inspector.Editor.TaoTieEditorWindow;
#endif

namespace TaoTie
{
    public class FsmTableItemEditor: OdinEditorWindow
    {
        [HideReferenceObjectPicker] 
        public ConfigFsmTableItem Data;
        
    }
}
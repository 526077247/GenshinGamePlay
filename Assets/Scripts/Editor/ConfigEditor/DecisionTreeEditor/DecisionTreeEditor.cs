namespace TaoTie
{
    public class DecisionTreeEditor: BaseEditorWindow<DecisionNode>
    {
        protected override string fileName => "DecisionTree";
        protected override string folderPath => base.folderPath + "/Unit";
        protected override DecisionNode CreateInstance()
        {
            return new DecisionActionNode();
        }
    }
}
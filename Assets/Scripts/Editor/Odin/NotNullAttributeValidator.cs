using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using TaoTie;
[assembly: RegisterValidator(typeof(NotNullAttributeValidator))]

namespace TaoTie
{
    #region AttributeValidator EmptyValidator
    public class NotNullAttributeValidator : AttributeValidator<NotNullAttribute>
    {
        public override bool CanValidateProperty(InspectorProperty property)
        {
            return true;
        }
        protected override void Initialize()
        {
            base.Initialize();
        }
        protected override void Validate(ValidationResult result)
        {
            if (Property.BaseValueEntry.WeakSmartValue == null ||
                (Property.BaseValueEntry.WeakSmartValue is string strVal && string.IsNullOrWhiteSpace(strVal)))
            {
                result.Message = Property.NiceName + "不能为空";
                result.ResultType = ValidationResultType.Error;
            }
        }

    }
    #endregion
}
#if !ODIN_INSPECTOR
namespace DaGenGraph
{
    /// <summary>
    /// 
    /// </summary>
    public interface IValueDropdownItem
    {
        /// <summary>Gets the label for the dropdown item.</summary>
        /// <returns>The label text for the item.</returns>
        string GetText();

        /// <summary>Gets the value of the dropdown item.</summary>
        /// <returns>The value for the item.</returns>
        object GetValue();
    }
    public struct ValueDropdownItem: IValueDropdownItem
    {
        /// <summary>The name of the item.</summary>
        public string Text;
        /// <summary>The value of the item.</summary>
        public object Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.ValueDropdownItem`1" /> class.
        /// </summary>
        /// <param name="text">The text to display for the dropdown item.</param>
        /// <param name="value">The value for the dropdown item.</param>
        public ValueDropdownItem(string text, object value)
        {
            this.Text = text;
            this.Value = value;
        }

        /// <summary>The name of this item.</summary>
        public override string ToString() => this.Text ?? this.Value?.ToString() ?? "";

        /// <summary>Gets the text.</summary>
        string IValueDropdownItem.GetText() => this.Text;

        /// <summary>Gets the value.</summary>
        object IValueDropdownItem.GetValue() => this.Value;
    }
}

#endif
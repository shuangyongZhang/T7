namespace UIWidgets.Examples
{
    using System.ComponentModel;

    /// <summary>
    /// TreeViewSampleItem interace.
    /// </summary>
    public interface ITreeViewSampleItem : INotifyPropertyChanged
	{
		/// <summary>
		/// Display item data using specified component.
		/// </summary>
		/// <param name="component">Component.</param>
		void Display(TreeViewSampleComponent component);
	}
}
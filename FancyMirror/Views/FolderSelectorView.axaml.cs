using Avalonia.Controls;
using Avalonia.Interactivity;

namespace FancyMirror.Views;

public partial class FolderSelectorView : UserControl
{
    #region Private Members

    /// <summary>
    /// the window that displays the content
    /// </summary>
    private readonly MainWindow _parentWindow;

    #endregion


    
    #region Constructor

    /// <summary>
    /// default constructor
    /// </summary>
    /// <param name="parentWindow"></param>
    public FolderSelectorView(MainWindow parentWindow)
    {
        InitializeComponent();
        
        _parentWindow = parentWindow;
    }

    #endregion



    #region Private Methods

    /// <summary>
    /// switches from Folder Selector view to Diff Inspector
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Continue(object sender, RoutedEventArgs e)
    {
        _parentWindow.SwitchWindow();
    }

    #endregion
}
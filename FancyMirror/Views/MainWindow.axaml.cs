using System;
using Avalonia.Controls;
using FancyMirror.ViewModels;

namespace FancyMirror.Views;

public partial class MainWindow : Window
{
    /// <summary>
    /// default constructor
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        MainContent.Content = new FolderSelector(this);
    }

    public void SwitchWindow()
    {
        (DataContext as MainWindowViewModel)?.LoadDiffs();
        MainContent.Content = new DifferView
        {
            DataContext = DataContext
        };
    }
}
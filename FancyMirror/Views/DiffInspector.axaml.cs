using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Net.Mime;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ExCSS;
using FancyMirror.DataModels;
using FancyMirror.Services;
using FancyMirror.ViewModels;
using Color = Avalonia.Media.Color;
using Colors = Avalonia.Media.Colors;
using HorizontalAlignment = Avalonia.Layout.HorizontalAlignment;

namespace FancyMirror.Views;

public partial class DifferView : UserControl
{
    private StackPanel _buttonsStackPanel;
    private TextBlock _textBlock;
    
    public DifferView()
    {
        InitializeComponent();
        
        this._buttonsStackPanel = this.FindControl<StackPanel>("ButtonsStackPanel") ?? throw new Exception("could not find Buttons Stack Panel");
        this._textBlock = this.FindControl<TextBlock>("ContentTextBloc") ?? throw new Exception("could not find ContentTextbox");
        
        this.DataContextChanged += OnDataContextChanged;
    }
    
    private void OnDataContextChanged(object sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel mainViewModel)
        {
            LoadDiffs(mainViewModel);
            GenerateButtons();
        }
    }
    
    private ObservableCollection<DiffedFileContent> _diffedFiles = new ObservableCollection<DiffedFileContent>();

    private void LoadDiffs(MainWindowViewModel mainViewModel)
    {
        
        var path1 = mainViewModel.FolderOnePath;
        var path2 = mainViewModel.FolderTwoPath;
        
        FileDiffer diffService = new FileDiffer();
        diffService.DiffCommonFilesFromDirs(path1, path2);
        _diffedFiles = diffService.GetDiffedFiles();
    }

    private void GenerateButtons()
    {
        var mainFont = Application.Current?.Resources["MainFont"] as FontFamily;
        var background = Application.Current?.Resources["Background"] as SolidColorBrush;
        var foreground = Application.Current?.Resources["Foreground"] as SolidColorBrush;
        var border = Application.Current?.Resources["Primary"] as SolidColorBrush;
        
        foreach (var file in _diffedFiles)
        {
            var button = new Button
            {
                Content = file.Title,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                FontFamily = mainFont ?? new FontFamily("default"),
                FontSize = 14,
                Background = background,
                BorderBrush = border ?? new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 3)
            };
            
            // Use the Click event to load content
            button.Click += (sender, args) => LoadContentOnScreen(file);

            this._buttonsStackPanel.Children.Add(button);
        }
    }

    private void LoadContentOnScreen(DiffedFileContent content)
    {
        this._textBlock.Inlines.Clear(); // clear the text block
        var regex = new Regex(@"(<old>)([\s\S]*?)(</old>)|(<new>)([\s\S]*?)(</new>)|([^<]+)");

        // Match all text portions, including those inside <old> and <new> tags
        var matches = regex.Matches(content.Content);

        foreach (Match match in matches)
        {
            if (match.Groups[1].Success) // Match for <old>...</old>
            {
                string word = match.Groups[2].Value;
                _textBlock.Inlines.Add(new Run(word) { Foreground = Brushes.Red });
            }
            else if (match.Groups[4].Success) // Match for <new>...</new>
            {
                string word = match.Groups[5].Value;
                _textBlock.Inlines.Add(new Run(word) { Foreground = Brushes.Green });
            }
            else if (match.Groups[7].Success) // Match for plain text (not inside any tag)
            {
                _textBlock.Inlines.Add(new Run(match.Groups[7].Value));
            }
        }
    }
}
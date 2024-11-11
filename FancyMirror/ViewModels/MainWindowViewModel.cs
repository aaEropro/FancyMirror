using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyMirror.DataModels;
using FancyMirror.Services;


namespace FancyMirror.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    #region Private Memebers
    
    [ObservableProperty] private Uri? _folderOnePath = null;
    
    [ObservableProperty] private Uri? _folderTwoPath = null;
    
    [ObservableProperty]
    private ObservableCollection<DiffedFileContent> _diffedFiles;

    #endregion


    
    #region Public Members

    /// <summary>
    /// indicates if there are folders selected.
    /// </summary>
    public bool AreFoldersSelected => FolderOnePath != null && FolderTwoPath != null && FolderOnePath != FolderTwoPath;

    #endregion



    #region Constructor

    /// <summary>
    /// default constructor.
    /// </summary>
    public MainWindowViewModel()
    {
        _diffedFiles = new ObservableCollection<DiffedFileContent>();
        
        PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(FolderOnePath) || e.PropertyName == nameof(FolderTwoPath))
            {
                OnPropertyChanged(nameof(AreFoldersSelected));
            }
        };
    }

    #endregion


    #region Public Methods

    /// <summary>
    /// generates diffs for the files in the selected folders.
    /// </summary>
    public void LoadDiffs()
    {
        if (!AreFoldersSelected)
            return;
        
        FileDiffer diffService = new FileDiffer();
        diffService.DiffCommonFilesFromDirs(FolderTwoPath, FolderTwoPath);
        DiffedFiles = diffService.GetDiffedFiles();
    }
    
    
    [RelayCommand]
    public async Task SelectFirstFolder(Window? parentWindow)
    {
        if (parentWindow == null)
        {
            // Handle the case where the parentWindow is null
            return;
        }

        var storage = parentWindow.StorageProvider;
        
        var folderPath = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false,
            
        });

        if (folderPath.Count != 0)
        {
            FolderOnePath = folderPath.First().Path;
        }

    }
    
    [RelayCommand]
    public async Task SelectSecondFolder(Window? parentWindow)
    {
        if (parentWindow == null)
        {
            return;
        }

        var storage = parentWindow.StorageProvider;
        
        var folderPath = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false,
            
        });
        
        if (folderPath.Count != 0)
            FolderTwoPath = folderPath.Last().Path;
    }

    #endregion
    
}

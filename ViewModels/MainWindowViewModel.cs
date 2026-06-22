using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Poe2BuildManager.Models;
using Poe2BuildManager.Services;
using Poe2BuildManager.Views;

namespace Poe2BuildManager.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;

    private readonly AppSettings _settings;

    private readonly BuildPlanService _buildPlanService;

    private readonly ImportBuildService _importBuildService;

    private FileSystemWatcher? _buildFolderWatcher;

    private DateTime _lastRefresh = DateTime.MinValue;

    public ObservableCollection<BuildPlan> BuildPlans { get; }
        = new();

    public string BuildListHeader =>
        $"Installed Builds ({BuildPlans.Count(x => !x.IsDisabled)})";

    public string EnableDisableButtonText =>
        SelectedBuildPlan?.IsDisabled == true
            ? "Enable Build"
            : "Disable Build";

    [ObservableProperty]
    private string buildFolder = "";

    [ObservableProperty]
    private string buildPlanUrl = "";

    [ObservableProperty]
    private string status = "Ready";

    [ObservableProperty]
    private BuildPlan? selectedBuildPlan;

    [ObservableProperty]
    private bool hideDisabledBuilds;

    public MainWindowViewModel()
    {
        _settingsService = new SettingsService();
        _buildPlanService = new BuildPlanService();
        _importBuildService = new ImportBuildService();

        _settings = _settingsService.Load();

        BuildFolder = _settings.BuildFolder;

        LoadBuildPlans();
        StartBuildFolderWatcher();
    }

    private void LoadBuildPlans()
    {
        BuildPlans.Clear();

        var buildPlans =
            _buildPlanService.GetBuildPlans(BuildFolder);

        if (HideDisabledBuilds)
        {
            buildPlans =
                buildPlans
                    .Where(x => !x.IsDisabled)
                    .ToList();
        }

        foreach (var buildPlan in buildPlans.OrderBy(b => b.BuildName))
        {
            BuildPlans.Add(buildPlan);
        }

        Status = $"{BuildPlans.Count} build plans loaded";

        OnPropertyChanged(nameof(BuildListHeader));
    }

    [RelayCommand(CanExecute = nameof(CanDeleteBuildPlan))]
    private void DeleteBuildPlan()
    {
        if (SelectedBuildPlan == null)
        {
            return;
        }

        var result = MessageBox.Show(
            $"Are you sure you want to delete:\n\n{SelectedBuildPlan.BuildName}",
            "Delete Build",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        var deletedIndex =
            BuildPlans.IndexOf(SelectedBuildPlan);

        File.Delete(SelectedBuildPlan.FullPath);

        LoadBuildPlans();

        if (BuildPlans.Count > 0)
        {
            SelectedBuildPlan =
                BuildPlans[
                    Math.Min(
                        deletedIndex,
                        BuildPlans.Count - 1)];
        }

        Status =
            "Build deleted";
    }

    private bool CanDeleteBuildPlan()
    {
        return SelectedBuildPlan != null;
    }

    partial void OnSelectedBuildPlanChanged(BuildPlan? value)
    {
        DeleteBuildPlanCommand.NotifyCanExecuteChanged();
        SaveBuildCommand.NotifyCanExecuteChanged();
        ToggleBuildEnabledCommand.NotifyCanExecuteChanged();

        OnPropertyChanged(nameof(EnableDisableButtonText));
    }

    public void ImportFiles(
    IEnumerable<string> files)
    {
        var imported = 0;

        foreach (var file in files)
        {
            imported +=
                _importBuildService.Import(
                    file,
                    BuildFolder);
        }

        LoadBuildPlans();

        Status =
            $"{imported} build files imported";
    }

    public void SetBuildFolder(string folder)
    {
        BuildFolder = folder;

        _settings.BuildFolder = folder;

        _settingsService.Save(_settings);

        LoadBuildPlans();

        StartBuildFolderWatcher();

        Status = $"Build folder changed to: {folder}";
    }
    
    [RelayCommand(CanExecute = nameof(CanSaveBuild))]
    private void SaveBuild()
    {
        if (SelectedBuildPlan == null)
        {
            return;
        }

        var selectedFile =
            SelectedBuildPlan.FullPath;

        _buildPlanService.SaveBuildPlan(
            SelectedBuildPlan);

        LoadBuildPlans();

        SelectedBuildPlan =
            BuildPlans.FirstOrDefault(
                x => x.FullPath == selectedFile);

        Status =
            $"Saved '{SelectedBuildPlan?.BuildName}'";
    }

    private bool CanSaveBuild()
    {
        return SelectedBuildPlan != null;
    }

    [RelayCommand(CanExecute = nameof(CanDisableBuild))]
    private void DisableBuild()
    {
        if (SelectedBuildPlan == null)
        {
            return;
        }

        var newPath =
            SelectedBuildPlan.FullPath + ".disabled";

        File.Move(
            SelectedBuildPlan.FullPath,
            newPath);

        LoadBuildPlans();

        SelectedBuildPlan =
            BuildPlans.FirstOrDefault(
                x => x.FullPath == newPath);

        Status =
            $"Disabled '{SelectedBuildPlan?.BuildName}'";

        OnPropertyChanged(nameof(EnableDisableButtonText));
    }

    private bool CanDisableBuild()
    {
        return SelectedBuildPlan != null
            && !SelectedBuildPlan.IsDisabled;
    }

    [RelayCommand(CanExecute = nameof(CanEnableBuild))]
    private void EnableBuild()
    {
        if (SelectedBuildPlan == null)
        {
            return;
        }

        var newPath =
            SelectedBuildPlan.FullPath
                .Replace(
                    ".disabled",
                    "",
                    StringComparison.OrdinalIgnoreCase);

        File.Move(
            SelectedBuildPlan.FullPath,
            newPath);

        LoadBuildPlans();

        SelectedBuildPlan =
            BuildPlans.FirstOrDefault(
                x => x.FullPath == newPath);

        Status =
            $"Enabled '{SelectedBuildPlan?.BuildName}'";
        
        OnPropertyChanged(nameof(EnableDisableButtonText));
    }

    private bool CanEnableBuild()
    {
        return SelectedBuildPlan?.IsDisabled == true;
    }

    partial void OnHideDisabledBuildsChanged(bool value)
    {
        LoadBuildPlans();
    }

    [RelayCommand(CanExecute = nameof(CanToggleBuildEnabled))]
    private void ToggleBuildEnabled()
    {
        if (SelectedBuildPlan == null)
        {
            return;
        }

        if (SelectedBuildPlan.IsDisabled)
        {
            EnableBuild();
        }
        else
        {
            DisableBuild();
        }

        OnPropertyChanged(
            nameof(EnableDisableButtonText));
    }

    private bool CanToggleBuildEnabled()
    {
        return SelectedBuildPlan != null;
    }

    [RelayCommand]
    private void ViewJson()
    {
        if (SelectedBuildPlan == null)
        {
            return;
        }

        var json =
            File.ReadAllText(
                SelectedBuildPlan.FullPath);

        var window = new JsonViewerWindow(json);

        window.ShowDialog();
    }

    private void StartBuildFolderWatcher()
    {
        if (!Directory.Exists(BuildFolder))
        {
            return;
        }

        _buildFolderWatcher?.Dispose();

        _buildFolderWatcher = new FileSystemWatcher(BuildFolder);
        
        _buildFolderWatcher.Filter = "*.build*";
        
        _buildFolderWatcher.Created += OnBuildFolderChanged;
        _buildFolderWatcher.Deleted += OnBuildFolderChanged;
        _buildFolderWatcher.Renamed += OnBuildFolderChanged;

        _buildFolderWatcher.EnableRaisingEvents = true;
    }

    private void OnBuildFolderChanged(object? sender, FileSystemEventArgs e)
    {
        if ((DateTime.Now - _lastRefresh).TotalMilliseconds < 500)
        {
            return;
        }

        _lastRefresh = DateTime.Now;

        Application.Current.Dispatcher.Invoke(() =>
        {
            LoadBuildPlans();
        });
    }

    public void Dispose()
    {
        _buildFolderWatcher?.Dispose();
    }
}
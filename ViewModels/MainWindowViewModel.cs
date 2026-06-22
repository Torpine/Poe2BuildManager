using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Poe2BuildManager.Models;
using Poe2BuildManager.Services;

namespace Poe2BuildManager.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;

    private readonly AppSettings _settings;

    private readonly BuildPlanService _buildPlanService;

    private readonly ImportBuildService _importBuildService;

    public ObservableCollection<BuildPlan> BuildPlans { get; }
        = new();

    public string BuildListHeader =>
        $"Installed Builds ({BuildPlans.Count})";

    [ObservableProperty]
    private string buildFolder = "";

    [ObservableProperty]
    private string buildPlanUrl = "";

    [ObservableProperty]
    private string status = "Ready";

    [ObservableProperty]
    private BuildPlan? selectedBuildPlan;

    public MainWindowViewModel()
    {
        _settingsService = new SettingsService();
        _buildPlanService = new BuildPlanService();
        _importBuildService = new ImportBuildService();

        _settings = _settingsService.Load();

        BuildFolder = _settings.BuildFolder;

        LoadBuildPlans();
    }

    private void LoadBuildPlans()
    {
        BuildPlans.Clear();

        var buildPlans =
            _buildPlanService.GetBuildPlans(BuildFolder);

        foreach (var buildPlan in buildPlans)
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

        File.Delete(SelectedBuildPlan.FullPath);

        LoadBuildPlans();
    }

    private bool CanDeleteBuildPlan()
    {
        return SelectedBuildPlan != null;
    }

    partial void OnSelectedBuildPlanChanged(BuildPlan? value)
    {
        DeleteBuildPlanCommand.NotifyCanExecuteChanged();
        SaveBuildCommand.NotifyCanExecuteChanged();
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

        Status = $"Build folder changed to: {folder}";
    }
    
   [RelayCommand(CanExecute = nameof(CanSaveBuild))]
    private void SaveBuild()
    {
        if (SelectedBuildPlan == null)
        {
            return;
        }

        _buildPlanService.SaveBuildPlan(
            SelectedBuildPlan);

        Status =
            $"Saved '{SelectedBuildPlan.BuildName}'";
    }

    private bool CanSaveBuild()
    {
        return SelectedBuildPlan != null;
    }
}
using CommunityToolkit.Mvvm.ComponentModel;

namespace Poe2BuildManager.Models;

public partial class BuildPlan : ObservableObject
{
    [ObservableProperty]
    private string fileName = "";

    [ObservableProperty]
    private string fullPath = "";

    [ObservableProperty]
    private string buildName = "";

    [ObservableProperty]
    private string author = "";

    [ObservableProperty]
    private string ascendancy = "";

    [ObservableProperty]
    private string description = "";

    public bool HasDescription => 
        !string.IsNullOrWhiteSpace(Description);
}
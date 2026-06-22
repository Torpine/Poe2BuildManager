using System.Windows;
using Microsoft.Win32;
using Poe2BuildManager.ViewModels;
using System.Diagnostics;
using System.Windows.Input;

namespace Poe2BuildManager;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DataContext = new MainWindowViewModel();
    }

    private void BuildPlans_DragEnter(
        object sender,
        DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.None;
            return;
        }

        e.Effects = DragDropEffects.Copy;
    }

    private void BuildPlans_Drop(
        object sender,
        DragEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        var files =
            (string[])e.Data.GetData(
                DataFormats.FileDrop);

        vm.ImportFiles(files);
    }

    private void BrowseBuildFolder_Click(
    object sender,
    RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog();

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        if (DataContext is MainWindowViewModel vm)
        {
            vm.SetBuildFolder(dialog.FolderName);
        }
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        Process.Start(
            new ProcessStartInfo
            {
                FileName = vm.BuildFolder,
                UseShellExecute = true
            });
    }

    private void BuildPlans_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            if (vm.ViewJsonCommand.CanExecute(null))
            {
                vm.ViewJsonCommand.Execute(null);
            }
        }
    }
}
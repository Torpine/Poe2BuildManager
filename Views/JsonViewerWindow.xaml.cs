using System.Windows;
using Microsoft.Win32;
using Poe2BuildManager.ViewModels;
using System.Diagnostics;
using ICSharpCode.AvalonEdit;
using System.Text.Json;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace Poe2BuildManager.Views;

public partial class JsonViewerWindow : Window
{
    public JsonViewerWindow(string json)
    {
        InitializeComponent();

        Editor.SyntaxHighlighting =
            HighlightingManager.Instance.GetDefinition("JavaScript");
            
        var prettyJson =
            JsonSerializer.Serialize(
                JsonSerializer.Deserialize<object>(json),
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        Editor.Text = prettyJson;
    }
}
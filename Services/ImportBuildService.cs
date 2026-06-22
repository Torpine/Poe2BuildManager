using System.IO;
using System.IO.Compression;

namespace Poe2BuildManager.Services;

public class ImportBuildService
{
    public int Import(
        string sourceFile,
        string buildFolder)
    {
        var extension =
            Path.GetExtension(sourceFile)
                .ToLowerInvariant();

        return extension switch
        {
            ".build" => ImportBuildFile(
                sourceFile,
                buildFolder),

            ".zip" => ImportZipFile(
                sourceFile,
                buildFolder),

            _ => 0
        };
    }

    private int ImportBuildFile(
        string sourceFile,
        string buildFolder)
    {
        var destination =
            Path.Combine(
                buildFolder,
                Path.GetFileName(sourceFile));

        File.Copy(
            sourceFile,
            destination,
            true);

        return 1;
    }

    private int ImportZipFile(
        string sourceFile,
        string buildFolder)
    {
        var count = 0;

        using var archive =
            ZipFile.OpenRead(sourceFile);

        foreach (var entry in archive.Entries)
        {
            if (!entry.FullName.EndsWith(
                    ".build",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var destination =
                Path.Combine(
                    buildFolder,
                    Path.GetFileName(entry.FullName));

            entry.ExtractToFile(
                destination,
                true);

            count++;
        }

        return count;
    }
}
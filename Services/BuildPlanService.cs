using System.IO;
using System.Text.Json;
using Poe2BuildManager.Models;
using System.Text.Json.Nodes;

namespace Poe2BuildManager.Services;

public class BuildPlanService
{
    public List<BuildPlan> GetBuildPlans(string buildFolder)
    {
        var buildPlans = new List<BuildPlan>();

        if (!Directory.Exists(buildFolder))
        {
            return buildPlans;
        }

        var files = Directory.GetFiles(
            buildFolder,
            "*.build*");

        foreach (var file in files)
        {
            buildPlans.Add(CreateBuildPlan(file));
        }

        return buildPlans
            .OrderBy(x => x.FileName)
            .ToList();
    }

    private BuildPlan CreateBuildPlan(string file)
    {
        var buildPlan = new BuildPlan
        {
            FileName = Path.GetFileName(file),
            FullPath = file
        };

        try
        {
            var json = File.ReadAllText(file);

            using var document = JsonDocument.Parse(json);

            var root = document.RootElement;

            if (root.TryGetProperty("name", out var name))
            {
                buildPlan.BuildName =
                    name.GetString() ?? "";
            }

            if (root.TryGetProperty("author", out var author))
            {
                buildPlan.Author =
                    author.GetString() ?? "";
            }

            if (root.TryGetProperty("ascendancy", out var ascendancy))
            {
                buildPlan.Ascendancy =
                    ascendancy.GetString() ?? "";
            }

            if (root.TryGetProperty("description", out var description))
            {
                buildPlan.Description =
                    description.GetString() ?? "";
            }
        }
        catch
        {
            // Ignore malformed build files
        }

        return buildPlan;
    }

    public void SaveBuildPlan(
    BuildPlan buildPlan)
    {
        var json =
            File.ReadAllText(
                buildPlan.FullPath);

        var root =
            JsonNode.Parse(json)
            ?.AsObject();

        if (root == null)
        {
            return;
        }

        root["name"] =
            buildPlan.BuildName;

        root["author"] =
            buildPlan.Author;

        root["ascendancy"] =
            buildPlan.Ascendancy;

        root["description"] =
            buildPlan.Description;

        var updatedJson =
            root.ToJsonString(
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        File.WriteAllText(
            buildPlan.FullPath,
            updatedJson);
    }
}
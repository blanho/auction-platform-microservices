using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace Architecture.Tests;

public sealed class RepositoryStructureTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string SourceRoot = Path.Combine(RepositoryRoot, "src");
    private static readonly string ServicesRoot = Path.Combine(RepositoryRoot, "src", "Services");

    [Fact]
    public void DomainProjects_DoNotReferenceOuterLayersOrContracts()
    {
        var violations = GetProjects("*.Domain.csproj")
            .SelectMany(project => GetProjectReferences(project)
                .Where(reference => ContainsAny(reference, ".Application", ".Infrastructure", "BuildingBlocks.Web", ".Contracts"))
                .Select(reference => $"{Relative(project)} -> {reference}"))
            .ToList();

        Assert.True(violations.Count == 0, FormatViolations(violations));
    }

    [Fact]
    public void ApplicationProjects_DoNotReferenceInfrastructureOrWebLayers()
    {
        var violations = GetProjects("*.Application.csproj")
            .SelectMany(project => GetProjectReferences(project)
                .Where(reference => ContainsAny(reference, ".Infrastructure", "BuildingBlocks.Web"))
                .Select(reference => $"{Relative(project)} -> {reference}"))
            .ToList();

        Assert.True(violations.Count == 0, FormatViolations(violations));
    }

    [Fact]
    public void SourceFolders_UseEstablishedNamingConventions()
    {
        var violations = new List<string>();

        violations.AddRange(Directory
            .EnumerateDirectories(ServicesRoot, "Querys", SearchOption.AllDirectories)
            .Select(path => $"Use 'Queries': {Relative(path)}"));

        violations.AddRange(Directory
            .EnumerateFiles(ServicesRoot, "*Endpoints.cs", SearchOption.AllDirectories)
            .Where(path => !HasAreaFolder(path, "Endpoints"))
            .Select(path => $"Endpoint is not grouped by area: {Relative(path)}"));

        violations.AddRange(Directory
            .EnumerateFiles(ServicesRoot, "*DomainEventHandler.cs", SearchOption.AllDirectories)
            .Where(path => !HasFolderAfterProjectLayer(path, ".Application", "EventHandlers"))
            .Select(path => $"Domain-event handler is outside Application/EventHandlers: {Relative(path)}"));

        Assert.True(violations.Count == 0, FormatViolations(violations));
    }

    [Fact]
    public void ContractProjects_ContainContractsAndKeepOnePublicContractPerFile()
    {
        var violations = new List<string>();

        foreach (var project in GetProjects("*.Contracts.csproj"))
        {
            var projectDirectory = Path.GetDirectoryName(project)!;
            var sourceFiles = Directory
                .EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories)
                .Where(path => !IsGeneratedPath(path))
                .ToList();
            var protoFiles = Directory
                .EnumerateFiles(projectDirectory, "*.proto", SearchOption.AllDirectories)
                .ToList();

            if (sourceFiles.Count == 0 && protoFiles.Count == 0)
            {
                violations.Add($"Empty contracts project: {Relative(project)}");
            }

            foreach (var sourceFile in sourceFiles)
            {
                var source = File.ReadAllText(sourceFile);
                var publicContractCount = Regex.Matches(
                    source,
                    @"^\s*public\s+(?:sealed\s+)?(?:record|class|enum|interface)\b",
                    RegexOptions.Multiline).Count;

                if (publicContractCount > 1)
                {
                    violations.Add($"Multiple public contracts in {Relative(sourceFile)}");
                }
            }
        }

        Assert.True(violations.Count == 0, FormatViolations(violations));
    }

    [Fact]
    public void Services_HaveDomainAndApplicationTestProjects()
    {
        var violations = new List<string>();

        foreach (var serviceDirectory in Directory.EnumerateDirectories(ServicesRoot))
        {
            var serviceName = Path.GetFileName(serviceDirectory);

            foreach (var layer in new[] { "Domain", "Application" })
            {
                var testProject = Path.Combine(
                    serviceDirectory,
                    "tests",
                    $"{serviceName}.{layer}.Tests",
                    $"{serviceName}.{layer}.Tests.csproj");

                if (!File.Exists(testProject))
                {
                    violations.Add($"Missing {layer} test project: {Relative(testProject)}");
                    continue;
                }

                var expectedReference = $"../../{serviceName}.{layer}/{serviceName}.{layer}.csproj";
                if (!GetProjectReferences(testProject).Contains(expectedReference, StringComparer.OrdinalIgnoreCase))
                {
                    violations.Add($"Incorrect target reference in {Relative(testProject)}");
                }
            }
        }

        Assert.True(violations.Count == 0, FormatViolations(violations));
    }

    [Fact]
    public void AnalyticsFeatures_UseOperationLevelVerticalSlices()
    {
        var featuresRoot = Path.Combine(
            ServicesRoot,
            "Analytics",
            "Analytics.Application",
            "Features");

        var violations = Directory
            .EnumerateFiles(featuresRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => Path.GetRelativePath(featuresRoot, path)
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Length < 3)
            .Select(path => $"Analytics feature is not grouped by area and operation: {Relative(path)}")
            .ToList();

        Assert.True(violations.Count == 0, FormatViolations(violations));
    }

    [Fact]
    public void SemanticProtocolValues_UseCentralizedConstants()
    {
        var forbiddenPatterns = new Dictionary<string, string>
        {
            [@"Status\s*:\s*""Queued"""] = "background job status",
            [@"Status\s*=\s*""Unread"""] = "notification status",
            [@"Type\s*=\s*""BulkNotification"""] = "notification payload type",
            [@"Action\s*=\s*""(?:Login|PasswordChange|Enable2FA|Disable2FA)"""] = "identity audit action",
            [@"(?:==|!=)\s*""(?:Local|asc)"""] = "branch control value",
            [@"\[""(?:Action|PreviousStatus|Reason|AuctionId|UserId|OrderId|PaymentId|JobId|FileId)""\]\s*="] = "audit metadata key",
            [@"(?:metadata\[|TryGetValue\()""(?:OriginalFileName|OwnerId)"""] = "blob metadata key",
            [@"Currency\s*(?:\{|=|:)?.{0,40}=\s*""USD"""] = "currency default"
        };

        var violations = Directory
            .EnumerateFiles(SourceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(IsProductionSource)
            .SelectMany(path => forbiddenPatterns
                .Where(pattern => Regex.IsMatch(File.ReadAllText(path), pattern.Key, RegexOptions.Multiline))
                .Select(pattern => $"Raw {pattern.Value} in {Relative(path)}"))
            .ToList();

        Assert.True(violations.Count == 0, FormatViolations(violations));
    }

    [Fact]
    public void ServiceApis_RegisterRequestLocalization()
    {
        var violations = Directory
            .EnumerateDirectories(ServicesRoot)
            .Select(serviceDirectory => Directory
                .EnumerateDirectories(serviceDirectory, "*.Api", SearchOption.TopDirectoryOnly)
                .SingleOrDefault())
            .Where(apiDirectory => apiDirectory is not null)
            .Where(apiDirectory => !Directory
                .EnumerateFiles(apiDirectory!, "*.cs", SearchOption.AllDirectories)
                .Where(path => !IsGeneratedPath(path))
                .Any(path => File.ReadAllText(path).Contains("AddAppLocalization<", StringComparison.Ordinal)))
            .Select(apiDirectory => $"Localization is not registered in {Relative(apiDirectory!)}")
            .ToList();

        Assert.True(violations.Count == 0, FormatViolations(violations));
    }

    [Fact]
    public void ServiceErrorCodes_HaveEnglishAndJapaneseResources()
    {
        var sharedResourcesDirectory = Path.Combine(
            SourceRoot,
            "BuildingBlocks",
            "BuildingBlocks.Application",
            "Resources");
        var violations = new List<string>();

        foreach (var serviceDirectory in Directory.EnumerateDirectories(ServicesRoot))
        {
            var errorCodes = Directory
                .EnumerateFiles(serviceDirectory, "*.cs", SearchOption.AllDirectories)
                .Where(IsProductionSource)
                .SelectMany(path => ExtractErrorCodes(File.ReadAllText(path)))
                .ToHashSet(StringComparer.Ordinal);

            foreach (var culture in new[] { string.Empty, "ja-JP" })
            {
                var availableKeys = ReadResourceKeys(sharedResourcesDirectory, culture)
                    .Concat(ReadResourceKeys(serviceDirectory, culture))
                    .ToHashSet(StringComparer.Ordinal);

                violations.AddRange(errorCodes
                    .Except(availableKeys)
                    .Select(code => $"Missing {(culture.Length == 0 ? "English" : culture)} resource '{code}' in {Path.GetFileName(serviceDirectory)}"));
            }
        }

        Assert.True(violations.Count == 0, FormatViolations(violations));
    }

    [Fact]
    public void NeutralAndJapaneseResourceFiles_HaveMatchingKeys()
    {
        var violations = new List<string>();
        var neutralResources = Directory
            .EnumerateFiles(SourceRoot, "*.resx", SearchOption.AllDirectories)
            .Where(path => !IsGeneratedPath(path))
            .Where(path => !Regex.IsMatch(Path.GetFileName(path), @"\.[a-z]{2}-[A-Z]{2}\.resx$"));

        foreach (var neutralResource in neutralResources)
        {
            var japaneseResource = Path.ChangeExtension(neutralResource, ".ja-JP.resx");
            if (!File.Exists(japaneseResource))
            {
                violations.Add($"Missing Japanese resource file for {Relative(neutralResource)}");
                continue;
            }

            var neutralKeys = ReadResourceKeysFromFile(neutralResource);
            var japaneseKeys = ReadResourceKeysFromFile(japaneseResource);

            violations.AddRange(neutralKeys
                .Except(japaneseKeys)
                .Select(key => $"Missing Japanese resource '{key}' in {Relative(japaneseResource)}"));
            violations.AddRange(japaneseKeys
                .Except(neutralKeys)
                .Select(key => $"Missing neutral resource '{key}' in {Relative(neutralResource)}"));
        }

        Assert.True(violations.Count == 0, FormatViolations(violations));
    }

    private static IEnumerable<string> GetProjects(string pattern) =>
        Directory.EnumerateFiles(ServicesRoot, pattern, SearchOption.AllDirectories);

    private static IEnumerable<string> GetProjectReferences(string projectPath)
    {
        var document = XDocument.Load(projectPath);
        return document
            .Descendants("ProjectReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Cast<string>();
    }

    private static bool ContainsAny(string value, params string[] candidates) =>
        candidates.Any(candidate => value.Contains(candidate, StringComparison.OrdinalIgnoreCase));

    private static IEnumerable<string> ExtractErrorCodes(string source) =>
        Regex.Matches(
                source,
                @"(?:(?:Error|ValidationError)\.Create|LocalizableError\.Localizable)\s*\(\s*""([^""]+)""",
                RegexOptions.Multiline)
            .Select(match => match.Groups[1].Value);

    private static IEnumerable<string> ReadResourceKeys(string root, string culture)
    {
        var suffix = culture.Length == 0 ? ".resx" : $".{culture}.resx";

        return Directory
            .EnumerateFiles(root, $"*{suffix}", SearchOption.AllDirectories)
            .Where(path => culture.Length > 0 ||
                !Regex.IsMatch(Path.GetFileName(path), @"\.[a-z]{2}-[A-Z]{2}\.resx$"))
            .SelectMany(ReadResourceKeysFromFile);
    }

    private static HashSet<string> ReadResourceKeysFromFile(string path) =>
        XDocument.Load(path)
            .Descendants("data")
            .Select(element => element.Attribute("name")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Cast<string>()
            .ToHashSet(StringComparer.Ordinal);

    private static bool HasAreaFolder(string path, string anchorFolder)
    {
        var segments = Path.GetRelativePath(ServicesRoot, path)
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var anchorIndex = Array.FindIndex(
            segments,
            segment => segment.Equals(anchorFolder, StringComparison.OrdinalIgnoreCase));

        if (anchorIndex < 0 || anchorIndex + 2 >= segments.Length)
        {
            return false;
        }

        return true;
    }

    private static bool HasFolderAfterProjectLayer(string path, string projectSuffix, string expectedFolder)
    {
        var segments = Path.GetRelativePath(ServicesRoot, path)
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var projectIndex = Array.FindIndex(
            segments,
            segment => segment.EndsWith(projectSuffix, StringComparison.OrdinalIgnoreCase));

        return projectIndex >= 0 &&
            projectIndex + 2 < segments.Length &&
            segments[projectIndex + 1].Equals(expectedFolder, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsGeneratedPath(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) ||
        path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);

    private static bool IsProductionSource(string path) =>
        !IsGeneratedPath(path) &&
        !path.Contains($"{Path.DirectorySeparatorChar}tests{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) &&
        !path.Contains($"{Path.DirectorySeparatorChar}Tests{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
        !path.Contains($"{Path.DirectorySeparatorChar}Constants{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) &&
        !path.Contains(".Contracts", StringComparison.OrdinalIgnoreCase);

    private static string Relative(string path) => Path.GetRelativePath(RepositoryRoot, path);

    private static string FormatViolations(IReadOnlyCollection<string> violations) =>
        violations.Count == 0
            ? string.Empty
            : "Architecture violations:" + Environment.NewLine + string.Join(Environment.NewLine, violations);

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "auction.sln")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the repository root containing auction.sln");
    }
}

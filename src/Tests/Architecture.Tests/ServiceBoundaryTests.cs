using System.Xml.Linq;
using Xunit;

namespace Architecture.Tests;

public sealed class ServiceBoundaryTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string SourceRoot = Path.Combine(RepositoryRoot, "src");
    private static readonly string[] Projects = Directory
        .EnumerateFiles(SourceRoot, "*.csproj", SearchOption.AllDirectories)
        .Where(IsProductionProject)
        .OrderBy(path => path, StringComparer.Ordinal)
        .ToArray();

    public static IEnumerable<object[]> BoundedContexts => Directory
        .EnumerateDirectories(Path.Combine(SourceRoot, "Services"))
        .Select(Path.GetFileName)
        .Append("Orchestration")
        .Select(name => new object[] { name! });

    [Theory]
    [MemberData(nameof(BoundedContexts))]
    public void ServicesReferenceOtherContextsOnlyThroughContracts(string boundedContext)
    {
        var implementationProjects = Projects
            .Where(project => Owner(project) == boundedContext && !IsContract(project))
            .ToArray();
        Assert.NotEmpty(implementationProjects);

        var violations = implementationProjects.SelectMany(project => References(project)
            .Where(reference => Owner(reference) is { } owner && owner != boundedContext && !IsContract(reference))
            .Select(reference => $"{Relative(project)} -> {Relative(reference)}"));

        AssertNoViolations(violations);
    }

    [Fact]
    public void SharedBuildingBlocksDoNotDependOnServiceImplementationsOrServiceContracts()
    {
        var sharedProjects = Projects.Where(project => Parts(project)[0] == "BuildingBlocks").ToArray();
        Assert.NotEmpty(sharedProjects);

        AssertNoViolations(sharedProjects.SelectMany(project => References(project)
            .Where(reference => Owner(reference) is not null)
            .Select(reference => $"{Relative(project)} -> {Relative(reference)}")));
    }

    [Fact]
    public void ContractsDoNotPullImplementationAssembliesAcrossServiceBoundaries()
    {
        var contracts = Projects.Where(IsContract).ToArray();
        Assert.NotEmpty(contracts);

        AssertNoViolations(contracts.SelectMany(project => References(project)
            .Where(reference => !IsContract(reference))
            .Select(reference => $"{Relative(project)} -> {Relative(reference)}")));
    }

    [Fact]
    public void ProductionProjectsNeverReferenceTestProjects()
    {
        AssertNoViolations(Projects.SelectMany(project => References(project)
            .Where(reference => !IsProductionProject(reference))
            .Select(reference => $"{Relative(project)} -> {Relative(reference)}")));
    }

    [Fact]
    public void EveryProjectReferenceResolvesToASourceProject()
    {
        var sourceProjects = Projects.ToHashSet(StringComparer.OrdinalIgnoreCase);
        AssertNoViolations(Projects.SelectMany(project => References(project)
            .Where(reference => !sourceProjects.Contains(reference))
            .Select(reference => $"Unresolved production reference: {Relative(project)} -> {Relative(reference)}")));
    }

    private static IEnumerable<string> References(string project) => XDocument.Load(project)
        .Descendants("ProjectReference")
        .Select(element => element.Attribute("Include")!.Value.Replace('\\', Path.DirectorySeparatorChar))
        .Select(reference => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(project)!, reference)));

    private static string? Owner(string project)
    {
        var parts = Parts(project);
        return parts[0] switch
        {
            "Services" => parts[1],
            "Orchestration" => "Orchestration",
            _ => null
        };
    }

    private static bool IsContract(string project) =>
        Path.GetFileNameWithoutExtension(project).EndsWith(".Contracts", StringComparison.Ordinal);

    private static bool IsProductionProject(string project) => !Parts(project)
        .Any(part => part.Equals("tests", StringComparison.OrdinalIgnoreCase) || part is "obj" or "bin");

    private static string[] Parts(string path) => Path.GetRelativePath(SourceRoot, path)
        .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    private static string Relative(string path) => Path.GetRelativePath(RepositoryRoot, path);

    private static void AssertNoViolations(IEnumerable<string> violations)
    {
        var failures = violations.ToArray();
        Assert.True(failures.Length == 0, string.Join(Environment.NewLine, failures));
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "auction.sln")))
                return directory.FullName;

        throw new DirectoryNotFoundException("Cannot find auction.sln from the test output directory.");
    }
}

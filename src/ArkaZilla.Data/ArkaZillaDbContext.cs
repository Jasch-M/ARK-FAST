using System.Reflection;
using ArkaZilla.Data.Models;
using ArkaZilla.Data.Models.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArkaZilla.Data;

/// <summary>
/// An implementation of the <see cref="DbContext"/> for this solution.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ArkaZillaDbContext"/>.
/// </remarks>
/// <param name="options">The options for this context.</param>
public class ArkaZillaDbContext(DbContextOptions<ArkaZillaDbContext> options) : DbContext(options)
{
    // Users
    public DbSet<User> Users { get; set; }

    // Logging
    public DbSet<Logging> Loggings { get; set; }
    public DbSet<LogLocation> LogLocations { get; set; }
    public DbSet<DefaultLoggingsForServers> DefaultLoggingsForServers { get; set; }
    public DbSet<DefaultLoggingsForCategories> DefaultLoggingsForCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("arkad");
        ApplyModuleConfigurations(modelBuilder);
    }

    private static void ApplyModuleConfigurations(ModelBuilder modelBuilder)
    {
        // On startup, explicitly load assemblies from a 'modules' directory
        // LoadModuleAssemblies();

        var prefixesToScan = new[] { "ArkaZilla.Modules.", "ArkaZilla.Modules.Data" };

        IEnumerable<Assembly> assembliesToScan = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => assembly.FullName is not null &&
                               prefixesToScan.Any(prefix => assembly.FullName.StartsWith(prefix)));

        foreach (Assembly assembly in assembliesToScan)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        }
    }

    private static void LoadModuleAssemblies()
    {
        string modulePath = FindRoot();
        if (!Directory.Exists(modulePath)) //TODO: Make this configurable
        {
            return;
        }

        IEnumerable<string> moduleDlls = Directory.GetFiles(modulePath, "*.dll", SearchOption.AllDirectories);

        foreach (string dllPath in moduleDlls)
        {
            string assemblyName = Path.GetFileNameWithoutExtension(dllPath);
            if (!assemblyName.StartsWith("ArkaZilla.Modules.Data"))
            {
                continue;
            }

            try
            {
                Assembly.Load(dllPath);
            }
            catch (FileLoadException)
            {
                Console.Error.WriteLine($"Failed to load {dllPath}");
            }
        }
    }

    private static string FindRoot()
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        currentDirectory = Path.GetFullPath(currentDirectory);
        while (!Directory.GetFiles(currentDirectory).Any(file => file.EndsWith(".sln")))
        {
            currentDirectory = Directory.GetParent(currentDirectory)?.FullName ?? string.Empty;
            if (currentDirectory == string.Empty)
            {
                return string.Empty;
            }
        }

        return currentDirectory;
    }
}

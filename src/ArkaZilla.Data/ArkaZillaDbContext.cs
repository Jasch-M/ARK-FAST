using System.Reflection;
using ArkaZilla.Data.Models;
using Microsoft.EntityFrameworkCore;

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
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("arkad");
        ApplyModuleConfigurations(modelBuilder);
    }

    private static void ApplyModuleConfigurations(ModelBuilder modelBuilder)
    {
        // On startup, explicitly load assemblies from a 'modules' directory
        LoadModuleAssemblies();

        var prefixesToScan = new[] { "ArkaZilla.Module.", "ArkaZilla.Data.Module." };

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
        if (!Directory.Exists(modulesPath))
        {
            return;
        }

        IEnumerable<string> moduleDlls = Directory.GetFiles(modulesPath, "*.dll", SearchOption.AllDirectories);

        foreach (string dllPath in moduleDlls)
        {
            try
            {
                Assembly.LoadFrom(dllPath);
            }
            catch (FileLoadException)
            {
            }
        }
    }
}

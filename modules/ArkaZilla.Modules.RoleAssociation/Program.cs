using ArkaZilla.Modules.Data.RoleAssociation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ArkaZilla.Modules.RoleAssociation;

public class Program
{
    public async static void Main(string[] args)
    {
        try
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }
        catch (Exception e)
        {

        }
    }

    public static HostApplicationBuilder CreateHostBuilder(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        builder.Services.AddNpgsql<RoleAssociationDbContext>(builder.Configuration.GetConnectionString("Default"),
            options => options.MigrationsHistoryTable("__EFMigrationsHistory", "arkad")
                .MigrationsAssembly(typeof(RoleAssociationDbContext).Assembly.FullName));

        return builder;
    }
}

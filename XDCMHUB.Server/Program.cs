using XDCMHUB.Server.Data;

namespace XDCMHUB.Server;

public class Program
{
	public static void Main(string[] args)
	{
		var host = CreateHostBuilder(args).Build();

        // Apply migrations on startup
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<AppDbContext>();
                var migrationManager = new DatabaseMigrationManager(context);

                // Apply any pending migrations
                migrationManager.ApplyMigrations();

                // Seed additional data if needed
                migrationManager.SeedAdditionalData();
            }
            catch (Exception ex)
            {
                // Log migration errors
                Console.WriteLine($"An error occurred while migrating the database: {ex}");
            }
        }

        host.Run();
	}

	public static IHostBuilder CreateHostBuilder(string[] args) =>
		Host.CreateDefaultBuilder(args)
			.ConfigureWebHostDefaults(webBuilder =>
			{
				webBuilder.UseStartup<Startup>();
			});
}
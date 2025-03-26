using Microsoft.EntityFrameworkCore;
using XDCMHUB.Server.Models;

namespace XDCMHUB.Server.Data;

public class DatabaseMigrationManager
{
    private readonly AppDbContext _context;

    public DatabaseMigrationManager(AppDbContext context)
    {
        _context = context;
    }

    // Method to apply migrations
    public void ApplyMigrations()
    {
        try
        {
            // Ensure database is created and all pending migrations are applied
            _context.Database.Migrate();
        }
        catch (Exception ex)
        {
            // Log or handle migration errors
            Console.WriteLine($"Migration error: {ex.Message}");
        }
    }

    // Method to seed additional data if needed
    public void SeedAdditionalData()
    {
        // Check if any data needs to be added programmatically
        if (!_context.Users.Any(u => u.Username == "buhmcdxadmin"))
        {
            _context.Users.Add(new User
            {
                Username = "buhmcdxadmin",
                Password = "$2a$11$Qwmg5pJ6bFXyRs.hpFONkuD5t1pe/fJTbIOPRV8dfhpYBPnQ2Rjsy" // 1234
            });
            _context.SaveChanges();
        }

        if(!_context.Channels.Any(c => c.Name == "XDCMHUBAdmins"))
        {
            _context.Channels.Add(new()
            {
                Name = "XDCMHUBAdmins",
                Description = "For Admins only"
            });
            _context.SaveChanges();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using XDCMHUB.Server.Data;
using XDCMHUB.Server.Models;

namespace XDCMHUB.Server.Services;

public class AuthService(AppDbContext context)
{
    readonly AppDbContext _context = context;

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        User? user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user is null) return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.Password)) return null;

        return user;
    }
}

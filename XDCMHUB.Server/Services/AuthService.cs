using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using XDCMHUB.Server.Data;
using XDCMHUB.Server.Models;

namespace XDCMHUB.Server.Services;

public class AuthService(AppDbContext context)
{
	readonly AppDbContext _context = context;

	public async Task<User?> AuthenticateAsync(string username, string password) => await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == password);
}

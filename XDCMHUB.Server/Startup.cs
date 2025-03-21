using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using XDCMHUB.Server.Data;
using XDCMHUB.Server.Hubs;
using XDCMHUB.Server.Services;
using Microsoft.EntityFrameworkCore;

namespace XDCMHUB.Server;

public class Startup(IConfiguration configuration)
{
	public IConfiguration Configuration { get; } = configuration;

	public void ConfigureServices(IServiceCollection services)
	{
		services.AddDbContext<AppDbContext>(options =>
			options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

		services.AddAuthentication("Basic")
			.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", null);

		services.AddSignalR();
		services.AddScoped<AuthService>();
		services.AddCors(options =>
		{
			options.AddPolicy("AllowAll", builder =>
				builder.AllowAnyOrigin()
					.AllowAnyMethod()
					.AllowAnyHeader());
		});
	}

	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		if (env.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}

		app.UseCors("AllowAll");
		app.UseRouting();

		app.UseAuthentication();
		app.UseAuthorization();

		app.UseEndpoints(endpoints =>
		{
			endpoints.MapHub<ChatHub>("/chathub");
			endpoints.MapGet("/", async context =>
			{
				await context.Response.WriteAsync("Chat Server Running");
			});
		});
	}
}

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
	private readonly AuthService _authService;

	public BasicAuthenticationHandler(
		IOptionsMonitor<AuthenticationSchemeOptions> options,
		ILoggerFactory logger,
		UrlEncoder encoder,
		Microsoft.AspNetCore.Authentication.ISystemClock clock,
		AuthService authService)
		: base(options, logger, encoder, clock)
	{
		_authService = authService;
	}

	protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		if (!Request.Headers.ContainsKey("Authorization"))
		{
			return AuthenticateResult.Fail("Missing Authorization Header");
		}

		try
		{
			var authHeader = Request.Headers["Authorization"].ToString();
			var credentialBytes = Convert.FromBase64String(authHeader.Replace("Basic ", ""));
			var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
			var username = credentials[0];
			var password = credentials[1];

			var user = await _authService.AuthenticateAsync(username, password);

			if (user == null)
			{
				return AuthenticateResult.Fail("Invalid Username or Password");
			}

			var claims = new[]
			{
					new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
					new Claim(ClaimTypes.Name, user.Username),
					new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
				};

			var identity = new ClaimsIdentity(claims, Scheme.Name);
			var principal = new ClaimsPrincipal(identity);
			var ticket = new AuthenticationTicket(principal, Scheme.Name);

			return AuthenticateResult.Success(ticket);
		}
		catch
		{
			return AuthenticateResult.Fail("Invalid Authorization Header");
		}
	}
}

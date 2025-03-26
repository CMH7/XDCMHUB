using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using XDCMHUB.Server.Data;
using XDCMHUB.Server.Models;

namespace XDCMHUB.Server.Hubs;

[Authorize]
public class AdminHub : Hub
{
    readonly AppDbContext _context;
    static readonly Dictionary<string, string> _userChannels = new Dictionary<string, string>();

    public AdminHub(AppDbContext context) => _context = context;

    public override async Task OnConnectedAsync()
    {
        var username = Context.User.Identity.Name;
        await Groups.AddToGroupAsync(Context.ConnectionId, "XDCMHUBAdmins");
        _userChannels[Context.ConnectionId] = "XDCMHUBAdmins";

        await Clients.Group("XDCMHUBAdmins").SendAsync("ReceiveMessage", "System", $"{username} entered admin side");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var username = Context.User.Identity.Name;

        if (_userChannels.TryGetValue(Context.ConnectionId, out var channelName))
        {
            await Clients.Group(channelName).SendAsync("ReceiveMessage", "System", $"{username} has left the admin side");
            _userChannels.Remove(Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task RegisterUser(string username, string password)
    {
        bool exist = await (
            from user in _context.Users
            where user.Username == username
            select user
            ).AnyAsync();

        if(!exist)
        {
            User newUser = new() { Username = username, Password = password, IsAdmin = false };
            await _context.AddAsync(newUser);
            await _context.SaveChangesAsync();

            await Clients.Group("XDCMHUBAdmins").SendAsync("ReceiveMessage", Context.User.Identity.Name, $"Registered {username} successfully");
        }
    }

    public async Task CreateChannel(string channel, string desc)
    {
        bool exist = await (
            from chan in _context.Channels
            where chan.Name.ToLower() == channel.ToLower()
            select chan
            ).AnyAsync();
        if(!exist)
        {
            Channel newChannel = new()
            {
                Name = channel,
                Description = desc
            };

            await _context.Channels.AddAsync(newChannel);
            await _context.SaveChangesAsync();

            await Clients.Group("XDCMHUBAdmins").SendAsync("ReceiveMessage", Context.User.Identity.Name, $"New Channel: {channel} created successfully");
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using XDCMHUB.Server.Data;
using XDCMHUB.Server.Models;

namespace XDCMHUB.Server.Hubs;

[Authorize]
public class ChatHub(AppDbContext appDb) : Hub
{
	static readonly Dictionary<string, string> _userChannels = [];
	static List<string> ConnectedUsers = [];

	public override async Task OnConnectedAsync()
	{
		var username = Context.User.Identity.Name;
		await Groups.AddToGroupAsync(Context.ConnectionId, "General");
		_userChannels[Context.ConnectionId] = "General";
		ConnectedUsers.Add(username);

		await Clients.Group("General").SendAsync("GetActiveUsers", ConnectedUsers);
		await Clients.Group("General").SendAsync("ReceiveMessage", "System", $"{username} has joined the chat");
		await base.OnConnectedAsync();
	}

	public override async Task OnDisconnectedAsync(Exception exception)
	{
		var username = Context.User.Identity.Name;

		if (_userChannels.TryGetValue(Context.ConnectionId, out var channelName))
		{
			ConnectedUsers.Remove(username);
			await Clients.Group(channelName).SendAsync("GetActiveUsers", ConnectedUsers);
			await Clients.Group(channelName).SendAsync("ReceiveMessage", "System", $"{username} has left the chat");
			_userChannels.Remove(Context.ConnectionId);
		}

		await base.OnDisconnectedAsync(exception);
	}

	public async Task SendMessage(string message)
	{
		var username = Context.User.Identity.Name;
		var userId = int.Parse(Context.UserIdentifier);

		if (_userChannels.TryGetValue(Context.ConnectionId, out var channelName))
		{
			var channel = await appDb.Channels.FirstOrDefaultAsync(c => c.Name == channelName);
			if (channel != null)
			{
				await Clients.Group(channelName).SendAsync("ReceiveMessage", username, message);
			}
		}
	}

	public async Task JoinChannel(string channelName)
	{
		var username = Context.User.Identity.Name;
		var channel = await appDb.Channels.FirstOrDefaultAsync(c => c.Name == channelName);

		if (channel == null)
		{
			await Clients.Caller.SendAsync("Error", $"Channel {channelName} does not exist");
			return;
		}

		if (_userChannels.TryGetValue(Context.ConnectionId, out var currentChannel))
		{
			await Groups.RemoveFromGroupAsync(Context.ConnectionId, currentChannel);
			await Clients.Group(currentChannel).SendAsync("ReceiveMessage", "System", $"{username} has left the channel");
		}

		await Groups.AddToGroupAsync(Context.ConnectionId, channelName);
		_userChannels[Context.ConnectionId] = channelName;

		await Clients.Group(channelName).SendAsync("ReceiveMessage", "System", $"{username} has joined the channel");
	}

	public async Task GetChannels()
	{
		var channels = await appDb.Channels.Select(c => c.Name).ToListAsync();
		await Clients.Caller.SendAsync("ChannelList", channels);
	}
}
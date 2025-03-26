using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using XDCMHUB.Server.Data;
using XDCMHUB.Server.Models;

namespace XDCMHUB.Server.Hubs;

[Authorize]
public class ChatHub : Hub
{
	readonly AppDbContext _context;
	static readonly Dictionary<string, string> _userChannels = new Dictionary<string, string>();

	public ChatHub(AppDbContext context) => _context = context;

	public override async Task OnConnectedAsync()
	{
		var username = Context.User.Identity.Name;
		await Groups.AddToGroupAsync(Context.ConnectionId, "General");
		_userChannels[Context.ConnectionId] = "General";

		await Clients.Group("General").SendAsync("ReceiveMessage", "System", $"{username} has joined the chat");
		await base.OnConnectedAsync();
	}

	public override async Task OnDisconnectedAsync(Exception exception)
	{
		var username = Context.User.Identity.Name;

		if (_userChannels.TryGetValue(Context.ConnectionId, out var channelName))
		{
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
			var channel = await _context.Channels.FirstOrDefaultAsync(c => c.Name == channelName);
			if (channel != null)
			{
				/**
				 * CM: Simply commented this to disable saving of messages to database hehe
				 */

				//var newMessage = new Message
				//{
				//	Content = message,
				//	UserId = userId,
				//	ChannelId = channel.Id,
				//	SentAt = DateTime.Now
				//};

				//_context.Messages.Add(newMessage);
				//await _context.SaveChangesAsync();

				await Clients.Group(channelName).SendAsync("ReceiveMessage", username, message);
			}
		}
	}

	public async Task JoinChannel(string channelName)
	{
		var username = Context.User.Identity.Name;
		var channel = await _context.Channels.FirstOrDefaultAsync(c => c.Name == channelName);

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

		/**
		* CM: Since we wont be saving any messages then this part of code is not needed anymore.
		*/

		// Get recent messages
		//var recentMessages = await _context.Messages
		//	.Include(m => m.User)
		//	.Where(m => m.Channel.Name == channelName)
		//	.OrderByDescending(m => m.SentAt)
		//	.Take(10)
		//	.ToListAsync();

		//foreach (var message in recentMessages.OrderBy(m => m.SentAt))
		//{
		//	await Clients.Caller.SendAsync("ReceiveMessage", message.User.Username, message.Content);
		//}

		await Clients.Group(channelName).SendAsync("ReceiveMessage", "System", $"{username} has joined the channel");
	}

	public async Task GetChannels()
	{
		var channels = await _context.Channels.Select(c => c.Name).ToListAsync();
		await Clients.Caller.SendAsync("ChannelList", channels);
	}
}
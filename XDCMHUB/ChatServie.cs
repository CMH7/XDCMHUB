using Microsoft.AspNetCore.SignalR.Client;
using System.Text;

namespace XDCMHUB;

public class ChatService : IAsyncDisposable
{
	private readonly HubConnection _hubConnection;
	private readonly string _username;
	private readonly string _password;
	private string _currentChannel = "General";

	public event Action<string, string> MessageReceived;
	public event Action<string> ErrorReceived;
	public event Action<string[]> ChannelListReceived;

	public ChatService(string serverUrl, string username, string password)
	{
		_username = username;
		_password = password;

		_hubConnection = new HubConnectionBuilder()
			.WithUrl(serverUrl, options =>
			{
				options.Headers.Add("Authorization",
					"Basic " + Convert.ToBase64String(
						Encoding.UTF8.GetBytes($"{username}:{password}")));
			})
			.WithAutomaticReconnect()
			.Build();

		_hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
		{
			message = Crypto.DecryptString(message, "buhmcdx");
            MessageReceived?.Invoke(user, message);
		});

		_hubConnection.On<string>("Error", message =>
		{
			ErrorReceived?.Invoke(message);
		});

		_hubConnection.On<string[]>("ChannelList", channels =>
		{
			ChannelListReceived?.Invoke(channels);
		});
	}

	public async Task StartAsync()
	{
		try
		{
			await _hubConnection.StartAsync();
			Console.WriteLine("Connected to chat server!");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error connecting to server: {ex.Message}");
			throw;
		}
	}

	public async Task SendMessageAsync(string message)
	{
		message = Crypto.EncryptString(message, "buhmcdx");

        await _hubConnection.InvokeAsync("SendMessage", message);
	}

	public async Task JoinChannelAsync(string channelName)
	{
		await _hubConnection.InvokeAsync("JoinChannel", channelName);
		_currentChannel = channelName;
	}

	public async Task GetChannelsAsync()
	{
		await _hubConnection.InvokeAsync("GetChannels");
	}

	public string GetCurrentChannel()
	{
		return _currentChannel;
	}

    #region Admin Side
	public async Task RegisterUser(string username, string password)
	{
		string passHash = BCrypt.Net.BCrypt.HashPassword(password);
		await _hubConnection.InvokeAsync("RegisterUser", username, passHash);
	}

	public async Task CreateChannel(string channel, string desc)
	{
		await _hubConnection.InvokeAsync("CreateChannel", channel, desc);
	}
    #endregion Admin Side

    public async ValueTask DisposeAsync()
	{
		await _hubConnection.DisposeAsync();
	}
}

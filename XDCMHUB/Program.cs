﻿namespace XDCMHUB;

class Program
{
	static async Task Main(string[] args)
	{
		Console.WriteLine("===== Chat Client =====");

		string serverUrl = GetInput("Enter server URL (default: http://localhost:5000/chathub):", "http://localhost:5000/chathub");
		string username = GetInput("Enter username:", null);
		string password = GetPassword("Enter password:");

		try
		{
			await using var chatService = new ChatService(serverUrl, username, password);

			chatService.MessageReceived += (user, message) =>
			{
				Console.ForegroundColor = user == "System" ? ConsoleColor.Yellow :
										 user == username ? ConsoleColor.Green : ConsoleColor.Cyan;
				Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{chatService.GetCurrentChannel()}] {user}: {message}");
				Console.ResetColor();
			};

			chatService.ErrorReceived += (error) =>
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"ERROR: {error}");
				Console.ResetColor();
			};

			chatService.ChannelListReceived += (channels) =>
			{
				Console.WriteLine("Available channels:");
				foreach (var channel in channels)
				{
					Console.WriteLine($"- {channel}");
				}
			};

			await chatService.StartAsync();

			bool running = true;
			while (running)
			{
				Console.ForegroundColor = ConsoleColor.White;
				var input = Console.ReadLine();
				Console.ResetColor();

				if (string.IsNullOrEmpty(input))
					continue;

				if (input.StartsWith("/"))
				{
					var parts = input.Split(' ', 2);
					var command = parts[0].ToLower();

					switch (command)
					{
						case "/exit":
							running = false;
							break;

						case "/join":
							if (parts.Length > 1)
							{
								await chatService.JoinChannelAsync(parts[1]);
							}
							else
							{
								Console.WriteLine("Usage: /join [channel name]");
							}
							break;

						case "/channels":
							await chatService.GetChannelsAsync();
							break;

						case "/help":
							ShowHelp();
							break;

						default:
							Console.WriteLine($"Unknown command: {command}");
							ShowHelp();
							break;
					}
				}
				else
				{
					await chatService.SendMessageAsync(input);
				}
			}
		}
		catch (Exception ex)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"Error: {ex.Message}");
			Console.ResetColor();
		}
	}

	static string GetInput(string prompt, string defaultValue)
	{
		Console.Write(prompt + " ");
		var input = Console.ReadLine();
		return string.IsNullOrWhiteSpace(input) ? defaultValue : input;
	}
	 
	static string GetPassword(string prompt)
	{
		Console.Write(prompt + " ");
		var password = new System.Text.StringBuilder();
		ConsoleKeyInfo key;

		do
		{
			key = Console.ReadKey(true);

			if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
			{
				password.Append(key.KeyChar);
				Console.Write("*");
			}
			else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
			{
				password.Remove(password.Length - 1, 1);
				Console.Write("\b \b");
			}
		} while (key.Key != ConsoleKey.Enter);

		Console.WriteLine();
		return password.ToString();
	}

	static void ShowHelp()
	{
		Console.WriteLine("Available commands:");
		Console.WriteLine("/join [channel] - Join a specific channel");
		Console.WriteLine("/channels - List available channels");
		Console.WriteLine("/help - Show this help message");
		Console.WriteLine("/exit - Exit the application");
	}
}
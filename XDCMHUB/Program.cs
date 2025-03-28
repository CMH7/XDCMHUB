using Spectre.Console;
using System.Xml.Linq;
using XDCMHUB.Components;

namespace XDCMHUB;

class Program
{
    public static Layout MainLayout { get; set; } = new();
    public static List<string> Messages { get; set; } = [];
    public static int ChatHistoryCount { get; set; } = 50;

    static async Task Main(string[] args)
    {
        string serverUrl = AnsiConsole.Prompt(new TextPrompt<string>("Server:").DefaultValue("http://192.168.2.32:9123/chathub"));
        string username = AnsiConsole.Prompt(new TextPrompt<string>("Username:"));
        string password = AnsiConsole.Prompt(new TextPrompt<string>("Password:").Secret());

        
        try
        {
            await using var chatService = new ChatService(serverUrl, username, password);

            ChatDisplayServices.CurrentChannel = "General";
            chatService.MessageReceived += (user, message) => ChatDisplayServices.MessageReceivedHandler(username, user, message, true);

            chatService.ErrorReceived += (error) => Messages.Add($"[darkred bold]ERROR:[/] [red]{error}[/]");

            chatService.ChannelListReceived += (channels) =>
            {
                Messages.Add("Available channels:");
                foreach (var channel in channels)
                {
                    Messages.Add($"- {channel}");
                }
            };

            await chatService.StartAsync();
            MainLayout = Layouts.MainLayout();
            bool running = true;
            while (running)
            {
                // Make sure to remove or release old chats...
                if(Messages.Count > ChatHistoryCount)
                {
                    Messages.Reverse();
                    Messages = [..Messages.Take(ChatHistoryCount)];
                    Messages.Reverse();
                }

                ReRenderMainLayout();

                // Get user input
                string? input = AnsiConsole.Ask<string>("[grey]>[/]");

                if (string.IsNullOrEmpty(input))
                    continue;

                input = input.Replace("[", "[[");
                input = input.Replace("]", "]]");

                if (input.StartsWith("/"))
                {
                    var parts = input.Split(' ', 3);
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
                                Messages.Add("[green bold]Usage:[/] [aqua]/join[/] [[channel name]]");
                            }
                            break;

                        case "/ccount":
                            if(parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
                            {
                                var confirmed = AnsiConsole.Prompt(
                                    new TextPrompt<bool>("This will delete old messages if ever. Do you want to continue?")
                                        .AddChoice(true)
                                        .AddChoice(false)
                                        .DefaultValue(true)
                                        .WithConverter(choice => choice ? "yes" : "no"));
                                if(confirmed) ChatHistoryCount = Convert.ToInt32(parts[1]);
                            }
                            else
                            {
                                Messages.Add("[green bold]Usage:[/] [aqua]/ccount[/] [[int]]");
                            }
                            break;

                        case "/channels":
                            await chatService.GetChannelsAsync();
                            break;

                        case "/help":
                            ShowHelp();
                            break;

                        case "/cls":
                            Messages.Clear();
                            break;

                        case "/regu":
                            await chatService.RegisterUser(parts[1], parts[2]);
                            break;

                        case "/newchan":
                            await chatService.CreateChannel(parts[1], parts[2]);
                            break;

                        case "/bcrypt":
                            string passwordHash = BCrypt.Net.BCrypt.HashPassword(parts[1]);
                            Messages.Add($"[pink]{passwordHash}[/]");
                            break;

                        default:
                            Messages.Add($"Unknown command: {command}");
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
            Messages.Add($"[bold darkred]Error:[/] [red]{ex.Message}[/]");
        }
    }

    static void ShowHelp()
    {
        Messages.Add("Available commands:");
        Messages.Add("[aqua]/join[/] [[channel]] - Join a specific channel");
        Messages.Add("[aqua]/channels[/] - List available channels");
        Messages.Add("[aqua]/help[/] - Show this help message");
        Messages.Add("[aqua]/ccount[/] - Chat history count");
        Messages.Add("[aqua]/cls[/] - Clears the console");
        Messages.Add("[aqua]/exit[/] - Exit the application");
    }

    // Method to render chat messages
    static Rows RenderChatMessages(List<string> messages)
    {
        List<Markup> markups = messages.ConvertAll(x => new Markup(x));
        return new Rows(markups);
    }

    public static void ReRenderMainLayout()
    {
        AnsiConsole.Clear();

        // Render chat messages in the chat area
        MainLayout["ChatArea"].Update(
            new Panel(RenderChatMessages(Messages))
                .Header($"[bold]Chat Messages[/] [bold teal]{ChatDisplayServices.CurrentChannel}[/]")
                .Expand());

        // Render input prompt in the top left section
        MainLayout["Bottom"].Update(
            new Panel("")
                .Header("[bold]Active Users[/]")
                .Expand());
        
        // Render input prompt in the bottom left section
        MainLayout["Bottom"].Update(
            new Panel("")
                .Header("[bold]Channels[/]")
                .Expand());

        // Render the updated layout
        AnsiConsole.Write(MainLayout);
    }
}
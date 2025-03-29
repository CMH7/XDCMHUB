using Spectre.Console;
using Spectre.Console.Rendering;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using XDCMHUB.Components;
using XDCMHUB.Services;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;

namespace XDCMHUB;

class Program
{
	#region Credentials
	public static string Username { get; set; }
	public static string Password { get; set; }
	#endregion Credentials

	public static Layout MainLayout { get; set; } = new();
    public static List<string> Messages { get; set; } = [];
    public static int ChatHistoryCount { get; set; } = 50;

	#region Active Users Area Data
	public static List<string> ActiveUsers { get; set; } = [];
	#endregion Active Users Area Data

	public static readonly Dictionary<string, string> BuiltInCommands = new() { 
		{ "/help", "Show this help message"},
		{ "/join [channel]", "Join a specific channel"},
		{ "/channels", "List available channels"},
		{ "/ccount [int]", "Chat history count"},
		{ "/mute", "Mute the chat"},
		{ "/bcrypt [string]", "Encrypt a message [EXP] Note: this is just on you"},
		{ "/cls", "Clears the console"},
		{ "/exit", "Exit the application"}
	};

    static async Task Main(string[] args)
    {
        string serverUrl = AnsiConsole.Prompt(new TextPrompt<string>("Server:").DefaultValue("http://192.168.2.32:9123/chathub"));
        Username = AnsiConsole.Prompt(new TextPrompt<string>("Username:"));
        Password = AnsiConsole.Prompt(new TextPrompt<string>("Password:").Secret());
        
        try
        {
            await using var chatService = new ChatService(serverUrl, Username, Password);

            ChatDisplayServices.CurrentChannel = "General";

            chatService.MessageReceived += (user, message) => ChatDisplayServices.MessageReceivedHandler(user, message, true);
            chatService.ErrorReceived += (error) => Messages.Add($"[darkred bold]ERROR:[/] [red]{error}[/]");
            chatService.GetActiveUsersReceived += (activeUsers) => ActiveUsers = [..activeUsers];

            chatService.ChannelListReceived += (channels) =>
            {
                RenderChannelList([.. channels]);
            };

            await chatService.StartAsync();
            MainLayout = Layouts.MainLayout();
            bool running = true;
            bool firstRender = true;
            while (running)
            {
                if (firstRender)
                {
                    ReRenderMainLayout();
                    firstRender = false;
                }

                // Make sure to remove or release old chats...
                if(Messages.Count > ChatHistoryCount)
                {
                    Messages.Reverse();
                    Messages = [..Messages.Take(ChatHistoryCount)];
                    Messages.Reverse();
                }

                // Get user input
                string? input = AnsiConsole.Ask<string>("");

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
                            Messages.Add("/help");
                            break;

                        case "/figlet":
							if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
                            {
								await chatService.SendMessageAsync(input);
							}
                            else
                            {
								Messages.Add("[green bold]Usage:[/] [aqua]/figlet[/] [[text]]");
							}
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
                            string passwordHash = BCrypt.Net.BCrypt.HashPassword(parts[1])
                                .Replace("[", "[[")
                                .Replace("]", "]]");
                            Messages.Add($"[pink]{passwordHash}[/]");
                            break;

                        case "/mute":
                            List<string> modes = ["Basic chats", "Mentions", "Totally ayoko makarinig ng notification :>"];
							var mode = AnsiConsole.Prompt(
	                            new SelectionPrompt<string>()
		                            .Title("Anong mode pre?")
									.AddChoices(modes));

                            if (mode == modes[0]) ChatDisplayServices.MuteChatNotification = true;
                            else if (mode == modes[1]) ChatDisplayServices.MuteMentionNotification = true;
                            else if (mode == modes[2]) ChatDisplayServices.MuteAllNotification = true;
							break;

                        default:
                            Messages.Add($"Unknown command: {command}");
                            Messages.Add("/help");
                            break;
                        }

                    if (parts[0] != "/figlet") ReRenderMainLayout();
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

    static Table RenderHelpTable()
    {
        Table table = new();

        table.AddColumn("Command");
        table.AddColumn("Description");

        BuiltInCommands.ToList().ForEach(cmd =>
            table.AddRow(
                $"[aqua]{cmd.Key.Replace("[", "[[").Replace("]", "]]")}[/]",
                cmd.Value.Replace("[", "[[").Replace("]", "]]")));

        return table;
    }

    static void RenderChannelList(List<string> channels)
    {
        Table table = new();
        table.AddColumn("Channel name");

        List<string> modChannels = [..channels.Select(x => x.Replace("[", "[[").Replace("]", "]]"))];

        modChannels.ForEach(chan => table.AddRow(chan));
        table.Expand();

        // Render chat messages in the chat area
        MainLayout["Others"]
            .Update(
                new Panel(table.Expand())
                    .Header($"[bold]Channel List[/]")
                    .Expand()
            )
            .Visible();
	}

    // Method to render chat messages
    static void RenderChatMessages()
    {
        List<IRenderable> toRenderComponents = [];
        Messages.ForEach(mess =>
        {
            var parts = mess.Split(" ", 2);

            if (parts[0] == "/help")
            {
				Grid grid = new();
				grid.AddColumn().AddColumn();
				grid.AddRow(new Markup("[yellow underline]System:[/]"), RenderHelpTable());
			    toRenderComponents.Add(grid);
			}
			else if (parts[1].Split(" ", 2)[0] == "[silver]/figlet")
            {
				var message = parts[1].Split(" ", 2)[1]
                .Replace("[blue]","")
                .Replace("[green]","")
                .Replace("[/]", "");
				Grid grid = new();
				grid.AddColumn().AddColumn();
				grid.AddRow(new Markup(parts[0]), new FigletText(message).Color(Color.RosyBrown));
			    toRenderComponents.Add(grid);
			}
			else
            {
				toRenderComponents.Add(new Markup(mess));
            }
		});

		// Render chat messages in the chat area
		MainLayout["ChatArea"].Update(
			new Panel(new Rows(toRenderComponents))
				.Header($"[bold]Chat Messages[/] [bold teal]{ChatDisplayServices.CurrentChannel}[/]")
				.Expand());
	}
    
    static Rows RenderActiveUsers(List<string> activeUsers)
    {
        List<Markup> markups = activeUsers.ConvertAll(x => new Markup(x));
        return new Rows(markups);
    }

    public static void ReRenderMainLayout()
    {
        AnsiConsole.Clear();
        RenderChatMessages();

		// Render input prompt in the top left section
		MainLayout["OtherUserArea"].Update(
            new Panel(RenderActiveUsers(ActiveUsers))
                .Header("[bold]Active Users[/]")
                .Expand());

        // Render the updated layout
        AnsiConsole.Write(MainLayout);
    }
}
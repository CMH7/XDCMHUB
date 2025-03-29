using System.Text.RegularExpressions;

namespace XDCMHUB.Services;

public class ChatDisplayServices
{
    public static string CurrentChannel { get; set; } = "General";

	#region Chat Notification Settings
	public static bool MuteChatNotification { get; set; }
	public static bool MuteMentionNotification { get; set; }
	public static bool MuteAllNotification { get; set; }
	#endregion Chat Notification Settings

	// Potential event handler structure
	public static void MessageReceivedHandler(string user, string message, bool? rerender = false)
    {
        DisplayMessage(user, Program.Username, message, CurrentChannel);

        if ((bool)rerender) Program.ReRenderMainLayout();

        // Notification logic
        if (message.Contains("@all") && !MuteMentionNotification && !MuteAllNotification)
            PlayAllMentionNotification();
        else if (message.Contains($"@{Program.Username}") && !MuteMentionNotification && !MuteAllNotification)
            PlayMentionedNotification();
        else /*if (!MuteChatNotification && !MuteAllNotification)*/
            PlaySoftChatNotification();
    }

    // Updated message display with robust color handling
    static void DisplayMessage(string user, string username, string message, string channel)
    {
        Program.Messages.Add($"{(user == username ? "[aqua]You[/]" : user == "System" ? $"[underline yellow]{user}[/]" : $"[purple_2]{user}[/]")}:{DisplayMessageWithMentions(message, username)}");
    }

    static string DisplayMessageWithMentions(string message, string username)
    {
        string finalMessage = "";

        // Regular expression to find mentions starting with @ 
        // removed case sensitivity
        MatchCollection mentions = Regex.Matches(message, @"@\w+", RegexOptions.IgnoreCase);
        int lastIndex = 0;

        foreach (Match mention in mentions)
        {
            finalMessage = $" [silver]{message.Substring(lastIndex, mention.Index - lastIndex)}[/][{(mention.Value.Trim('@') == username ? "green" : "blue")}]{mention.Value}[/]";

            // Update last processed index
            lastIndex = mention.Index + mention.Length;
        }

        // Print remaining text after last mention
        if (lastIndex < message.Length)
        {
            finalMessage = $"{finalMessage} [silver]{message.Substring(lastIndex)}[/]";
        }

        return finalMessage;
    }

    #region Notifications
    static void PlayMentionedNotification()
    {
        Random _random = new Random();

        // Base frequencies mimicking messenger's crisp sound
        int[] baseFrequencies = { 800, 1200, 1600 };

        // Robotic sound sequence
        foreach (int baseFreq in baseFrequencies)
        {
            // Add robotic variations
            int frequency = baseFreq + _random.Next(-50, 50);

            // Short, sharp beeps with metallic quality
            Console.Beep(frequency, 50);
            Thread.Sleep(30);
        }

        // Final robotic ping
        Console.Beep(1000, 100);
    }

    static void PlaySoftChatNotification()
    {
        // Super soft, brief notification
        Console.Beep(600, 30);  // Gentle, short ping
        Thread.Sleep(20);
        Console.Beep(700, 20);  // Subtle follow-up
    }

    static void PlayAllMentionNotification()
    {
        // Orchestral-inspired grand notification
        int[] elegantSequence = {
            800,   // Initial refined tone
            1000,  // Rising elegant pitch
            1200,  // Crescendo
            1500   // Refined high note
        };

        // Simulate a sophisticated, grand entrance
        foreach (int freq in elegantSequence)
        {
            // Smooth, controlled beeps
            Console.Beep(freq, 150);
            Thread.Sleep(50);
        }

        // Final majestic flourish
        Console.Beep(1200, 300);
    }
    #endregion Notifications
}

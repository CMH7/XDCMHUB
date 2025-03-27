using Microsoft.Toolkit.Uwp.Notifications;
//using Windows.UI.Notification;

namespace XDCMHUB;

public class DesktopNotificationManager
{
    // Application ID for notification registration
    private const string APP_ID = "YourChatApplicationID";

    /// <summary>
    /// Shows a desktop notification for a new chat message
    /// </summary>
    /// <param name="sender">Name of the message sender</param>
    /// <param name="message">Message content</param>
    /// <param name="channel">Chat channel</param>
    public static void ShowChatNotification(string sender, string message, string channel)
    {
        try
        {
            // Truncate message if too long
            string displayMessage = message.Length > 50
                ? message.Substring(0, 50) + "..."
                : message;

            // Create toast notification
            new ToastContentBuilder()
                .AddArgument("action", "viewConversation")
                .AddArgument("conversationId", channel)
                .AddText($"New message from {sender}", hintMaxLines: 1)
                .AddText(displayMessage, hintMaxLines: 2);
                //.Show(toast =>
                //{
                //    toast.Tag = "new-message";
                //    toast.Group = channel;
                //});
        }
        catch (Exception ex)
        {
            // Fallback logging if notification fails
            Console.WriteLine($"Notification error: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks and handles mentions specifically
    /// </summary>
    public static void HandleMentionNotification(string sender, string message, string username, string channel)
    {
        // Check if user is mentioned
        if (message.Contains($"@{username}") || message.Contains("@all"))
        {
            // Play sound for mentioned notification
            Console.Beep(1000, 200);

            // Show a more prominent notification
            new ToastContentBuilder()
                .AddArgument("action", "viewMention")
                .AddArgument("conversationId", channel)
                .AddText("You were mentioned!", hintMaxLines: 1)
                .AddText($"{sender}: {message}", hintMaxLines: 2);
                //.Show(toast =>
                //{
                //    toast.Tag = "mention";
                //    toast.Group = channel;
                //});
        }
        else
        {
            // Regular message notification
            ShowChatNotification(sender, message, channel);
        }
    }

    /// <summary>
    /// Initialize notification capabilities
    /// </summary>
    public static void InitializeNotifications()
    {
        try
        {
            // Ensure notification capabilities are set up
            //ToastNotificationManagerCompat.OnActivated += ToastNotificationManagerCompat_OnActivated;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Notification initialization error: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle notification click events
    /// </summary>
    //private static void ToastNotificationManagerCompat_OnActivated(ToastNotificationActivatedEventArgsCompat toastArgs)
    //{
    //    // Process notification click
    //    ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

    //    if (args.TryGetValue("action", out string action))
    //    {
    //        switch (action)
    //        {
    //            case "viewConversation":
    //                // Bring chat window to front or perform specific action
    //                Console.WriteLine("Conversation view requested");
    //                break;
    //            case "viewMention":
    //                // Handle mention-specific action
    //                Console.WriteLine("Mention view requested");
    //                break;
    //        }
    //    }
    //}
}

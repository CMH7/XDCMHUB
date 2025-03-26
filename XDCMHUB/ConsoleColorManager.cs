using System.Text.RegularExpressions;

namespace XDCMHUB;

public class ConsoleColorManager
{
    public static void ConfigureConsoleColors()
    {
        try
        {
            // Explicitly set console color mode
            Console.BackgroundColor = ConsoleColor.Black;

            // Verify and set foreground colors
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Color configuration error: {ex.Message}");
        }
    }

    // Robust color selection method
    public static void SetUserColor(string user, string username)
    {
        try
        {
            ConsoleColor selectedColor = user switch
            {
                "System" => ConsoleColor.Yellow,
                string u when u == username => ConsoleColor.Green,
                _ => ConsoleColor.Cyan  // Default case
            };

            Console.ForegroundColor = selectedColor;
        }
        catch
        {
            // Fallback to default color if specific color fails
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }

    // Alternative color method with more robust fallback
    public static void SafeSetColor(ConsoleColor color)
    {
        try
        {
            // List of known good colors as fallback
            ConsoleColor[] safeColors = {
                ConsoleColor.White,
                ConsoleColor.Gray,
                ConsoleColor.DarkGray
            };

            Console.ForegroundColor = color;
        }
        catch
        {
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }

    // Updated message display with robust color handling
    public static void DisplayMessage(string user, string username, string message, string channel)
    {
        try
        {
            // Color selection
            SetUserColor(user, username);

            Console.Write($"[{DateTime.Now:HH:mm:ss}] [{channel}] {user}: ");

            // Reset color for message body
            Console.ResetColor();

            // Display message with mention highlighting
            DisplayMessageWithMentions(message, username);
        }
        catch
        {
            // Completely fallback display if color setting fails
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{channel}] {user}: {message}");
        }
        finally
        {
            // Always reset color
            Console.ResetColor();
        }
    }

    // Potential event handler structure
    public static void MessageReceivedHandler(string currentUsername, string user, string message)
    {
        string currentChannel = "General"; // Replace with actual channel method

        DisplayMessage(user, currentUsername, message, currentChannel);

        // Notification logic
        if (message.Contains("@all"))
            PlayAllMentionNotification();
        else if (message.Contains($"@{currentUsername}"))
            PlayMentionedNotification();
        else
            PlaySoftChatNotification();
    }

    static void DisplayMessageWithMentions(string message, string username)
    {
        // Regular expression to find mentions starting with @
        MatchCollection mentions = Regex.Matches(message, @"@\w+");
        int lastIndex = 0;

        try
        {
            foreach (Match mention in mentions)
            {
                // Print text before the mention in default color
                Console.Write(message.Substring(lastIndex, mention.Index - lastIndex));

                // Check if mention matches current user
                if (mention.Value.Trim('@') == username)
                {
                    // Highlight user's own mention differently
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    // Standard mention color
                    Console.ForegroundColor = ConsoleColor.Blue;
                }

                // Write the mention
                Console.Write(mention.Value);
                Console.ResetColor();

                // Update last processed index
                lastIndex = mention.Index + mention.Length;
            }

            // Print remaining text after last mention
            if (lastIndex < message.Length)
            {
                Console.Write(message.Substring(lastIndex));
            }
        }
        catch
        {
            // Fallback to plain message display if highlighting fails
            Console.WriteLine(message);
        }
        finally
        {
            // Ensure a new line is always printed
            Console.WriteLine();
        }
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

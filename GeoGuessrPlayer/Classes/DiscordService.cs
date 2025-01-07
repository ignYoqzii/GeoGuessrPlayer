using DiscordRPC;
using System.Windows;

namespace GeoGuessrPlayer.Classes
{
    public class DiscordService
    {
        readonly static string clientId = "1324155732198686820";
        public static DiscordRpcClient DiscordClient = new(clientId);

        public static void InitializeDiscordRichPresence(string defaultStatus)
        {
            try
            {
                DiscordClient.Initialize();
                UpdatePresence("Playing GeoGuessr", defaultStatus);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing Discord Rich Presence: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Set or update the Discord Rich Presence
        public static void UpdatePresence(string details, string state)
        {
            DiscordClient.SetPresence(new RichPresence
            {
                Details = details,
                State = state,
                Timestamps = Timestamps.Now,
                Assets = new Assets
                {
                    LargeImageKey = "geoguessrplayer",
                    LargeImageText = "GeoGuessr Player",
                    SmallImageKey = "starzlogo",
                    SmallImageText = "By StarZ Team"
                },
                Buttons =
                [
                    new Button() 
                    { 
                        Label = "Download the App", 
                        Url = "https://github.com/ignYoqzii/GeoGuessrPlayer/releases", // Not showing and I don't know why...
                    }
                ]
            });
        }

        // Dispose of the Discord client
        public static void Dispose()
        {
            DiscordClient.Dispose();
        }
    }
}

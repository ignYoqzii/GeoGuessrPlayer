using DiscordRPC;

namespace GeoGuessrPlayer.Classes
{
    public class DiscordService
    {
        private readonly DiscordRpcClient discordClient;
        public DiscordService(string clientId)
        {
            discordClient = new DiscordRpcClient(clientId);
            discordClient.Initialize();
        }

        // Set or update the Discord Rich Presence
        public void UpdatePresence(string details, string state)
        {
            discordClient.SetPresence(new RichPresence
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
                }
            });
        }

        // Dispose of the Discord client
        public void Dispose()
        {
            discordClient.Dispose();
        }
    }
}

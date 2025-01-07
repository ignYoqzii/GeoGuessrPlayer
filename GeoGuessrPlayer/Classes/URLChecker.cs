namespace GeoGuessrPlayer.Classes
{
    public class URLChecker
    {
        // List of singleplayer URLs with API features
        public static readonly List<string> SingleplayerGameUrls =
        [
            "https://www.geoguessr.com/game/", // General singleplayer mode
            "https://www.geoguessr.com/explorer/", // Explorer mode
            "https://www.geoguessr.com/maps/", // Custom maps singleplayer mode
            "https://www.geoguessr.com/streaks/", // Streaks mode
        ];

        // List of multiplayer URLs with API features
        public static readonly List<string> MultiplayerGameUrls =
        [
            "https://www.geoguessr.com/duels/", // Multiplayer duels
            "https://www.geoguessr.com/team-duels/", // Team duels
            "https://www.geoguessr.com/battle-royale/", // Battle Royale mode
            "https://www.geoguessr.com/party/", // Party mode
            "https://www.geoguessr.com/live-challenge/", // Live Challenge mode //
        ];

        public static string GetGameModeFromUrl(string url)
        {
            if (url == "https://www.geoguessr.com/") return "GeoGuessr"; // "Playing GeoGuessr"

            if (url.Contains("/singleplayer")) return "Campaign";
            if (url.Contains("/maps/community")) return "Community Maps";
            if (url.Contains("/maps")) return "Classic Maps";
            if (url.Contains("/multiplayer/teams")) return "Ranked Team Duels";
            if (url.Contains("/multiplayer/unranked-teams")) return "Unranked Team Duels";
            if (url.Contains("/multiplayer/battle-royale-countries")) return "Battle Royale: Countries";
            if (url.Contains("/multiplayer/battle-royale-distance")) return "Battle Royale: Distance";
            if (url.Contains("/multiplayer")) return "Duels";
            if (url.Contains("/party")) return "In A Party";

            return "GeoGuessr"; // "Playing GeoGuessr" - Default Mode
        }
    }
}

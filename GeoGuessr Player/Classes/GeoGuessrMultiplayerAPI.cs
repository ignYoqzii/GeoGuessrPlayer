using Newtonsoft.Json;
using System.Net.Http;
using System.Windows;

namespace GeoGuessrPlayer.Classes
{
    // Class representing the duel mode game fetching details from an API
    public class GeoGuessrMultiplayerAPI
    {
        private static readonly HttpClient httpClient = new();

        // Function to fetch duel game info by using a token
        public static async Task<GeoGuessrMultiplayerInfo?> FetchMultiplayerInfoAsync(string gameToken, string ncfaCookie)
        {
            try
            {
                // Construct the API endpoint for multiplayer duel mode
                string apiUrl = $"https://game-server.geoguessr.com/api/duels/{gameToken}";

                // Create an HttpRequestMessage and set headers (including _ncfa cookie for authorization)
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                requestMessage.Headers.Add("Cookie", $"_ncfa={ncfaCookie}");

                // Send the request and get the response
                HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

                // Handle the response
                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    var multiplayerInfo = JsonConvert.DeserializeObject<GeoGuessrMultiplayerInfo>(responseData);
                    return multiplayerInfo;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching duel game info: {ex.Message}");
                return null;
            }
        }

        // Extract the game token from a URL to fetch duel game details
        public static string ExtractMultiplayerTokenFromUrl(string url)
        {
            return url[(url.LastIndexOf('/') + 1)..];
        }
    }

    // Class representing the game information for duel modes (multiplayer)
    public class GeoGuessrMultiplayerInfo
    {

        [JsonProperty("currentRoundNumber")]
        public int? CurrentRoundNumber { get; set; }

        [JsonProperty("options")]
        public GameOptions? Options { get; set; }

        [JsonProperty("movementOptions")]
        public MovementOptions? MovementOptions { get; set; }
    }

    public class GameOptions
    {
        [JsonProperty("isTeamDuels")]
        public bool? IsTeamDuels { get; set; }

        // Method to determine the game type
        public string GameType
        {
            get
            {
                // Return the appropriate game type based on IsTeamDuels flag
                return IsTeamDuels.HasValue && IsTeamDuels.Value ? "Team Duels" : "Duels";
            }
        }
    }

    public class MovementOptions
    {
        [JsonProperty("forbidMoving")]
        public bool? ForbidMoving { get; set; }

        [JsonProperty("forbidZooming")]
        public bool? ForbidZooming { get; set; }

        [JsonProperty("forbidRotating")]
        public bool? ForbidRotating { get; set; }

        public string Category
        {
            get
            {
                bool forbidMoving = ForbidMoving ?? false;
                bool forbidZooming = ForbidZooming ?? false;
                bool forbidRotating = ForbidRotating ?? false;

                // Determine category based on the settings
                if (!forbidMoving && !forbidZooming && !forbidRotating)
                {
                    return "Moving"; // Full movement enabled
                }
                else if (forbidMoving && forbidZooming && forbidRotating)
                {
                    return "No Move"; // No movement allowed
                }
                else if (forbidMoving && forbidZooming && !forbidRotating)
                {
                    return "NMPZ"; // No move, Panning allowed, Zooming not allowed
                }
                else
                {
                    return "Custom"; // For any other unexpected combination
                }
            }
        }
    }
}
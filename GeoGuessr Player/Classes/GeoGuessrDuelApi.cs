using Newtonsoft.Json;
using System.Net.Http;
using System.Windows;

namespace GeoGuessrPlayer.Classes
{
    // Class representing the duel mode game fetching details from an API
    public class GeoGuessrDuelApi
    {
        private static readonly HttpClient httpClient = new();

        // Function to fetch duel game info by using a token
        public static async Task<GeoGuessrDuelGameInfo?> FetchDuelGameInfoAsync(string gameToken, string ncfaCookie)
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
                if (((int)response.StatusCode) == 200)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    var duelGameInfo = JsonConvert.DeserializeObject<GeoGuessrDuelGameInfo>(responseData);
                    return duelGameInfo;
                }
                else
                {
                    MessageBox.Show($"Error: {response.StatusCode}");
                    string errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error Content: {errorContent}");
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
        public static string ExtractDuelGameTokenFromUrl(string url)
        {
            return url[(url.LastIndexOf('/') + 1)..];
        }
    }

    // Class representing the game information for duel modes (multiplayer)
    public class GeoGuessrDuelGameInfo
    {

        [JsonProperty("currentRoundNumber")]
        public int? CurrentRoundNumber { get; set; }

        [JsonProperty("options")]
        public GameOptions? Options { get; set; }
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

}

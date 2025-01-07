using Newtonsoft.Json;
using System.Net.Http;
using System.Windows;

namespace GeoGuessrPlayer
{
    // Only for singleplayer modes
    public class GeoGuessrSingleplayerAPI
    {
        private static readonly HttpClient httpClient = new();

        // Function to fetch the game info by using a token, and now includes _ncfa authorization
        public static async Task<GeoGuessrSingleplayerInfo?> FetchSingleplayerInfoAsync(string gameToken, string ncfaCookie)
        {
            try
            {
                // Create the URI with the game token
                string apiUrl = $"https://www.geoguessr.com/api/v3/games/{gameToken}";

                // Create an HttpRequestMessage and set headers (including _ncfa cookie)
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, apiUrl);

                // Add _ncfa cookie to request header for authorization
                requestMessage.Headers.Add("Cookie", $"_ncfa={ncfaCookie}");

                // Send request
                HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

                // Handle response
                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    var singleplayerInfo = JsonConvert.DeserializeObject<GeoGuessrSingleplayerInfo>(responseData);
                    return singleplayerInfo;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching game info: {ex.Message}");
                return null;
            }
        }

        // Extract game token from the URL
        public static string ExtractSingleplayerTokenFromUrl(string url)
        {
            return url[(url.LastIndexOf('/') + 1)..];
        }
    }

    // The complete class representing the API response (GameInfo)
    public class GeoGuessrSingleplayerInfo
    {

        [JsonProperty("type")]
        public string? Type { get; set; }

        public string GameType
        {
            get
            {
                // If Type is "standard", return "Singleplayer", otherwise "Unknown Game Type" for now until I figure it out
                return Type == "standard" ? "Singleplayer" : "Unknown Game Type";
            }
        }

        [JsonProperty("roundCount")]
        public int? RoundCount { get; set; }

        [JsonProperty("map")]
        public string? Map { get; set; }

        [JsonProperty("mapName")]
        public string? MapName { get; set; }

        [JsonProperty("round")]
        public int? Round { get; set; }

        [JsonProperty("player")]
        public Player? Player { get; set; }
    }

    public class Player
    {
        [JsonProperty("totalScore")]
        public TotalScore? totalScore { get; set; }

        [JsonProperty("nick")]
        public string? Nickname { get; set; }

        // This is useful for presenting total game stats
        public class TotalScore
        {
            [JsonProperty("amount")]
            public int? Amount { get; set; }
        }
    }
}
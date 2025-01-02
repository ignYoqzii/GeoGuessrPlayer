using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Web.WebView2.Core;
using GeoGuessrPlayer.Classes;

namespace GeoGuessrPlayer
{
    public partial class MainWindow : Window
    {
        private DiscordService? discordService;
        private string defaultstatus = "Exploring the World";
        private DispatcherTimer? urlCheckTimer;
        private readonly string defaultUrl = "https://www.geoguessr.com/";

        public MainWindow()
        {
            InitializeComponent();
            InitializeWebView();
            InitializeDiscordRichPresence();
            StartUrlCheckTimer();  // Starts the periodic URL checking timer
        }

        private async void InitializeWebView()
        {
            try
            {
                string userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GeoGuessrPlayer");
                var webViewEnvironment = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
                await WebView.EnsureCoreWebView2Async(webViewEnvironment);

                WebView.Source = new Uri(defaultUrl);
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing WebView: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Function to extract the _ncfa cookie from WebView2
        public async Task<string?> ExtractNcfaCookieAsync()
        {
            // Wait until the WebView2 environment is initialized
            if (WebView != null)
            {
                var cookieManager = WebView.CoreWebView2.CookieManager;
                var cookies = await cookieManager.GetCookiesAsync("https://www.geoguessr.com");

                // Find and return the _ncfa cookie
                foreach (var cookie in cookies)
                {
                    if (cookie.Name == "_ncfa")
                    {
                        return cookie.Value;  // Return the _ncfa cookie value
                    }
                }
            }
            return null;
        }

        private void InitializeDiscordRichPresence()
        {
            try
            {
                discordService = new DiscordService("1324155732198686820");
                discordService.UpdatePresence("Playing GeoGuessr", defaultstatus);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing Discord Rich Presence: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartUrlCheckTimer()
        {
            urlCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // Check every second
            };
            urlCheckTimer.Tick += OnUrlCheckTimerTick!;
            urlCheckTimer.Start();
        }

        private string previousMode = "GeoGuessr";
        private async void OnUrlCheckTimerTick(object sender, EventArgs e)
        {
            try
            {
                string url = WebView.Source.ToString();
                string? _ncfaToken = await ExtractNcfaCookieAsync();

                if (string.IsNullOrEmpty(_ncfaToken))
                {
                    discordService!.UpdatePresence("Error", "Failed to extract user token.");
                    return;
                }

                if (url.StartsWith("https://www.geoguessr.com/game/")) // Singleplayer mode
                {
                    await HandleSinglePlayerModeAsync(url, _ncfaToken);
                }
                else if (url.StartsWith("https://www.geoguessr.com/duels/") || url.StartsWith("https://www.geoguessr.com/team-duels/")) // Multiplayer duels modes
                {
                    await HandleMultiplayerModeAsync(url, _ncfaToken);
                }
                else
                {
                    string mode = GetGameModeFromUrl(url, previousMode);
                    previousMode = mode;
                    discordService!.UpdatePresence($"Playing {mode}", defaultstatus);
                }

                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error determining game mode: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task HandleSinglePlayerModeAsync(string url, string ncfaToken)
        {
            string gameToken = GeoGuessrGameApi.ExtractGameTokenFromUrl(url);

            if (string.IsNullOrEmpty(gameToken))
                return;

            var gameInfo = await GeoGuessrGameApi.FetchGameInfoAsync(gameToken, ncfaToken);
            if (gameInfo != null)
            {
                discordService!.UpdatePresence($"Playing {gameInfo.GameType}, logged in as {gameInfo.Player!.Nickname}",
                                                $"Round: {gameInfo.Round}/{gameInfo.RoundCount}, Score: {gameInfo.Player.totalScore!.Amount}, Map: {gameInfo.MapName}");
            }
        }

        private async Task HandleMultiplayerModeAsync(string url, string ncfaToken)
        {
            string gameToken = GeoGuessrDuelApi.ExtractDuelGameTokenFromUrl(url);

            if (string.IsNullOrEmpty(gameToken))
                return;

            var gameInfo = await GeoGuessrDuelApi.FetchDuelGameInfoAsync(gameToken, ncfaToken);
            if (gameInfo != null)
            {
                discordService!.UpdatePresence($"Playing {gameInfo.Options!.GameType}",
                                               $"Round: {gameInfo.CurrentRoundNumber}");
            }
        }

        private static string GetGameModeFromUrl(string url, string previousMode)
        {
            // Reset to default if exactly "https://www.geoguessr.com/"
            if (url == "https://www.geoguessr.com/")
            {
                return "Geoguessr"; // Default text or something more appropriate
            }

            // Check for known game modes
            if (url.Contains("/campaign")) return "Campaign";
            if (url.Contains("/maps/community")) return "Community Maps";
            if (url.Contains("/maps")) return "Classic Maps";
            if (url.Contains("/multiplayer/teams")) return "Ranked Team Duels";
            if (url.Contains("/multiplayer/unranked-teams")) return "Unranked Team Duels";
            if (url.Contains("/multiplayer/battle-royale-countries")) return "Battle Royale: Countries";
            if (url.Contains("/multiplayer/battle-royale-distance")) return "Battle Royale: Distance";
            if (url.Contains("/multiplayer")) return "Duels";

            // If the URL doesn't match any of the above conditions, return the previous mode.
            return previousMode;
        }


        private void UpdateButtonStates()
        {
            string currentUrl = WebView.Source.ToString();
            BackButton.IsEnabled = currentUrl != defaultUrl;
        }

        // "Back" button click
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (WebView.CanGoBack)
            {
                WebView.GoBack();
            }
        }

        // "Refresh" button click
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            WebView.Reload();
        }

        protected override void OnClosed(EventArgs e)
        {
            urlCheckTimer?.Stop();
            discordService?.Dispose();
            base.OnClosed(e);
        }
    }
}
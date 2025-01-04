using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Web.WebView2.Core;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Diagnostics;
using GeoGuessrPlayer.Classes;

namespace GeoGuessrPlayer
{
    public partial class MainWindow : Window
    {
        private DiscordService? discordService;
        private string defaultStatus = "Exploring the World";
        private readonly string defaultUrl = "https://www.geoguessr.com/";

        private DispatcherTimer? gameInfoTimer; // Timer for game info refresh
        private string? currentGameMode; // Keeps track of the current game mode
        private string? previousUrl;
        private bool pinState = false; // Default value (false = unchecked, true = checked)

        public MainWindow()
        {
            InitializeComponent();
            InitializeWebView();
            InitializeDiscordRichPresence();
            NotesManager.LoadNotes(NotesTextBox);
        }

        private async void InitializeWebView()
        {
            try
            {
                string userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GeoGuessrPlayer");
                var webViewEnvironment = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
                await WebView.EnsureCoreWebView2Async(webViewEnvironment);

                WebView.Source = new Uri(defaultUrl);
                URLTextBlock.Text = defaultUrl;
                UpdateButtonStates();

                WebView.CoreWebView2.SourceChanged += WebView_SourceChanged!;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing WebView: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<string?> ExtractNcfaCookieAsync()
        {
            if (WebView == null) return null;

            var cookieManager = WebView.CoreWebView2.CookieManager;
            var cookies = await cookieManager.GetCookiesAsync("https://www.geoguessr.com");

            foreach (var cookie in cookies)
            {
                if (cookie.Name == "_ncfa") return cookie.Value;
            }
            return null;
        }

        private void InitializeDiscordRichPresence()
        {
            try
            {
                discordService = new DiscordService("1324155732198686820");
                discordService.UpdatePresence("Playing GeoGuessr", defaultStatus);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing Discord Rich Presence: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void WebView_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            try
            {
                string url = WebView.Source.ToString();
                if (url == previousUrl) return; // Ignore unnecessary repeated URL changes

                URLTextBlock.Text = url;
                previousUrl = url; // Track the current URL

                string? ncfaToken = await ExtractNcfaCookieAsync();
                if (string.IsNullOrEmpty(ncfaToken))
                {
                    discordService?.UpdatePresence("Error", "Failed to extract user token.");
                    return;
                }

                // Stop the timer if moving away from a game mode
                if (currentGameMode != null && !url.StartsWith("https://www.geoguessr.com/game/") &&
                    !url.StartsWith("https://www.geoguessr.com/duels/") || url.StartsWith("https://www.geoguessr.com/team-duels/"))
                {
                    gameInfoTimer?.Stop();
                    currentGameMode = null;
                }

                // Handle game modes based on URL
                if (url.StartsWith("https://www.geoguessr.com/game/")) // Singleplayer modes
                {
                    currentGameMode = "Singleplayer";
                    StartGameInfoTimer(url, ncfaToken);
                }
                else if (url.StartsWith("https://www.geoguessr.com/duels/") || url.StartsWith("https://www.geoguessr.com/team-duels/")) // Multiplayer duels modes
                {
                    currentGameMode = "Multiplayer";
                    StartGameInfoTimer(url, ncfaToken);
                }
                else
                {
                    currentGameMode = null;
                    HandleOtherGameModes(url);
                }

                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error determining game mode: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Start a timer to periodically fetch game info
        private void StartGameInfoTimer(string url, string ncfaToken)
        {
            gameInfoTimer?.Stop();// Stop existing timer if it's already running

            gameInfoTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            gameInfoTimer.Tick += async (sender, e) => await OnGameInfoTimerTick(url, ncfaToken);
            gameInfoTimer.Start();
        }

        // Fetch game info every second for selected modes
        private async Task OnGameInfoTimerTick(string url, string ncfaToken)
        {
            if (currentGameMode == "Singleplayer")
            {
                await HandleSinglePlayerModeAsync(url, ncfaToken);
            }
            else if (currentGameMode == "Multiplayer")
            {
                await HandleMultiplayerModeAsync(url, ncfaToken);
            }
        }

        // Handle Singleplayer game mode
        private async Task HandleSinglePlayerModeAsync(string url, string ncfaToken)
        {
            string gameToken = GeoGuessrGameApi.ExtractGameTokenFromUrl(url);
            if (string.IsNullOrEmpty(gameToken)) return;

            var gameInfo = await GeoGuessrGameApi.FetchGameInfoAsync(gameToken, ncfaToken);
            if (gameInfo != null)
            {
                discordService?.UpdatePresence($"Playing {gameInfo.GameType}, logged in as {gameInfo.Player?.Nickname}",
                                               $"Round: {gameInfo.Round}/{gameInfo.RoundCount}, Score: {gameInfo.Player?.totalScore?.Amount}, Map: {gameInfo.MapName}");
            }
        }

        // Handle Multiplayer game mode (e.g. duels)
        private async Task HandleMultiplayerModeAsync(string url, string ncfaToken)
        {
            string gameToken = GeoGuessrDuelApi.ExtractDuelGameTokenFromUrl(url);
            if (string.IsNullOrEmpty(gameToken)) return;

            var duelgameInfo = await GeoGuessrDuelApi.FetchDuelGameInfoAsync(gameToken, ncfaToken);
            if (duelgameInfo != null)
            {
                discordService?.UpdatePresence($"Playing {duelgameInfo.Options?.GameType}",
                                               $"Round: {duelgameInfo.CurrentRoundNumber}, Category: {duelgameInfo.MovementOptions!.Category}");
            }
        }

        // Handle non-specific game modes
        private void HandleOtherGameModes(string url)
        {
            string mode = GetGameModeFromUrl(url);
            discordService?.UpdatePresence($"Playing {mode}", defaultStatus);
        }

        private static string GetGameModeFromUrl(string url)
        {
            if (url == "https://www.geoguessr.com/") return "GeoGuessr";

            if (url.Contains("/singleplayer")) return "Campaign";
            if (url.Contains("/maps/community")) return "Community Maps";
            if (url.Contains("/maps")) return "Classic Maps";
            if (url.Contains("/multiplayer/teams")) return "Ranked Team Duels";
            if (url.Contains("/multiplayer/unranked-teams")) return "Unranked Team Duels";
            if (url.Contains("/multiplayer/battle-royale-countries")) return "Battle Royale: Countries";
            if (url.Contains("/multiplayer/battle-royale-distance")) return "Battle Royale: Distance";
            if (url.Contains("/multiplayer")) return "Duels";

            return "Unknown Mode";
        }

        private void UpdateButtonStates()
        {
            string currentUrl = WebView.Source.ToString();
            BackButton.IsEnabled = currentUrl != defaultUrl;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (WebView.CanGoBack)
            {
                WebView.GoBack();
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            WebView.Reload();
        }

        protected override void OnClosed(EventArgs e)
        {
            NotesManager.SaveNotes(NotesTextBox);
            gameInfoTimer?.Stop();
            discordService?.Dispose();
            base.OnClosed(e);
        }

        private void NotesPage_MouseEnter(object sender, MouseEventArgs e)
        {
            if (pinState == true) { return; }
            ForwardIcon.Visibility = Visibility.Collapsed;
            NotesPage.BeginAnimation(WidthProperty, new DoubleAnimation(50, 500, TimeSpan.FromSeconds(0.3)));
            NotesTextBox.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3)));
            NotesPageInfo.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3)));
            NotesPageShortcuts.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3)));
        }

        private void NotesPage_MouseLeave(object sender, MouseEventArgs e)
        {
            if (pinState == true) { return; }
            ForwardIcon.Visibility = Visibility.Visible;
            NotesPage.BeginAnimation(WidthProperty, new DoubleAnimation(500, 50, TimeSpan.FromSeconds(0.3)));
            NotesTextBox.BeginAnimation(OpacityProperty, new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3)));
            NotesPageInfo.BeginAnimation(OpacityProperty, new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3)));
            NotesPageShortcuts.BeginAnimation(OpacityProperty, new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3)));
        }

        private void PinState_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (pinState == false)
            {
                pinState = true;
                PinState.Source = new BitmapImage(new Uri("/Resources/PinChecked.png", UriKind.Relative));
            }
            else 
            {
                pinState = false;
                PinState.Source = new BitmapImage(new Uri("/Resources/Pin.png", UriKind.Relative));
            }
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string query = SearchTextBox.Text;
                if (!string.IsNullOrWhiteSpace(query))
                {
                    string url = $"https://www.google.com/search?q={Uri.EscapeDataString(query)}";
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                    SearchTextBox.Clear();
                }
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchTextBox.Text))
            {
                SearchTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                SearchTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        // Handle the KeyDown event for RichTextBox
        private void NotesTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var richTextBox = sender as RichTextBox;
            if (richTextBox == null) return;

            // Check for Ctrl+B (Bold)
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.B)
            {
                NotesManager.ToggleBold(NotesTextBox);
                e.Handled = true; // Prevent the default behavior (like typing 'b')
            }

            // Check for Ctrl+I (Italic)
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.I)
            {
                NotesManager.ToggleItalic(NotesTextBox);
                e.Handled = true; // Prevent the default behavior (like typing 'i')
            }

            // Check for Shift+Plus (Increase font size)
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Shift && (e.Key == Key.OemPlus || e.Key == Key.Add))
            {
                NotesManager.IncreaseFontSize(NotesTextBox);
                e.Handled = true; // Prevent the default behavior (like typing '+')
            }

            // Check for Shift+Minus (Decrease font size)
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Shift && e.Key == Key.OemMinus)
            {
                NotesManager.DecreaseFontSize(NotesTextBox);
                e.Handled = true; // Prevent the default behavior (like typing '-')
            }
        }
    }
}
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TCGShop
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly string _dataFolder;
        private readonly string _usersFilePath;

        public MainWindow()
        {
            InitializeComponent();
            _dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TCGShop");
            _usersFilePath = Path.Combine(_dataFolder, "users.json");
        }

        private record User(string Username, string Password);

        private async Task<List<User>> LoadUsersAsync()
        {
            try
            {
                if (!Directory.Exists(_dataFolder)) Directory.CreateDirectory(_dataFolder);
                if (!File.Exists(_usersFilePath)) return new List<User>();

                using var stream = File.OpenRead(_usersFilePath);
                var users = await JsonSerializer.DeserializeAsync<List<User>>(stream);
                return users ?? new List<User>();
            }
            catch
            {
                return new List<User>();
            }
        }

        private async Task SaveUsersAsync(List<User> users)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            using var stream = File.Create(_usersFilePath);
            await JsonSerializer.SerializeAsync(stream, users, options);
        }

        private void SetStatus(string text, bool isError = true)
        {
            StatusTextBlock.Text = text;
            StatusTextBlock.Foreground = isError ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red) : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);
        }

        private async void CreateUser_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text?.Trim();
            var password = PasswordBox.Password ?? string.Empty;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                SetStatus("Enter username and password.");
                return;
            }

            var users = await LoadUsersAsync();
            if (users.Any(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)))
            {
                SetStatus("User already exists.");
                return;
            }

            users.Add(new User(username, password));
            await SaveUsersAsync(users);
            SetStatus("User created.", isError: false);
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text?.Trim();
            var password = PasswordBox.Password ?? string.Empty;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                SetStatus("Enter username and password.");
                return;
            }

            var users = await LoadUsersAsync();
            var match = users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase) && u.Password == password);
            if (match is null)
            {
                SetStatus("Invalid username or password.");
                return;
            }

            SetStatus("Login successful.", isError: false);

            var win = new BlankWindow();
            win.Activate();
        }
    }
}

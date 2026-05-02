using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace TCGShop
{
    public class BlankWindow : Window
    {
        public BlankWindow()
        {
            this.Title = "Menú";

            // Fondo oscuro igual al Login
            var rootGrid = new Grid();
            rootGrid.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 10, 22, 40)); // #0A1628

            rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // ── Tarjeta central ──────────────────────────────────────────
            var card = new Border
            {
                Width = 480,
                HorizontalAlignment = HorizontalAlignment.Center,
                CornerRadius = new CornerRadius(12),
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 15, 30, 58)),   // #0F1E3A
                BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 227, 1, 24)),   // #E30118
                BorderThickness = new Thickness(1)
            };
            Grid.SetRow(card, 1);

            var cardStack = new StackPanel();

            // ── Header ───────────────────────────────────────────────────
            var header = new Border
            {
                CornerRadius = new CornerRadius(12, 12, 0, 0),
                Padding = new Thickness(32, 22, 32, 18),
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 30, 49, 96))    // #1E3160
            };

            var headerContent = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, Spacing = 4 };

            var titleRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Spacing = 12 };

            var logo = new Image
            {
                Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new System.Uri("ms-appx:///Assets/logo-redbull.png")),
                Width = 200,
                Height = 200,
                VerticalAlignment = VerticalAlignment.Center
            };

            var brandStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Spacing = 2 };
            brandStack.Children.Add(new TextBlock { Text = "ORACLE", FontSize = 18, FontWeight = Microsoft.UI.Text.FontWeights.Bold, Foreground = new SolidColorBrush(Colors.White), CharacterSpacing = 80 });
            brandStack.Children.Add(new TextBlock { Text = "RED BULL", FontSize = 22, FontWeight = Microsoft.UI.Text.FontWeights.Bold, Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 227, 1, 24)), CharacterSpacing = 80 });
            brandStack.Children.Add(new TextBlock { Text = "RACING", FontSize = 11, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 201, 6)), CharacterSpacing = 120 });

            titleRow.Children.Add(logo);
            titleRow.Children.Add(brandStack);
            headerContent.Children.Add(titleRow);

            headerContent.Children.Add(new Border
            {
                Height = 2,
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(204, 227, 1, 24)),
                Margin = new Thickness(0, 8, 0, 0)
            });

            headerContent.Children.Add(new TextBlock
            {
                Text = "MENÚ PRINCIPAL",
                FontSize = 10,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 168, 196, 224)),
                HorizontalAlignment = HorizontalAlignment.Center,
                CharacterSpacing = 150,
                Margin = new Thickness(0, 4, 0, 0)
            });

            header.Child = headerContent;
            cardStack.Children.Add(header);

            // ── Cuerpo — botones del menú ────────────────────────────────
            var body = new StackPanel { Padding = new Thickness(32, 24, 32, 28), Spacing = 12 };

            body.Children.Add(new TextBlock
            {
                Text = "Selecciona una opción para continuar",
                FontSize = 13,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 168, 196, 224)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 6)
            });

            // Botón Ver Productos  — estilo outline amarillo
            var verProductos = CreateMenuButton("  Ver Productos", isPrimary: false);
            verProductos.Click += async (s, e) =>
            {
               var win = new ViewProductsWindow();
                win.Activate();
            };

            // Botón Registrar Producto — estilo sólido rojo (acción principal)
            var registrarProductos = CreateMenuButton("  Registrar Producto", isPrimary: true);
            registrarProductos.Click += (s, e) =>
            {
                var win = new RegisterProductWindow();
                win.Activate();
            };

    

            body.Children.Add(verProductos);
            body.Children.Add(registrarProductos);
        

            cardStack.Children.Add(body);

            // ── Franja roja decorativa ───────────────────────────────────
            cardStack.Children.Add(new Border
            {
                Height = 3,
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 227, 1, 24))
            });

            // ── Footer ───────────────────────────────────────────────────
            var footer = new Border
            {
                Padding = new Thickness(0, 10, 0, 10),
                CornerRadius = new CornerRadius(0, 0, 12, 12),
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 15, 30, 58))
            };
            footer.Child = new TextBlock
            {
                Text = "© 2025 Red Bull Racing · Todos los derechos reservados",
                FontSize = 11,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 74, 96, 128)),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            cardStack.Children.Add(footer);

            card.Child = cardStack;
            rootGrid.Children.Add(card);

            this.Content = rootGrid;
        }
        // ── Fábrica de botones ────────────────────────────────────────────
        // isPrimary = true  → rojo sólido (acción destacada)
        // isPrimary = false → borde amarillo transparente
        private Button CreateMenuButton(string text, bool isPrimary)
        {
            return new Button
            {
                Content = text,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 52,
                FontSize = 15,
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(0, 0, 0, 0),
                Background = isPrimary
                    ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 227, 1, 24))
                    : new SolidColorBrush(Colors.Transparent),
                BorderBrush = isPrimary
                    ? new SolidColorBrush(Colors.Transparent)
                    : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 201, 6)),
                BorderThickness = new Thickness(isPrimary ? 0 : 1),
                Foreground = isPrimary
                    ? new SolidColorBrush(Colors.White)
                    : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 201, 6))
            };
        }

        private Task ShowMessageAsync(UIElement host, string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Acción",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = (host as FrameworkElement)?.XamlRoot
            };

            // Fire and forget the dialog show. Awaiting IAsyncOperation may require WinRT extension methods
            _ = dialog.ShowAsync();
            return Task.CompletedTask;
        }

    }
}

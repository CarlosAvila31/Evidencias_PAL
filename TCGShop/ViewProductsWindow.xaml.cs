using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TCGShop
{
    // ── ViewModel expuesto al DataTemplate ──────────────────────────────
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Marca { get; set; }
        public string PrecioStr { get; set; }
        public string Existencias { get; set; }
        public string FechaStr { get; set; }
        public ImageSource ImageSource { get; set; }
        public SolidColorBrush RowBackground { get; set; }
        public SolidColorBrush StockColor { get; set; }
        public Visibility DescripcionVisible { get; set; }
    }

    // ── Code-behind ─────────────────────────────────────────────────────
    public sealed partial class ViewProductsWindow : Window
    {
        // Modelo JSON — mismo esquema que RegisterProductWindow
        private record Product
        {
            public int Id { get; init; }
            public string Nombre { get; init; }
            public string Descripcion { get; init; }
            public string Marca { get; init; }
            public decimal Precio { get; init; }
            public int Existencias { get; init; }
            public DateTime FechaLanzamiento { get; init; }
            public string Imagen { get; init; }
        }

        private readonly string _productsFilePath;
        private readonly string _imagesFolder;

        // Lista maestra (sin filtrar)
        private List<Product> _allProducts = new();

        public ViewProductsWindow()
        {
            InitializeComponent();

            var dataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TCGShop");
            _productsFilePath = Path.Combine(dataFolder, "products.json");
            _imagesFolder = Path.Combine(dataFolder, "Images");

            _ = LoadAsync();
        }

        // ── Carga inicial ────────────────────────────────────────────────
        private async Task LoadAsync()
        {
            _allProducts = await ReadProductsJsonAsync();
            ProductCountLabel.Text = $"{_allProducts.Count} producto{(_allProducts.Count != 1 ? "s" : "")}";
            ApplyFilterAndSort();
        }

        // ── Búsqueda en tiempo real ──────────────────────────────────────
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
            => ApplyFilterAndSort();

        // ── Cambio de ordenamiento ───────────────────────────────────────
        private void SortCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ApplyFilterAndSort();

        // ── Filtrar + ordenar + renderizar ───────────────────────────────
        private void ApplyFilterAndSort()
        {
            // Guardas: los controles pueden no existir aún si el evento
            // se dispara durante InitializeComponent (ej. ComboBox con IsSelected)
            if (EmptyState is null || EmptyLabel is null ||
                ProductsListView is null) return;

            var query = SearchBox?.Text?.Trim().ToLowerInvariant() ?? string.Empty;

            IEnumerable<Product> filtered = _allProducts;

            if (!string.IsNullOrEmpty(query))
            {
                filtered = filtered.Where(p =>
                    (p.Nombre ?? "").ToLowerInvariant().Contains(query) ||
                    (p.Marca ?? "").ToLowerInvariant().Contains(query) ||
                    (p.Descripcion ?? "").ToLowerInvariant().Contains(query));
            }

            var sortIndex = SortCombo?.SelectedIndex ?? 0;
            filtered = sortIndex switch
            {
                1 => filtered.OrderBy(p => p.Nombre),
                2 => filtered.OrderBy(p => p.Precio),
                3 => filtered.OrderByDescending(p => p.Precio),
                4 => filtered.OrderByDescending(p => p.Existencias),
                _ => filtered.OrderBy(p => p.Id)
            };

            var result = filtered.ToList();

            // Estado vacío
            var isEmpty = result.Count == 0;
            EmptyState.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;
            ProductsListView.Visibility = isEmpty ? Visibility.Collapsed : Visibility.Visible;
            EmptyLabel.Text = string.IsNullOrEmpty(query)
                ? "No hay productos registrados aún."
                : $"Sin resultados para \"{query}\".";



            // Convertir a ViewModels y asignar a la lista
            ProductsListView.ItemsSource = result
                .Select((p, i) => ToViewModel(p, i))
                .ToList();
        }

        // ── Conversión Product → ProductViewModel ────────────────────────
        private ProductViewModel ToViewModel(Product p, int rowIndex)
        {
            // Color de fondo alternado
            var bgColor = rowIndex % 2 == 0
                ? Windows.UI.Color.FromArgb(255, 15, 30, 58)    // #0F1E3A
                : Windows.UI.Color.FromArgb(255, 18, 36, 70);   // un tono más claro

            // Color de stock: verde / amarillo / rojo
            var stockColor = p.Existencias > 10
                ? Windows.UI.Color.FromArgb(255, 100, 220, 130)
                : p.Existencias > 0
                    ? Windows.UI.Color.FromArgb(255, 255, 180, 50)
                    : Windows.UI.Color.FromArgb(255, 227, 80, 80);

            // Imagen
            ImageSource imgSource = null;
            if (!string.IsNullOrEmpty(p.Imagen))
            {
                var fullPath = Path.Combine(_imagesFolder, p.Imagen);
                if (File.Exists(fullPath))
                    imgSource = new BitmapImage(new Uri(fullPath));
            }

            return new ProductViewModel
            {
                Id = p.Id,
                Nombre = p.Nombre ?? "—",
                Descripcion = p.Descripcion ?? string.Empty,
                Marca = p.Marca ?? "—",
                PrecioStr = $"${p.Precio:N2}",
                Existencias = p.Existencias.ToString(),
                FechaStr = p.FechaLanzamiento.ToString("dd/MM/yyyy"),
                ImageSource = imgSource,
                RowBackground = new SolidColorBrush(bgColor),
                StockColor = new SolidColorBrush(stockColor),
                DescripcionVisible = string.IsNullOrEmpty(p.Descripcion)
                    ? Visibility.Collapsed
                    : Visibility.Visible
            };
        }

        // ── Lectura del JSON ─────────────────────────────────────────────
        private async Task<List<Product>> ReadProductsJsonAsync()
        {
            try
            {
                if (!File.Exists(_productsFilePath)) return new();
                using var stream = File.OpenRead(_productsFilePath);
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var items = await JsonSerializer.DeserializeAsync<List<Product>>(stream, opts);
                return items ?? new();
            }
            catch
            {
                return new();
            }
        }
    }
}
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Windows.Storage;

namespace TCGShop
{
    public sealed partial class RegisterProductWindow : Window
    {
        private readonly string _dataFolder;
        private readonly string _productsFilePath;
        private readonly string _imagesFolder;

        public RegisterProductWindow()
        {
            this.InitializeComponent();

            _dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TCGShop");
            _productsFilePath = Path.Combine(_dataFolder, "products.json");
            _imagesFolder = Path.Combine(_dataFolder, "Images");

            // Set default date
            ReleaseDatePicker.Date = DateTimeOffset.Now;
        }

        private async void Browse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var picker = new FileOpenPicker();
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".gif");

                var hwnd = WindowNative.GetWindowHandle(this);
                InitializeWithWindow.Initialize(picker, hwnd);

                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    ImagePathTextBox.Text = file.Path ?? file.Name;
                }
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = "Error al seleccionar la imagen: " + ex.Message;
            }
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            StatusTextBlock.Text = string.Empty;

            var name = NameTextBox.Text?.Trim();
            var description = DescriptionTextBox.Text?.Trim();
            var brand = BrandTextBox.Text?.Trim();
            if (string.IsNullOrEmpty(name)) { StatusTextBlock.Text = "Nombre es obligatorio."; return; }
            if (string.IsNullOrEmpty(brand)) { StatusTextBlock.Text = "Marca es obligatoria."; return; }

            if (!decimal.TryParse(PriceTextBox.Text?.Trim(), out var price) || price < 0)
            {
                StatusTextBlock.Text = "Precio inválido. Debe ser un número mayor o igual a 0."; return;
            }

            if (!int.TryParse(StockTextBox.Text?.Trim(), out var stock) || stock < 0)
            {
                StatusTextBlock.Text = "Existencias inválidas. Debe ser un número entero mayor o igual a 0."; return;
            }

            var releaseDate = ReleaseDatePicker.Date.DateTime;

            try
            {
                if (!Directory.Exists(_dataFolder)) Directory.CreateDirectory(_dataFolder);
                if (!Directory.Exists(_imagesFolder)) Directory.CreateDirectory(_imagesFolder);
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = "No se pudo crear carpeta de datos: " + ex.Message; return;
            }

            var products = await LoadProductsAsync();
            var nextId = products.Any() ? products.Max(p => p.Id) + 1 : 1;

            string imageFileName = null;
            var selectedPath = ImagePathTextBox.Text;
            if (!string.IsNullOrEmpty(selectedPath))
            {
                try
                {
                    if (File.Exists(selectedPath))
                    {
                        var ext = Path.GetExtension(selectedPath);
                        imageFileName = $"{Guid.NewGuid()}{ext}";
                        var dest = Path.Combine(_imagesFolder, imageFileName);
                        File.Copy(selectedPath, dest, overwrite: true);
                    }
                    else
                    {
                        imageFileName = Path.GetFileName(selectedPath);
                    }
                }
                catch
                {
                    imageFileName = Path.GetFileName(selectedPath);
                }
            }

            var product = new Product
            {
                Id = nextId,
                Nombre = name,
                Descripcion = description,
                Marca = brand,
                Precio = price,
                Existencias = stock,
                FechaLanzamiento = releaseDate,
                Imagen = imageFileName
            };

            products.Add(product);

            try
            {
                await SaveProductsAsync(products);
                StatusTextBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Green);
                StatusTextBlock.Text = $"Producto registrado con Id {product.Id}.";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
                StatusTextBlock.Text = "Error al guardar producto: " + ex.Message;
            }
        }

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

        private async Task<List<Product>> LoadProductsAsync()
        {
            try
            {
                if (!File.Exists(_productsFilePath)) return new List<Product>();
                using var stream = File.OpenRead(_productsFilePath);
                var items = await JsonSerializer.DeserializeAsync<List<Product>>(stream);
                return items ?? new List<Product>();
            }
            catch
            {
                return new List<Product>();
            }
        }

        private async Task SaveProductsAsync(List<Product> products)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            using var stream = File.Create(_productsFilePath);
            await JsonSerializer.SerializeAsync(stream, products, options);
        }
    }
}

using SaleBootsApp.AddProducts;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SaleBootsApp
{
    /// <summary>
    /// Логика взаимодействия для ProductListPage.xaml
    /// </summary>
    public partial class ProductListPage : Page
    {
        // Поле для хранения полного списка всех загруженных товаров
        private List<ProductViewModel> _products;

        public ProductListPage()
        {
            InitializeComponent();
            LoadUserInfo();
            SetupManagerControls();
            LoadFilterData();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }

        private void LoadProducts()
        {
            try
            {
                using (var db = new DB_SP_SaleBootsEntities())
                {
                    var rawProducts = db.Products
                        .Include(p => p.Categoryes)
                        .Include(p => p.Manufactures)
                        .Include(p => p.Suppliers)   
                        .Include(p => p.Units)
                        .ToList();


                    _products = rawProducts
                        .Select(p => new ProductViewModel
                        {
                            ProductArticle = p.Article,
                            ProductName = p.ProductName ?? "",
                            Description = p.Description ?? "",
                            CategoryName = p.Categoryes.CategoryName,
                            ManufacturerName = p.Manufactures.ManufacturerName,
                            SupplierName = p.Suppliers.SupplierName,
                            Discount = (decimal)p.Discount,
                            QuantityInStock = (int)p.QuantityInStock,
                            UnitsName = p.Units.UnitsName,
                            PhotoPath = p.Photo ?? "",
                            OriginalPrice = (decimal)p.Price,


                            FinalPrice = ProductViewModel.CalculateFinalPrice((decimal)p.Price, (decimal)p.Discount)
                        })
                        .ToList();

                    ApplyFilterSortAndSearch();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных о товарах: {ex.Message}", "Ошибка БД");
            }
        }

        private void LoadUserInfo()
        {
            // Отображение ФИО в правом верхнем углу
            if (CurrentUser.Instance != null)
            {
                UserInfoTextBlock.Text = $"{CurrentUser.Instance.FullName}";
            }
            else
            {
                UserInfoTextBlock.Text = "Неизвестный пользователь";
            }
        }

        private void SetupManagerControls()
        {
            if (CurrentUser.Instance == null || CurrentUser.Instance.RoleId > 2)
            {
                ManagerPanel.Visibility = Visibility.Collapsed;
                return;
            }

            ManagerPanel.Visibility = Visibility.Visible;

            if (CurrentUser.Instance.RoleId == 1)
            {
                AdminButtonsPanel.Visibility = Visibility.Visible;
            }
            else
            {
                AdminButtonsPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser.Logout();

            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService.Navigate(new AuthPage());
            }
        }

        private void LoadFilterData()
        {
            try
            {
                using (var db = new DB_SP_SaleBootsEntities())
                {

                    var suppliers = db.Suppliers.Select(c => c.SupplierName).ToList();
                    suppliers.Insert(0, "Все поставщики");

                    SupplierFilterComboBox.ItemsSource = suppliers;
                    SupplierFilterComboBox.SelectedIndex = 0;

                    SortComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных фильтров: {ex.Message}", "Ошибка БД");
            }
        }

        private void ApplyFilterSortAndSearch()
        {
            if (_products == null) return;

            IEnumerable<ProductViewModel> filteredProducts = _products;

            //1. ФИЛЬТРАЦИЯ по Поставщику
            string selectedSupplier = SupplierFilterComboBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedSupplier) && selectedSupplier != "Все поставщики")
            {
                filteredProducts = filteredProducts.Where(p => p.SupplierName == selectedSupplier);
            }

            //2. ПОИСК по Наименованию/Описанию
            string searchText = SearchTextBox.Text.ToLower();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredProducts = filteredProducts.Where(p =>
                    (p.ProductName ?? "").ToLower().Contains(searchText) ||
                    (p.Description ?? "").ToLower().Contains(searchText));
            }

            // 3. СОРТИРОВКА
            int sortIndex = SortComboBox.SelectedIndex;

            if (sortIndex == 1) // По возрастанию количества
            {
                filteredProducts = filteredProducts.OrderBy(p => p.QuantityInStock);
            }
            else if (sortIndex == 2) // По убыванию количества
            {
                filteredProducts = filteredProducts.OrderByDescending(p => p.QuantityInStock);
            }


            ProductListView.ItemsSource = filteredProducts.ToList();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditProductPage());
        }

        private void ProductListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ProductListView.SelectedItem is ProductViewModel selectedProductViewModel)
            {
                try
                {
                    using (var db = new DB_SP_SaleBootsEntities())
                    {
                        // Находим полную сущность Product в БД по артикулу (Article)
                        var productToEdit = db.Products
                            .Include(p => p.Categoryes)
                            .Include(p => p.Manufactures)
                            .Include(p => p.Suppliers)
                            .Include(p => p.Units)
                            .FirstOrDefault(p => p.Article == selectedProductViewModel.ProductArticle);

                        if (productToEdit != null)
                        {
                            NavigationService.Navigate(new AddEditProductPage(productToEdit));
                        }
                        else
                        {
                            MessageBox.Show("Не удалось найти полную информацию о товаре.", "Ошибка");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке товара для редактирования: {ex.Message}", "Ошибка БД");
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductListView.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар для удаления.", "Ошибка");
                return;
            }

            if (ProductListView.SelectedItem is ProductViewModel productToDelete)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить товар: {productToDelete.ProductName} (Артикул: {productToDelete.ProductArticle})?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new DB_SP_SaleBootsEntities())
                        {
                            var productEntity = db.Products
                                .FirstOrDefault(p => p.Article == productToDelete.ProductArticle);

                            if (productEntity != null)
                            {
                                db.Products.Remove(productEntity);
                                db.SaveChanges();

                                MessageBox.Show("Товар успешно удален.", "Успех");

                                LoadProducts();
                            }
                            else
                            {
                                MessageBox.Show("Товар не найден в базе данных.", "Ошибка удаления");
                            }
                        }
                    }
                    catch (System.Data.Entity.Infrastructure.DbUpdateException dbEx)
                    {
                        // Обработка ошибки, если товар связан с другими таблицами (например, в заказах)
                        MessageBox.Show($"Невозможно удалить товар. Возможно, он используется в других записях (например, в заказах).", "Ошибка целостности данных");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Произошла ошибка при удалении товара: {ex.Message}", "Ошибка БД");
                    }
                }
            }
        }

        private void FilterSortSearch_Changed(object sender, TextChangedEventArgs e)
        {
            ApplyFilterSortAndSearch();
        }

        private void SupplierFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilterSortAndSearch();
        }

        private void SortComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilterSortAndSearch();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilterSortAndSearch();
        }
    }
}

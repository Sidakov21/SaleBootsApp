using System;
using System.Collections.Generic;
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
using System.Data.Entity;

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
                            ProductName = p.ProductName,
                            Description = p.Description,
                            CategoryName = p.Categoryes.CategoryName,
                            ManufacturerName = p.Manufactures.ManufacturerName,
                            SupplierName = p.Suppliers.SupplierName,
                            Discount = (decimal)p.Discount,
                            QuantityInStock = (int)p.QuantityInStock,
                            UnitsName = p.Units.UnitsName,
                            PhotoPath = p.Photo,
                            OriginalPrice = (decimal)p.Price,
                            // Вычисляем конечную цену прямо в запросе
                            FinalPrice = ProductViewModel.CalculateFinalPrice((decimal)p.Price, (decimal)p.Discount)
                        })
                        .ToList();

                    // Вызываем фильтрацию/сортировку, чтобы обновить ListView
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
                UserInfoTextBlock.Text = $"{CurrentUser.Instance.FullName} ({CurrentUser.Instance.RoleName})";
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

        private void LoadFilterData()
        {
            try
            {
                using (var db = new DB_SP_SaleBootsEntities())
                {
                    var suppliers = db.Suppliers.Select(c => c.SupplierName).ToList();
                    suppliers.Insert(0, "Все поставщики"); // Добавляем опцию "Все"

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

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser.Logout();

            if (NavigationService.CanGoBack)
            {
                // Если AuthPage в истории, можно просто вернуться
                NavigationService.GoBack();
            }
            else
            {
                // Иначе создаем новую страницу авторизации
                NavigationService.Navigate(new AuthPage());
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

            // 2. ПОИСК по Наименованию/Описанию
            string searchText = SearchTextBox.Text.ToLower();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredProducts = filteredProducts.Where(p =>
                    p.ProductName.ToLower().Contains(searchText) ||
                    p.Description.ToLower().Contains(searchText));
            }

            // 3. СОРТИРОВКА по Количеству на складе
            int sortIndex = SortComboBox.SelectedIndex;
            if (sortIndex == 1)
            {
                filteredProducts = filteredProducts.OrderBy(p => p.QuantityInStock);
            }
            else if (sortIndex == 2)
            {
                filteredProducts = filteredProducts.OrderByDescending(p => p.QuantityInStock);
            }

            ProductListView.ItemsSource = filteredProducts.ToList();
        }

        private void FilterSortSearch_Changed(object sender, TextChangedEventArgs e)
        {
            ApplyFilterSortAndSearch();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Будет реализована страница добавления/редактирования товара.", "Функционал Администратора");
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductListView.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар для удаления.", "Ошибка");
                return;
            }

            MessageBox.Show("Будет реализована логика удаления товара из БД.", "Функционал Администратора");
        }

        private void SupplierFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilterSortAndSearch();
        }

        private void SortComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilterSortAndSearch();
        }
    }
}

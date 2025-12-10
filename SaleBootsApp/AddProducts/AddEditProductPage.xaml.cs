using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
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

namespace SaleBootsApp.AddProducts
{
    /// <summary>
    /// Логика взаимодействия для AddEditProductPage.xaml
    /// </summary>
    public partial class AddEditProductPage : Page
    {
        private DB_SP_SaleBootsEntities _db = new DB_SP_SaleBootsEntities();
        private Products _currentProduct;
        private bool _isNewProduct;

        public AddEditProductPage()
        {
            InitializeComponent();

            _currentProduct = new Products() { Price = 0, Discount = 0, QuantityInStock = 0 }; // Инициализация
            _isNewProduct = true;
            this.DataContext = _currentProduct;
            PageTitle.Text = "Добавление нового товара";
            LoadReferenceData();
        }

        public AddEditProductPage(Products productToEdit)
        {
            InitializeComponent();

            _currentProduct = productToEdit;

            _db.Products.Attach(_currentProduct);
            _db.Entry(_currentProduct).State = System.Data.Entity.EntityState.Modified;

            _isNewProduct = false;
            this.DataContext = _currentProduct;

            PageTitle.Text = $"Редактирование товара: {_currentProduct.ProductName}";
            LoadReferenceData();

            // Запрещаем редактирование артикула для существующего товара
            ArticleTextBox.IsReadOnly = true;
        }

        private void LoadReferenceData()
        {
            try
            {
                // Загружаем данные для ComboBox'ов
                CategoryComboBox.ItemsSource = _db.Categoryes.ToList();
                ManufacturerComboBox.ItemsSource = _db.Manufactures.ToList();
                SupplierComboBox.ItemsSource = _db.Suppliers.ToList();
                UnitsComboBox.ItemsSource = _db.Units.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки справочных данных: {ex.Message}", "Ошибка БД");
            }
        }

        private void SelectPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Определяем путь к папке для хранения фотографий товаров
                    string appDir = AppDomain.CurrentDomain.BaseDirectory;
                    string photoDir = System.IO.Path.Combine(appDir, "Resources", "Продукты");

                    // 1. Создаем папку, если ее нет
                    if (!System.IO.Directory.Exists(photoDir))
                    {
                        System.IO.Directory.CreateDirectory(photoDir);
                    }

                    string fileName = System.IO.Path.GetFileName(openFileDialog.FileName);
                    string destinationPath = System.IO.Path.Combine(photoDir, fileName);
                     
                    // 2. Копируем файл в папку приложения (с перезаписью)
                    System.IO.File.Copy(openFileDialog.FileName, destinationPath, true);

                    // 3. Сохраняем ТОЛЬКО имя файла в свойстве Photo для БД
                    _currentProduct.Photo = fileName;

                    // Обновляем отображение в TextBox (для пользователя)
                    PhotoPathTextBox.Text = fileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при копировании файла: {ex.Message}", "Ошибка");
                }
            }
        }

        private bool ValidateForm()
        {
            // 1. Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(_currentProduct.Article) ||
                string.IsNullOrWhiteSpace(_currentProduct.ProductName) ||
                _currentProduct.CategoryID == null || _currentProduct.ManufacturerID == null ||
                _currentProduct.SupplierID == null || _currentProduct.UnitsID == null)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля (отмеченные *).", "Ошибка валидации");
                return false;
            }

            // 2. Проверка числовых полей
            if (_currentProduct.Price == null || _currentProduct.Price < 0)
            {
                MessageBox.Show("Пожалуйста, введите корректную цену (>= 0).", "Ошибка валидации");
                return false;
            }
            if (_currentProduct.Discount == null || _currentProduct.Discount < 0 || _currentProduct.Discount > 100)
            {
                MessageBox.Show("Скидка должна быть в пределах от 0 до 100%.", "Ошибка валидации");
                return false;
            }
            if (_currentProduct.QuantityInStock == null || _currentProduct.QuantityInStock < 0)
            {
                MessageBox.Show("Пожалуйста, введите корректное количество на складе (>= 0).", "Ошибка валидации");
                return false;
            }

            return true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            try
            {
                if (_isNewProduct)
                {
                    // Проверка на уникальность Артикула
                    if (_db.Products.Any(p => p.Article == _currentProduct.Article))
                    {
                        MessageBox.Show("Товар с таким Артикулом уже существует.", "Ошибка сохранения");
                        return;
                    }

                    _db.Products.Add(_currentProduct);
                }

                _db.SaveChanges();
                MessageBox.Show("Данные о товаре успешно сохранены.", "Успех");

                NavigationService.GoBack();
            }
            catch (DbEntityValidationException dbEx)
            {
                // Обработка ошибок валидации, если EF не принял данные (напр., нарушение длины строки)
                var errorMessages = dbEx.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage);
                var fullErrorMessage = string.Join("; ", errorMessages);
                MessageBox.Show($"Ошибка валидации данных: {fullErrorMessage}", "Ошибка сохранения");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения данных: {ex.Message}", "Ошибка БД");
            }
        }

        private void GoBackButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}

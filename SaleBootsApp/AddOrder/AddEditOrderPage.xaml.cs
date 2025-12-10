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

namespace SaleBootsApp.AddOrder
{
    /// <summary>
    /// Логика взаимодействия для AddEditOrderPage.xaml
    /// </summary>
    public partial class AddEditOrderPage : Page
    {
        private DB_SP_SaleBootsEntities _db = new DB_SP_SaleBootsEntities();
        public Orders CurrentOrder { get; set; }
        private bool _isNewOrder;

        public string PageTitle { get; set; }
        // ВРЕМЕННОЕ ПОЛЕ для привязки XAML, так как в сущности Orders нет OrderArticle
        public string CurrentOrderArticle { get; set; }

        public AddEditOrderPage()
        {
            InitializeComponent();
            PageTitle = "Добавление нового заказа";

            _isNewOrder = true;
            // Создаем новый заказ с датой сегодня и начальным статусом
            CurrentOrder = new Orders
            {
                OrderDate = DateTime.Now,
                DeliveryDate = DateTime.Now.AddDays(3),
                UsersID = 1,
            };

            // 1. Генерируем новый код (используем ReceiptCode как базу)
            int newReceiptCode = GenerateNewReceiptCode();
            CurrentOrder.ReceiptCode = newReceiptCode;

            // 2. Устанавливаем отображаемый артикул (например, строка с префиксом)
            CurrentOrderArticle = $"ORD-{newReceiptCode}";

            this.DataContext = this;
            LoadReferenceData();
        }

        public AddEditOrderPage(Orders orderToEdit)
        {
            InitializeComponent();

            _isNewOrder = false;

            // Явно загружаем сущность с навигационными свойствами
            CurrentOrder = _db.Orders
                               .Include(o => o.OrderStatuses) 
                               .Include(o => o.PickupPoints)                                                                
                               .Include(o => o.OrderProducts.Select(op => op.Products))
                               .FirstOrDefault(o => o.OrderID == orderToEdit.OrderID);

            if (CurrentOrder == null)
            {
                MessageBox.Show("Заказ не найден в базе данных.", "Ошибка");
                NavigationService.GoBack();
                return;
            }

            // --- Логика определения Артикула Заказа ---
            CurrentOrderArticle = CurrentOrder.OrderProducts
                                                            .FirstOrDefault()
                                                            ?.Products?.Article
                                                            ?? CurrentOrder.OrderID.ToString();

            PageTitle = $"Редактирование заказа №{CurrentOrderArticle}"; // Установка заголовка

            this.DataContext = this;
            LoadReferenceData();

            // Установка выбранных элементов в ComboBox (т.к. DataContext уже установлен)
            StatusComboBox.SelectedItem = CurrentOrder.OrderStatuses;
            PickupPointComboBox.SelectedItem = CurrentOrder.PickupPoints;
        }

        // Вспомогательный метод для генерации артикула
        private int GenerateNewReceiptCode()
        {
            // Пример: Берем текущий максимальный ID и увеличиваем
            int maxId = _db.Orders.Max(o => (int?)o.ReceiptCode) ?? 900;
            return maxId + 1;
        }

        private void LoadReferenceData()
        {
            StatusComboBox.ItemsSource = _db.OrderStatuses.ToList();
         
            PickupPointComboBox.ItemsSource = _db.PickupPoints.ToList();

            // Если это новый заказ, выбираем первый статус по умолчанию
            if (_isNewOrder && StatusComboBox.Items.Count > 0)
            {
                StatusComboBox.SelectedIndex = 0;
            }
        }

        private bool ValidateData()
        {
            if (StatusComboBox.SelectedItem == null || PickupPointComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите статус и пункт выдачи.", "Ошибка валидации");
                return false;
            }
            if (CurrentOrder.DeliveryDate == null || CurrentOrder.DeliveryDate < CurrentOrder.OrderDate)
            {
                MessageBox.Show("Дата выдачи должна быть указана и не может быть раньше даты заказа.", "Ошибка валидации");
                return false;
            }
            return true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateData()) return;

            try
            {
                // Устанавливаем выбранные объекты перед сохранением
                CurrentOrder.OrderStatusesID = (StatusComboBox.SelectedItem as OrderStatuses)?.StatusID ?? 1;
                CurrentOrder.PickupPointsID = (PickupPointComboBox.SelectedItem as PickupPoints)?.AddressID ?? 1;

                if (_isNewOrder)
                {
                    _db.Orders.Add(CurrentOrder);
                }

                _db.SaveChanges();
                MessageBox.Show("Данные заказа успешно сохранены!", "Успех");

                // Возвращаемся к списку заказов
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении заказа: {ex.Message}", "Ошибка сохранения");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}

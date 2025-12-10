using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.NetworkInformation;
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
    /// Логика взаимодействия для OrderListPage.xaml
    /// </summary>
    public partial class OrderListPage : Page
    {
        private List<OrderViewModel> _orders;
        private int _currentUser;

        public OrderListPage(int roleId)
        {
            InitializeComponent();

            _currentUser = roleId;
            SetupAccessControls();
        }

        private void SetupAccessControls()
        {
            // Управление доступом: только Администратор может добавлять и удалять
            if (_currentUser == 1)
            {
                AddOrderButton.Visibility = Visibility.Visible;
                DeleteOrderButton.Visibility = Visibility.Visible;
            }
            else // Если Менеджер
            {
                AddOrderButton.Visibility = Visibility.Collapsed;
                DeleteOrderButton.Visibility = Visibility.Collapsed;
            }
        }

        private void OrderListPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                using (var db = new DB_SP_SaleBootsEntities())
                {
                    _orders = db.Orders
                        .Include(o => o.PickupPoints)
                        .Include(o => o.OrderStatuses)
                        .Include(o => o.OrderProducts.Select(op => op.Products)) //Обращаемся к Products? чтобы взять Article
                        .ToList()
                        .Select(o =>
                        {
                            string productArticle = o.OrderProducts.FirstOrDefault()?.Products?.Article;

                            return new OrderViewModel
                            {
                                OrderID = o.OrderID,
                                OrderArticle = string.IsNullOrEmpty(productArticle)
                                                                ? $"ORD-{o.ReceiptCode}" // Форматируем ReceiptCode для новых заказов
                                                                : productArticle,         // Используем артикул продукта для старых
                                StatusName = o.OrderStatuses?.OrderStatus ?? "Неизвестно",
                                PickupPointAddress = o.PickupPoints?.Address ?? "Не указан",
                                OrderDate = o.OrderDate ?? DateTime.UtcNow,
                                DeliveryDate = o.DeliveryDate
                            };
                        }).OrderByDescending(o => o.OrderDate).ToList(); 
                    OrderListView.ItemsSource = _orders;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка БД");
            }
        }

        private void GoToProductsButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void AddOrderButton_Click(object sender, RoutedEventArgs e)
        {
            // Переход на форму добавления
            NavigationService.Navigate(new AddEditOrderPage());
        }

        private void OrderListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (OrderListView.SelectedItem is OrderViewModel selectedOrder)
            {
                // Переход на форму редактирования (только для Администратора)
                if (_currentUser == 1)
                {
                    try
                    {
                        using (var db = new DB_SP_SaleBootsEntities())
                        {
                            var orderToEdit = db.Orders
                                                .AsNoTracking()
                                                .FirstOrDefault(o => o.OrderID == selectedOrder.OrderID);

                            if (orderToEdit != null)
                            {
                                NavigationService.Navigate(new AddEditOrderPage(orderToEdit));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при загрузке заказа для редактирования: {ex.Message}", "Ошибка БД");
                    }
                }
                else
                {
                    MessageBox.Show("У вас нет прав для редактирования заказов.", "Доступ запрещен");
                }
            }
        }

        private void DeleteOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedOrderViewModel = OrderListView.SelectedItem as OrderViewModel;

            if (selectedOrderViewModel == null)
            {
                MessageBox.Show("Пожалуйста, выберите заказ для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить заказ №{selectedOrderViewModel.OrderArticle} (ID: {selectedOrderViewModel.OrderID})? Это действие нельзя отменить.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new DB_SP_SaleBootsEntities())
                    {
                        var orderToRemove = db.Orders.Find(selectedOrderViewModel.OrderID);

                        if (orderToRemove != null)
                        {
                            // 4. Проверяем связанные данные (OrderProducts)
                            // Используем локальный db
                            var productsToRemove = db.OrderProducts.Where(op => op.OrderID == orderToRemove.OrderID).ToList();
                            if (productsToRemove.Any())
                            {
                                db.OrderProducts.RemoveRange(productsToRemove);
                            }

                            db.Orders.Remove(orderToRemove);
                            db.SaveChanges();

                            MessageBox.Show("Заказ успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                            LoadOrders();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении заказа: {ex.Message}\nПроверьте, нет ли связанных данных (например, в логах или других таблицах), которые блокируют удаление.", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

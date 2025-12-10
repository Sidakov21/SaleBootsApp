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

namespace SaleBootsApp.AddOrder
{
    /// <summary>
    /// Логика взаимодействия для OrderListPage.xaml
    /// </summary>
    public partial class OrderListPage : Page
    {
        private List<OrderViewModel> _orders;
        private Users _currentUser; 

        public OrderListPage()
        {
            InitializeComponent();

            _currentUser = new Users { RoleID = 1 };
            SetupAccessControls();
        }

        private void SetupAccessControls()
        {
            // Управление доступом: только Администратор может добавлять и удалять
            if (_currentUser.RoleID == 1)
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
                        .Select(o => new OrderViewModel
                        {
                            OrderID = o.OrderID,

                            // Ищем первый элемент в коллекции, безопасно обращаемся к Product, затем к Article.
                            // Если OrderProducts пуст, используем OrderID как запасной вариант.
                            OrderArticle = o.OrderProducts
                                    .FirstOrDefault()
                                    ?.Products?.Article
                                    ?? o.OrderID.ToString(),

                            StatusName = o.OrderStatuses?.OrderStatus ?? "Неизвестно",
                            PickupPointAddress = o.PickupPoints?.Address ?? "Не указан",
                            OrderDate = o.OrderDate ?? DateTime.UtcNow,
                            DeliveryDate = o.DeliveryDate

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
                if (_currentUser.RoleID == 1)
                {
                    try
                    {
                        using (var db = new DB_SP_SaleBootsEntities())
                        {
                            var orderToEdit = db.Orders.FirstOrDefault(o => o.OrderID == selectedOrder.OrderID);

                            if (orderToEdit != null)
                            {
                                // !!! ВАЖНО: Отсоединяем, чтобы избежать ошибки "Multiple Change Tracker" !!!
                                db.Entry(orderToEdit).State = EntityState.Detached;

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
            
        }
    }
}

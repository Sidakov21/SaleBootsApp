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
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        public AuthPage()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль.", "Ошибка ввода");
                return;
            }

            try
            {
                
                using (var db = new DB_SP_SaleBootsEntities())
                {

                    var userEntity = db.Users
                                         .Include(u => u.Roles)
                                         .FirstOrDefault(u => u.Login == login && u.Password == password);

                    if (userEntity != null)
                    {
                        new CurrentUser(
                            userEntity.UserID,
                            userEntity.RoleID,
                            userEntity.FullName,
                            userEntity.Roles.RoleUser
                        );

                        MessageBox.Show($"Добро пожаловать, {userEntity.FullName} ({userEntity.Roles.RoleUser})!", "Успешный вход");

                        LoginTextBox.Text = "";
                        NavigationService.Navigate(new ProductListPage());
                    }
                    else
                    {
                        // Неверные учетные данные
                        MessageBox.Show("Неверный логин или пароль.", "Ошибка авторизации");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}", "Ошибка");
            }
        }

        private void GuestLoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Создаем объект Гостя
                new CurrentUser(
                    userId: 0,
                    roleId: CurrentUser.RoleGuestId,
                    fullName: "Гость",
                    roleName: "Гость"
                );

                LoginTextBox.Text = "";
                NavigationService.Navigate(new ProductListPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе как Гость: {ex.Message}", "Ошибка");
            }
        }
    }
}

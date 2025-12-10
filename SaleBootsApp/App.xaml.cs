using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SaleBootsApp
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    base.OnStartup(e);

        //    // 1. СИМУЛЯЦИЯ ВХОДА АДМИНИСТРАТОРА (для отладки)
        //    // Используем EmployeeID=1, RoleID=1, ФИО и роль "Администратор"
        //    CurrentUser.LoginAdminForDebug(
        //        userId: 1,
        //        roleId: CurrentUser.RoleAdminId,
        //        fullName: "Никифорова Весения Николаевна",
        //        roleName: "Администратор");

        //    // 2. Создаем главное окно
        //    MainWindow mainWindow = new MainWindow();

        //    // 3. Устанавливаем в качестве содержимого окна ProductListPage, 
        //    // обернутую в Frame для корректной работы NavigationService
        //    Frame mainFrame = new Frame();
        //    mainFrame.Content = new ProductListPage();
        //    mainWindow.Content = mainFrame;

        //    mainWindow.Show();
        //}
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaleBootsApp
{
    public class CurrentUser
    {
        public const int RoleAdminId = 1;
        public const int RoleManagerId = 2;
        public const int RoleClientId = 3;
        public const int RoleGuestId = 4; // Определяем ID для Гостя (не хранится в БД)

        public int UserId { get; }
        public int RoleId { get; }
        public string FullName { get; }
        public string RoleName { get; }

        // Статический объект для доступа к текущему пользователю из любой точки приложения
        public static CurrentUser Instance { get; private set; }

        public CurrentUser(int userId, int roleId, string fullName, string roleName)
        {
            UserId = userId;
            RoleId = roleId;
            FullName = fullName;
            RoleName = roleName;
            Instance = this; // Установка текущего пользователя при создании
        }

        public static void LoginAdminForDebug(int userId, int roleId, string fullName, string roleName)
        {
            // Инициализация объекта Instance с данными Администратора
            new CurrentUser(userId, roleId, fullName, roleName);
        }

        public static void Logout()
        {
            Instance = null;
        }
    }
}


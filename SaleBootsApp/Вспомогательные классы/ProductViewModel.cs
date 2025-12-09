using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaleBootsApp
{
    public class ProductViewModel
    {
        // Основные поля для отображения
        public string ProductArticle { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }
        public string ManufacturerName { get; set; }
        public string SupplierName { get; set; }
        public decimal Discount { get; set; }
        public int QuantityInStock { get; set; }
        public string UnitsName { get; set; }
        public string PhotoPath { get; set; } // Путь к файлу (например, "1.jpg" или null)


        public decimal OriginalPrice { get; set; }
        public decimal FinalPrice { get; set; }


        // 1. Нужна ли перечеркнутая цена? (Если скидка > 0)
        public bool IsDiscounted => Discount > 0;

        // 2. Скидка больше 15%? (Для выделения цветом #2E8B57)
        public bool IsHighDiscount => Discount > 15;

        // 3. Нет на складе? (Для выделения голубым)
        public bool IsOutOfStock => QuantityInStock == 0;

        // 4. Путь к отображаемому фото (для заглушки)
        public string DisplayPhotoPath
        {
            get
            {
                const string DefaultPath = "Resources/picture.png";
                if (string.IsNullOrEmpty(PhotoPath))
                {
                    return DefaultPath;
                }

                // Строим полный, абсолютный путь к файлу
                // Мы предполагаем, что файлы лежат в подпапке ProductPhotos в каталоге, откуда запущено приложение.
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string fullPath = System.IO.Path.Combine(appDir, "Resources", "Продукты", PhotoPath);

                // Проверяем, существует ли файл по полному пути
                if (System.IO.File.Exists(fullPath))
                {
                    return fullPath;
                }

                return $"/Resources/Продукты/{PhotoPath}";
            }
        }

        // Метод для расчета цены (Удобно вызывать из Linq)
        public static decimal CalculateFinalPrice(decimal originalPrice, decimal discountPercent)
        {
            if (discountPercent <= 0)
            {
                return originalPrice;
            }
            return originalPrice * (1M - (decimal)discountPercent / 100M);
        }
    }
}

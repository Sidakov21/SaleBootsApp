using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaleBootsApp.AddOrder
{
    public class OrderViewModel
    {
        // Поля для отображения
        public int OrderID { get; set; }
        public string OrderArticle { get; set; }
        public string StatusName { get; set; }
        public string PickupPointAddress { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
    }
}

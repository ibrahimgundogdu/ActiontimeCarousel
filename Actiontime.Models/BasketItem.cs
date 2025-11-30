using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Models
{
    public class BasketItem
    {
        public int id { get; set; }
        public int locationId { get; set; }
        public int employeeId { get; set; }
        public int priceCategoryId { get; set; }
        public int priceId { get; set; }
        public int productId { get; set; }
        public string date { get; set; }
        public int quantity { get; set; }
        public int unit { get; set; }
        public double price { get; set; }
        public double taxRate { get; set; }
        public double tax { get; set; }
        public double discount { get; set; }
        public double total { get; set; }
        public string currency { get; set; }
        public int? promotionId { get; set; }
        public bool? isPromotion { get; set; }
        public int recordEmployeeId { get; set; }
        public string recordDate { get; set; }
        public double masterPrice { get; set; }
        public double promoPrice { get; set; }
        public double totalPrice { get; set; }
    }


}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class CUPrice
    {
        public string Price { get; set; }
        public string DateBegin { get; set; }
        public string DateBeginHour { get; set; }
        public string UseSale { get; set; }
        public string Active { get; set; }
        public string CompanyName { get; set; }
        public string TicketTypeName { get; set; }
        public string CategoryName { get; set; }
        public string ProductName { get; set; }
        public string Unit { get; set; }
        public int? ID { get; set; }
    }

    public class CUAjPrice
    {
        public string _price { get; set; }
        public string _datebegin { get; set; }
        public string _datebeginhour { get; set; }
        public int? _usesale { get; set; }
        public int? _isactive { get; set; }
        public int? id { get; set; }
        public int? _typeid { get; set; }
        public int? _catid { get; set; }
    }

    public class CUNewPrice
    {
        public string IsSelected { get; set; }
        public int? productID { get; set; }
        public int? categoryID { get; set; }
        public string Price { get; set; }
        public string UseSale { get; set; }
        public string IsActive { get; set; }
    }
}
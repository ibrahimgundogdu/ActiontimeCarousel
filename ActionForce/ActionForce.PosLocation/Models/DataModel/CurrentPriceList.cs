using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    //dapper örnek için hazırlandı
    public class CurrentPriceList
    {
        public int ID { get; set; }
        public int OurCompanyID { get; set; }
        public int PriceCategoryID { get; set; }
        public int TicketTypeID { get; set; }
        public int ProductID { get; set; }
        public float Price { get; set; }
        public string Currency { get; set; }
        public float ExtraMultiple { get; set; }
        public DateTime StartDate { get; set; }
        public bool UseToSale { get; set; }
        public bool IsActive { get; set; }
        public string TicketTypeName { get; set; }
        public string CompanyName { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public string ProductCategoryName { get; set; }
        public string ProductName { get; set; }
        public string Style { get; set; }
        public string Sign { get; set; }
        public string BackStyle { get; set; }
        public string Description { get; set; }
        public int Unit { get; set; }
        public float TaxRate { get; set; }
    }



}
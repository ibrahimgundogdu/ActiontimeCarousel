using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class FormLocationPriceCategory
    {
        public int ID { get; set; }
        public int LocationID { get; set; }
        public int PriceCategoryID { get; set; }
        public int MondayPCID { get; set; }
        public int TuesdayPCID { get; set; }
        public int WednesdayPCID { get; set; }
        public int ThursdayPCID { get; set; }
        public int FridayPCID { get; set; }
        public int SaturdayPCID { get; set; }
        public int SundayPCID { get; set; }
        public DateTime StartDate { get; set; }
    }
}
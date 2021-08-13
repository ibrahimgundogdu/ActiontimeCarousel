using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.PosLocation.Controllers
{
    public class SalesController : BaseController
    {
        // GET: Sales
        public ActionResult Index(string id)
        {
            SalesControlModel model = new SalesControlModel();

            DateTime date = DateTime.UtcNow.AddHours(this.AuthenticationData.CurrentLocation.TimeZone).Date;

            var OrderNumber = string.Format("S{0}{1}{2}{3}", date.Year.ToString().Substring(2, 2), date.Month < 10 ? "0" + date.Month.ToString() : date.Month.ToString(), date.Day < 10 ? "0" + date.Day.ToString() : date.Day.ToString(), DateTime.Now.Ticks.ToString());

            var orderID = Db.AddOrder(model.Authentication.CurrentLocation.ID, model.Authentication.CurrentEmployee.EmployeeID, OrderNumber, neworder.OrderFullName, neworder.OrderDetail, neworder.OrderMobile, neworder.OrderIdentity, neworder.OrderDescription, LocationHelper.GetIPAddress());

            LocationHelper.AddApplicationLog("Location", "TicketSale", "Insert", orderID.ToString(), "Default", "CreateOrder", null, true, $"{OrderNumber} Nolu Sipariş Eklendi", string.Empty, DateTime.UtcNow.AddHours(model.Location.TimeZone), $"{model.Authentication.CurrentEmployee.EmployeeID} {model.Authentication.CurrentEmployee.FullName}", LocationHelper.GetIPAddress(), string.Empty, null);


            return View(model);
        }
    }
}
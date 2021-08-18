using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.PosLocation.Controllers
{
    public class TransactionController : BaseController
    {
        // GET: Transaction
        public ActionResult Index(long? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Default");
            }

            TransactionControlModel model = new TransactionControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            var order = Db.TicketSale.FirstOrDefault(x => x.ID == id);
            model.Order = order;

            return View(model);
        }

        //SaveOrder
        public ActionResult SaveOrder()
        {
            TransactionControlModel model = new TransactionControlModel();
            model.Authentication = this.AuthenticationData;
            long? orderID = null;

            DateTime date = DateTime.UtcNow.AddHours(this.AuthenticationData.CurrentLocation.TimeZone).Date;
            try
            {
                var OrderNumber = "S" + Db.GetPosOrderNumber().FirstOrDefault().ToString();

                orderID = Db.AddPosOrder(model.Authentication.CurrentLocation.ID, model.Authentication.CurrentEmployee.EmployeeID, OrderNumber, null, null, null, null, null, PosManager.GetIPAddress()).FirstOrDefault();

                model.Result.IsSuccess = true;
                model.Result.Message = $"{orderID} ID'li Sipariş Eklendi";

                var clean = Db.CleanBasket(model.Authentication.CurrentLocation.ID, model.Authentication.CurrentEmployee.EmployeeID);

                PosManager.AddApplicationLog("Location", "TicketSale", "Insert", orderID.ToString(), "Default", "CreateOrder", null, true, model.Result.Message, string.Empty, DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone), $"{model.Authentication.CurrentEmployee.EmployeeID} {model.Authentication.CurrentEmployee.FullName}", PosManager.GetIPAddress(), string.Empty, null);

                TempData["Result"] = model.Result;

                return RedirectToAction("Index", new { id = orderID });
            }
            catch (Exception ex)
            {
                model.Result.IsSuccess = false;
                model.Result.Message = $"Sipariş Eklenemedi. Hata :" + ex.Message;
                TempData["Result"] = model.Result;
                return RedirectToAction("Index", "Default");
            }

        }
    }
}
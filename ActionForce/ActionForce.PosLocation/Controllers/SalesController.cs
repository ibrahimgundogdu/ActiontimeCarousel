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
        public ActionResult Index()
        {
            SalesControlModel model = new SalesControlModel();
            model.Authentication = this.AuthenticationData;

            var DocumentDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone).Date;

            model.SaleSummary = Db.VTicketSaleSummary.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.Date == DocumentDate).ToList();

            
            return View(model);
        }

        public ActionResult Detail(long? id)
        {
            if (id <= 0 || id == null)
            {
                return RedirectToAction("Index");
            }

            SalesControlModel model = new SalesControlModel();
            model.Authentication = this.AuthenticationData;

           model.DocumentDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone).Date;

            model.TicketSaleSummary = Db.VTicketSaleSummary.FirstOrDefault(x => x.ID == id);
            model.TicketSale = Db.TicketSale.FirstOrDefault(x => x.ID == id);
            model.TicketSaleRows = Db.VTicketSaleRowsAll.Where(x => x.OrderID == id).ToList();
            model.TicketSalePosPayment = Db.TicketSalePosPayment.Where(x => x.SaleID == id).ToList();
            model.TicketSalePosStatus = Db.TicketSalePosStatus.Where(x => x.IsActive == true).ToList();
            model.PosPaymentType = Db.PosPaymentType.ToList();
            model.PosPaymentSubType = Db.PosPaymentSubType.ToList();
            model.Environments = Db.Environment.ToList();
            model.Currencys = Db.Currency.ToList();
            model.TicketSalePosPaymentSummary = Db.VTicketSalePosPaymentSummary.Where(x => x.SaleID == id).ToList();

            return View(model);
        }
    }
}